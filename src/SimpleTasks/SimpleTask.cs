using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTasks
{
    public class SimpleTask
    {
        public static SimpleTaskSet DefaultSet { get; } = new SimpleTaskSet();

        public string Name { get; }
        public string? Description { get; }
        private readonly List<string> dependencies = new List<string>();
        public IReadOnlyList<string> Dependencies => this.dependencies;
        internal RunMethodInvoker? Invoker { get; private set; }

        internal SimpleTask(string name, string? description)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Description = description;
        }

        public SimpleTask DependsOn(params string[] dependencies)
        {
            if (dependencies == null || dependencies.Contains(null!))
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            this.dependencies.AddRange(dependencies);
            return this;
        }

        public SimpleTask DependsOn(params SimpleTask[] dependencies)
        {
            if (dependencies == null || dependencies.Contains(null!))
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            this.dependencies.AddRange(dependencies.Select(x => x.Name));
            return this;
        }

        public SimpleTask Run(Action action)
        {
            this.Invoker = new RunMethodInvoker(action);
            return this;
        }
        public SimpleTask Run<T>(Action<T> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T>();
            return this;
        }
        public SimpleTask Run<T1, T2>(Action<T1, T2> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>();
            return this;
        }

        public static SimpleTask Create(string name, string? description = null) =>
            DefaultSet.Create(name, description);
        public static void Invoke(params string[] args) =>
            DefaultSet.InvokeAdvanced(args);
    }
}
