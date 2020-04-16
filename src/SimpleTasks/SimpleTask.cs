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
        internal List<SimpleTask> Prerequisites { get; } = new List<SimpleTask>();
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
            foreach (var dependency in dependencies)
            {
                dependency.Prerequisites.Add(this);
            }
            return this;
        }

        public SimpleTask Run(Action action)
        {
            this.Invocation = new RunMethodInvoker(action);
            return this;
        }
        public SimpleTask Run<T>(Action<T> action)
        {
            this.Invocation = new RunMethodInvoker(action).Arg<T>();
            return this;
        }
        public SimpleTask Run<T1, T2>(Action<T1, T2> action)
        {
            this.Invocation = new RunMethodInvoker(action).Arg<T1>().Arg<T2>();
            return this;
        }
    }
}
