using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleTasks
{
    public class SimpleTaskException : Exception
    {
        public SimpleTaskException(string message) : base(message)
        {
        }
    }

    public class CircularDependencyException : SimpleTaskException
    {
        public IReadOnlyList<SimpleTask> Tasks { get; }

        internal CircularDependencyException(IReadOnlyList<SimpleTask> tasks)
            : base($"Recursive dependency found: {string.Join(" -> ", tasks.Select(x => x.Name))}")
        {
            this.Tasks = tasks;
        }
    }
}
