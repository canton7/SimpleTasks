using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Options;

namespace SimpleTasks
{
    internal interface IInvocationArg
    {
        int Index { get; }
        string Name { get; }
        object? DefaultValue { get; }
        bool HasDefaultValue { get; }
        void AddOption(Command command, Action<object?> handler);
    }

    internal class InvocationArg<T> : IInvocationArg
    {
        private readonly ParameterInfo parameterInfo;
        public int Index { get; }
        public string Name => this.parameterInfo.Name;

        public object? DefaultValue => this.parameterInfo.DefaultValue;
        public bool HasDefaultValue => this.parameterInfo.HasDefaultValue;

        public InvocationArg(ParameterInfo parameterInfo, int argIndex)
        {
            this.parameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            this.Index = argIndex;
        }

        public void AddOption(Command command, Action<object?> handler)
        {
            command.Options.Add(this.Name + "=", new Action<T>(x => handler(x)));
        }
    }

}
