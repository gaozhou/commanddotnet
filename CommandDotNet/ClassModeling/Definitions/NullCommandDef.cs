﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace CommandDotNet.ClassModeling.Definitions
{
    internal class NullCommandDef : ICommandDef
    {
        public NullCommandDef(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Type CommandHostClassType { get; } = null;
        public ICustomAttributeProvider CustomAttributeProvider => null;
        public bool IsExecutable => false;
        public bool HasInterceptor => false;
        public IReadOnlyCollection<ICommandDef> SubCommands => new List<ICommandDef>().AsReadOnly();
        public IMethodDef InterceptorMethodDef { get; } = NullMethodDef.Instance;
        public IMethodDef InvokeMethodDef { get; } = NullMethodDef.Instance;
        public Command Command { get; set; }
    }
}