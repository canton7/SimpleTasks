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
        void AddOption(Command command, Action<object?> handler);
    }

    internal class InvocationArg<T> : IInvocationArg
    {
        private readonly ParameterInfo parameterInfo;
        public int Index { get; }
        public string Name => this.parameterInfo.Name;

        public object? DefaultValue { get; }
        public bool IsOptional { get; }

        public InvocationArg(ParameterInfo parameterInfo, int argIndex)
        {
            this.parameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            this.Index = argIndex;

            if (this.parameterInfo.HasDefaultValue)
            {
                this.DefaultValue = this.parameterInfo.DefaultValue;
                this.IsOptional = true;
            }
            else if (Nullable.GetUnderlyingType(this.parameterInfo.ParameterType) != null ||
                (!this.parameterInfo.ParameterType.IsValueType && IsNullable(this.parameterInfo)))
            {
                this.DefaultValue = null;
                this.IsOptional = true;
            }
            else if (this.parameterInfo.ParameterType == typeof(bool))
            {
                this.DefaultValue = false;
                this.IsOptional = true;
            }
            else
            {
                this.DefaultValue = null;
                this.IsOptional = false;
            }
        }

        public void AddOption(Command command, Action<object?> handler)
        {
            var attribute = this.parameterInfo.GetCustomAttribute<OptionAttribute>();
            string name = this.Name;
            string? description = null;
            if (attribute != null)
            {
                name = attribute.Name ?? name;
                description = attribute.Description;
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

        private static bool IsNullable(ParameterInfo parameterInfo)
        {
            var nullable = parameterInfo.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
            if (nullable != null && nullable.ConstructorArguments.Count == 1)
            {
                var attributeArgument = nullable.ConstructorArguments[0];
                if (attributeArgument.ArgumentType == typeof(byte[]))
                {
                    var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value;
                    if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
                    {
                        return (byte)args[0].Value == 2;
                    }
                }
                else if (attributeArgument.ArgumentType == typeof(byte))
                {
                    return (byte)attributeArgument.Value == 2;
                }
            }

            var context = parameterInfo.Member.CustomAttributes
                .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context != null &&
                context.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
            {
                return (byte)context.ConstructorArguments[0].Value == 2;
            }

            // Couldn't find a suitable attribute
            return false;
        }
    }

}
