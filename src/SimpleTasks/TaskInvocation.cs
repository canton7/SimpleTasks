using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace SimpleTasks
{
    internal class TaskInvocation
    {
        public SimpleTask Task { get; }
        public Command Command { get; }

        private readonly object?[] argValues;
        private readonly bool[] argSupplied;

        public TaskInvocation(SimpleTask task)
        {
            this.Task = task ?? throw new ArgumentNullException(nameof(task));

            this.argValues = new object[task.Invocation!.Args.Count];
            this.argSupplied = new bool[task.Invocation.Args.Count];

            for (int i = 0; i < task.Invocation.Args.Count; i++)
            {
                this.argValues[i] = task.Invocation.Args[i].DefaultValue;
                this.argSupplied[i] = task.Invocation.Args[i].HasDefaultValue;
            }

            var command = new Command(task.Name, task.Description)
            {
                Options = new OptionSet()
            };
            command.Options.Add("help|h", "Show help", _ => command.Options.WriteOptionDescriptions(Console.Out), hidden: true);
            foreach (var taskArg in task.Invocation.Args)
            {
                taskArg.AddOption(command, x =>
                {
                    this.argValues[taskArg.Index] = x;
                    this.argSupplied[taskArg.Index] = true;
                });
            }

            this.Command = command;
        }

        public IEnumerable<string> GetMissingArguments()
        {
            for (int i = 0; i < this.Task.Invocation!.Args.Count; i++)
            {
                if (!this.argSupplied[i])
                {
                    yield return this.Task.Invocation.Args[i].Name;
                }
            }
        }

        public void Invoke()
        {
            this.Task.Invocation!.Invoke(this.argValues);
        }
    }
}
