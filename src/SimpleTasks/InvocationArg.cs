using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Mono.Options;

namespace SimpleTasks
{
    internal interface IInvocationArg
    {
        int Index { get; }
        string Name { get; }
        object? DefaultValue { get; }
        bool IsOptional { get; }
        void AddOption(Mono.Options.Command command, Action<object?> handler);
    }

    internal class InvocationArg<T> : IInvocationArg
    {
        private readonly ParameterInfo parameterInfo;
        public int Index { get; }
        public string Name { get; }

        public object? DefaultValue { get; }
        public bool IsOptional { get; }

        public InvocationArg(ParameterInfo parameterInfo, int argIndex)
        {
            this.parameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            this.Index = argIndex;

            if (this.parameterInfo.Name.EndsWith("Opt"))
            {
                this.Name = this.parameterInfo.Name.Substring(0, this.parameterInfo.Name.Length - "Opt".Length);
                this.IsOptional = true;
            }
            else
            {
                this.Name = this.parameterInfo.Name;
            }

            if (this.parameterInfo.HasDefaultValue)
            {
                this.DefaultValue = this.parameterInfo.DefaultValue;
                this.IsOptional = true;
            }
            else if (Nullable.GetUnderlyingType(this.parameterInfo.ParameterType) != null)
            {
                this.IsOptional = true;
            }
            else if (this.parameterInfo.ParameterType == typeof(bool))
            {
                this.DefaultValue = false;
                this.IsOptional = true;
            }
        }

        public void AddOption(Mono.Options.Command command, Action<object?> handler)
        {
            var attribute = this.parameterInfo.GetCustomAttribute<OptionAttribute>();
            string name = this.Name;
            string description = string.Empty;
            if (attribute != null)
            {
                name = attribute.Name ?? name;
                description = attribute.Description ?? string.Empty;
            }


            if (this.IsOptional)
            {
                description = "(optional) " + description;
            }

            if (this.parameterInfo.ParameterType == typeof(bool))
            {
                command.Options.Add(name, description, new Action<string>(x => handler(x != null)));
            }
            else
            {
                command.Options.Add(name + "=", description, new Action<T>(x => handler(x)));
            }
        }
    }
}
