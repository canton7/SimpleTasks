using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace SimpleTasks
{
    public class SimpleTask
    {
        public static SimpleTaskSet DefaultSet { get; } = new SimpleTaskSet();

        public string Name { get; }
        public string? Description { get; }
        public IReadOnlyList<SimpleTask> Dependencies { get; private set; } = Array.Empty<SimpleTask>();
        internal RunMethodInvoker? Invocation { get; private set; }

        internal SimpleTask(string name, string? description)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Description = description;
        }

        public SimpleTask DependsOn(params SimpleTask[] dependencies)
        {
            if (dependencies == null || dependencies.Contains(null!))
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            this.Dependencies = dependencies;
            return this;
        }

        public void Run(Action action) => this.Invocation = new RunMethodInvoker(action);
        public void Run<T>(Action<T> action) => this.Invocation = new RunMethodInvoker(action).Arg<T>();
        public void Run<T1, T2>(Action<T1, T2> action) => this.Invocation = new RunMethodInvoker(action).Arg<T1>().Arg<T2>();
    }
}
