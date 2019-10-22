﻿// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// copied & adapted from System.CommandLine 

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CommandDotNet.Extensions;
using CommandDotNet.Prompts;
using CommandDotNet.Rendering;

namespace CommandDotNet.TestTools
{
    /// <summary>A test console that can be used to capture all output and to provide input for ReadLine and ReadToEnd</summary>
    public class TestConsole : IConsole
    {
        private readonly Func<TestConsole, ConsoleKeyInfo> _onReadKey;

        public TestConsole(
            Func<TestConsole, string> onReadLine = null,
            IEnumerable<string> pipedInput = null,
            Func<TestConsole, ConsoleKeyInfo> onReadKey = null)
        {
            _onReadKey = onReadKey;
            IsInputRedirected = pipedInput != null;

            if (pipedInput != null)
            {
                if (onReadLine != null)
                {
                    throw new Exception($"{nameof(onReadLine)} and {nameof(pipedInput)} cannot both be specified. " +
                                        "Windows will throw 'System.IO.IOException: The handle is invalid' on an attempt to ");
                }

                if (pipedInput is ICollection<string> inputs)
                {
                    var queue = new Queue<string>(inputs);
                    onReadLine = console => queue.Count == 0 ? null : queue.Dequeue();
                }
                else
                {
                    onReadLine = console => pipedInput.Take(1).FirstOrDefault();
                }
            }

            var joined = new StandardStreamWriter();
            Joined = joined;
            Out = new StandardStreamWriter(joined);
            Error = new StandardStreamWriter(joined);
            In = new StandardStreamReader(
                () =>
                {
                    var input = onReadLine?.Invoke(this);
                    // write to joined output so it can be logged for debugging
                    joined.WriteLine();
                    joined.WriteLine($"IConsole.ReadLine > {input}");
                    joined.WriteLine();
                    return input;
                });
        }

        public IStandardStreamWriter Error { get; }

        public IStandardStreamWriter Out { get; }

        public string OutLastLine => Out.ToString().SplitIntoLines().Last();

        /// <summary>
        /// This is the combined output for <see cref="Error"/> and <see cref="Out"/> in the order the lines were output.
        /// </summary>
        public IStandardStreamWriter Joined { get; }

        public bool IsOutputRedirected { get; } = false;

        public bool IsErrorRedirected { get; } = false;

        public IStandardStreamReader In { get; }

        public bool IsInputRedirected { get; }

        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            ConsoleKeyInfo consoleKeyInfo;

            do
            {
                consoleKeyInfo = _onReadKey?.Invoke(this)
                                 ?? new ConsoleKeyInfo('\u0000', ConsoleKey.Enter, false, false, false);

                // mimic System.Console which does not interrupt during ReadKey
                // and does not return Ctrl+C unless TreatControlCAsInput == true.
            } while (!TreatControlCAsInput && consoleKeyInfo.IsCtrlC());

            if (!intercept)
            {
                if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    Out.WriteLine("");
                }
                else
                {
                    Out.Write(consoleKeyInfo.KeyChar.ToString());
                }
            }
            return consoleKeyInfo;
        }
        public bool TreatControlCAsInput { get; set; }

        private class StandardStreamReader : IStandardStreamReader
        {
            private readonly Func<string> _onReadLine;

            public StandardStreamReader(Func<string> onReadLine)
            {
                _onReadLine = onReadLine;
            }

            public string ReadLine()
            {
                return _onReadLine?.Invoke();
            }

            public string ReadToEnd()
            {
                return _onReadLine.EnumerateLinesUntilNull().ToCsv(Environment.NewLine);
            }
        }

        private class StandardStreamWriter : TextWriter, IStandardStreamWriter
        {
            private readonly StandardStreamWriter _inner;
            private readonly StringBuilder _stringBuilder = new StringBuilder();

            public StandardStreamWriter(
                StandardStreamWriter inner = null)
            {
                _inner = inner;
            }

            public override void Write(char value)
            {
                _inner?.Write(value);
                if (value == '\b' && _stringBuilder.Length > 0)
                {
                    _stringBuilder.Length = _stringBuilder.Length - 1;
                }
                else
                {
                    _stringBuilder.Append(value);
                }
            }

            public override Encoding Encoding { get; } = Encoding.Unicode;

            public override string ToString() => _stringBuilder.ToString();
        }
    }
}