using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace SimpleTasks
{
    internal class RunMethodInvoker
    {
        private readonly Delegate @delegate;
        internal List<IInvocationArg> Args { get; } = new List<IInvocationArg>();
        private int argIndex = 0;

        public RunMethodInvoker(Delegate @delegate)
        {
            this.@delegate = @delegate ?? throw new ArgumentNullException(nameof(@delegate));
        }

        public RunMethodInvoker Arg<T>()
        {
            var parameterInfo = this.@delegate.Method.GetParameters()[this.argIndex];
            this.Args.Add(new InvocationArg<T>(parameterInfo, this.argIndex));
            this.argIndex++;
            return this;
        }

        public void Invoke(object?[] argValues)
        {
            try
            {
                this.@delegate.DynamicInvoke(argValues);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
        }
    }
}
