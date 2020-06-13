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

    public class SimpleTaskDuplicateTaskNameException : SimpleTaskException
    {
        public string TaskName { get; }

        public SimpleTaskDuplicateTaskNameException(string taskName)
            : base($"Multiple tasks with the name \"{taskName}\" found")
        {
            this.TaskName = taskName;
        }
    }

    public class SimpleTaskHasNoInvocationException : SimpleTaskException
    {
        public string TaskName { get; }

        public SimpleTaskHasNoInvocationException(string taskName)
            : base($"Task \"{taskName}\" missing a call to .Run(...)")
        {
            this.TaskName = taskName;
        }
    }

    public class SimpleTaskMissingOptionsException : SimpleTaskException
    {
        public IReadOnlyList<string> Options { get; }

        public SimpleTaskMissingOptionsException(List<IGrouping<string, SimpleTask>> groups)
            : base($"Missing option{(groups.Count == 1 ? "" : "s")}: " +
                  $"{string.Join(", ", groups.Select(FormatSingle))}")
        {
            this.Options = groups.Select(x => x.Key).ToList();
        }

        private static string FormatSingle(IGrouping<string, SimpleTask> group) =>
            $"\"{group.Key}\" (required by {string.Join(", ", group.Select(x => $"\"{x.Name}\""))})";
    }

    public class SimpleTaskUnknownOptionsException : SimpleTaskException
    {
        public IReadOnlyList<string> Options { get; }

        public SimpleTaskUnknownOptionsException(List<string> options)
            : base($"Unknown option{(options.Count == 1 ? "" : "s")}: " +
                  $"{string.Join(", ", options.Select(x => $"\"{x}\""))}")
        {
            this.Options = options;
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
