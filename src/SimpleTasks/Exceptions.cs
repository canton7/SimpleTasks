using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Options;

namespace SimpleTasks
{
    public class SimpleTaskException : Exception
    {
        public SimpleTaskException(string message) : base(message)
        {
        }
        public SimpleTaskException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public class SimpleTaskNoTaskNameSpecifiedException : SimpleTaskException
    {
        public SimpleTaskNoTaskNameSpecifiedException()
            : base("No task name to run specified (and no task called \"default\" was defined)")
        {
        }
    }

    public class SimpleTaskNotFoundException : SimpleTaskException
    {
        public string TaskName { get; }

        public SimpleTaskNotFoundException(string taskName)
            : base($"Unable to find task \"{taskName}\"")
        {
            this.TaskName = taskName;
        }
    }

    public class SimpleTaskDependencyNotFoundException : SimpleTaskException
    {
        public SimpleTask Task { get; }
        public string DependencyName { get; }

        public SimpleTaskDependencyNotFoundException(SimpleTask task, string dependencyName)
            : base($"Task \"{task.Name}\": unable to find dependency \"{dependencyName}\"")
        {
            this.Task = task;
            this.DependencyName = dependencyName;
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
        public SimpleTask Task { get; }

        public SimpleTaskHasNoInvocationException(SimpleTask task)
            : base($"Task \"{task.Name}\" missing a call to .Run(...)")
        {
            this.Task = task;
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

    public class SimpleTaskCircularDependencyException : SimpleTaskException
    {
        public IReadOnlyList<SimpleTask> Tasks { get; }

        internal SimpleTaskCircularDependencyException(IReadOnlyList<SimpleTask> tasks)
            : base($"Recursive dependency found: {string.Join(" -> ", tasks.Select(x => x.Name))}")
        {
            this.Tasks = tasks;
        }
    }

    public class SimpleTaskOptionException : SimpleTaskException
    {
        public string OptionName => ((OptionException)this.InnerException).OptionName;

        public SimpleTask Task { get; }

        internal SimpleTaskOptionException(SimpleTask task, OptionException innerException)
            : base($"Task: \"{task.Name}\": {innerException.Message}", innerException)
        {
            this.Task = task;
        }
    }
}
