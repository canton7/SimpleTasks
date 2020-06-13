using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace SimpleTasks
{
    public enum TaskInvocationMark
    {
        None,
        Temporary,
        Permanent,
    }

    internal class TaskInvocation
    {
        public SimpleTask Task { get; }
        public Command Command { get; }
        public TaskInvocationMark Mark { get; set; }

        public List<TaskInvocation> Prerequisites { get; } = new List<TaskInvocation>();

        private readonly object?[] argValues;
        private readonly bool[] argSupplied;

        public TaskInvocation(SimpleTask task)
        {
            this.Task = task ?? throw new ArgumentNullException(nameof(task));

            this.argValues = new object[task.Invoker!.Args.Count];
            this.argSupplied = new bool[task.Invoker.Args.Count];

            for (int i = 0; i < task.Invoker.Args.Count; i++)
            {
                this.argValues[i] = task.Invoker.Args[i].DefaultValue;
                this.argSupplied[i] = task.Invoker.Args[i].IsOptional;
            }

            var command = new Command(task.Name, task.Description)
            {
                Options = new OptionSet()
            };
            command.Options.Add("help|h", "Show help", _ => command.Options.WriteOptionDescriptions(Console.Out), hidden: true);
            foreach (var taskArg in task.Invoker.Args)
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
            for (int i = 0; i < this.Task.Invoker!.Args.Count; i++)
            {
                if (!this.argSupplied[i])
                {
                    yield return this.Task.Invoker.Args[i].Name;
                }
            }
        }

        public void Invoke()
        {
            this.Task.Invoker!.Invoke(this.argValues);
        }
    }
}
