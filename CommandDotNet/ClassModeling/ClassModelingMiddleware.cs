﻿using System;
using System.Linq;
using System.Threading.Tasks;
using CommandDotNet.ClassModeling.Definitions;
using CommandDotNet.Execution;
using CommandDotNet.Parsing;

namespace CommandDotNet.ClassModeling
{
    internal static class ClassModelingMiddleware
    {
        internal static AppBuilder UseClassDefMiddleware<TRootCommandType>(this AppBuilder appBuilder)
        {
            appBuilder.AddMiddlewareInStage(BuildMiddleware<TRootCommandType>, MiddlewareStages.Build);
            appBuilder.AddMiddlewareInStage(SetInvocationContextMiddleware, MiddlewareStages.ParseInput);
            appBuilder.AddMiddlewareInStage(SetValuesMiddleware, MiddlewareStages.BindValues);
            appBuilder.AddMiddlewareInStage(CreateInstancesMiddleware, MiddlewareStages.BindValues);
            appBuilder.AddMiddlewareInStage(InvokeCommandDefMiddleware, MiddlewareStages.Invoke, int.MaxValue);
            return appBuilder;
        }

        private static Task<int> BuildMiddleware<TRootCommandType>(CommandContext commandContext, Func<CommandContext, Task<int>> next)
        {
            commandContext.CurrentCommand = ClassCommandDef.CreateRootCommand(typeof(TRootCommandType), commandContext);
            return next(commandContext);
        }

        private static Task<int> SetInvocationContextMiddleware(CommandContext commandContext, Func<CommandContext, Task<int>> next)
        {
            var commandDef = commandContext.CurrentCommand.ContextData.Get<ICommandDef>();
            if (commandDef != null)
            {
                var ctx = commandContext.InvocationContext;
                ctx.InstantiateInvocation = commandDef.InstantiateMethodDef;
                ctx.CommandInvocation = commandDef.InvokeMethodDef;
            }

            return next(commandContext);
        }

        private static Task<int> SetValuesMiddleware(CommandContext commandContext, Func<CommandContext, Task<int>> next)
        {
            var commandDef = commandContext.CurrentCommand.ContextData.Get<ICommandDef>();
            if (commandDef != null)
            {
                var argumentValues = commandContext.ParseResult.ArgumentValues;
                var parserFactory = new ParserFactory(commandContext.AppSettings);

                // TODO: move to Context object
                var instantiateArgs = commandDef.InstantiateMethodDef.ArgumentDefs;
                var invokeArgs = commandDef.InvokeMethodDef.ArgumentDefs;
                foreach (var argumentDef in instantiateArgs.Union(invokeArgs))
                {
                    if (argumentValues.TryGetValues(argumentDef.Argument, out var values))
                    {
                        var parser = parserFactory.CreateInstance(argumentDef.Argument);
                        var value = parser.Parse(argumentDef.Argument, values);
                        argumentDef.SetValue(value);
                    }
                    else if (argumentDef.HasDefaultValue)
                    {
                        argumentDef.SetValue(argumentDef.DefaultValue);
                    }
                }
            }
            return next(commandContext);
        }

        private static async Task<int> CreateInstancesMiddleware(CommandContext commandContext, Func<CommandContext, Task<int>> next)
        {
            var command = commandContext.ParseResult.Command;
            var commandDef = command.ContextData.Get<ICommandDef>();

            if (commandDef != null)
            {
                commandContext.InvocationContext.Instance = commandDef.InstantiateMethodDef.Invoke(null);
            }

            try
            {
                return await next(commandContext);
            }
            finally
            {
                // TODO: remove this when the instance is managed by DI
                //       and we can move creation of instance into an
                //       internal implementation of IDependencyResolver
                if (commandContext.InvocationContext.Instance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }

        private static Task<int> InvokeCommandDefMiddleware(CommandContext commandContext, Func<CommandContext, Task<int>> next)
        {
            var ctx = commandContext.InvocationContext;

            var result = ctx.CommandInvocation.Invoke(ctx.Instance);
            return result.GetResultCodeAsync(commandContext);
        }
    }
}