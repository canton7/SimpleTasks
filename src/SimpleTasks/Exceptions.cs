using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Options;

namespace SimpleTasks
{
    /// <summary>
    /// Base class for all exceptions thrown by SimpleTasks
    /// </summary>
    public abstract class SimpleTaskException : Exception
    {
        /// <inheritdoc/>
        public SimpleTaskException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public SimpleTaskException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Exception thrown when the user doesn't specify a task name to run (and there's no 'default' task)
    /// </summary>
    public class SimpleTaskNoTaskNameSpecifiedException : SimpleTaskException
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskNoTaskNameSpecifiedException"/> class
        /// </summary>
        public SimpleTaskNoTaskNameSpecifiedException()
            : base("No task name to run specified (and no task called \"default\" was defined)")
        {
        }
    }

    /// <summary>
    /// Exception thrown when the no task is found with the given task name
    /// </summary>
    public class SimpleTaskNotFoundException : SimpleTaskException
    {
        /// <summary>
        /// Gets the task name which the user specified, which was not found
        /// </summary>
        public string TaskName { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskNotFoundException"/> with the given task name
        /// </summary>
        /// <param name="taskName">Task name which was not found</param>
        public SimpleTaskNotFoundException(string taskName)
            : base($"Unable to find task \"{taskName}\"")
        {
            this.TaskName = taskName;
        }
    }

    /// <summary>
    /// Exception thrown when a task's dependency was not found
    /// </summary>
    public class SimpleTaskDependencyNotFoundException : SimpleTaskException
    {
        /// <summary>
        /// Gets the task whose dependency was not found
        /// </summary>
        public SimpleTask Task { get; }

        /// <summary>
        /// Gets the name of the dependency which was not found
        /// </summary>
        public string DependencyName { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskDependencyNotFoundException"/> class
        /// </summary>
        /// <param name="task">Task whose dependency was not found</param>
        /// <param name="dependencyName">Name of the dependency which was not found</param>
        public SimpleTaskDependencyNotFoundException(SimpleTask task, string dependencyName)
            : base($"Task \"{task.Name}\": unable to find dependency \"{dependencyName}\"")
        {
            this.Task = task;
            this.DependencyName = dependencyName;
        }
    }

    /// <summary>
    /// Exception thrown when multiple tasks with the same name are found
    /// </summary>
    public class SimpleTaskDuplicateTaskNameException : SimpleTaskException
    {
        /// <summary>
        /// Gets the duplicate task name
        /// </summary>
        public string TaskName { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskDuplicateTaskNameException"/> class
        /// </summary>
        /// <param name="taskName">Duplicate task name</param>
        public SimpleTaskDuplicateTaskNameException(string taskName)
            : base($"Multiple tasks with the name \"{taskName}\" found")
        {
            this.TaskName = taskName;
        }
    }

    /// <summary>
    /// Exception thrown when a task has no invocation
    /// </summary>
    public class SimpleTaskHasNoInvocationException : SimpleTaskException
    {
        /// <summary>
        /// Gets the task which has no invocation
        /// </summary>
        public SimpleTask Task { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskHasNoInvocationException"/> class
        /// </summary>
        /// <param name="task">Task which has no invocation</param>
        public SimpleTaskHasNoInvocationException(SimpleTask task)
            : base($"Task \"{task.Name}\" missing a call to .Run(...)")
        {
            this.Task = task;
        }
    }

    /// <summary>
    /// Exception thrown when one or more required options are missing
    /// </summary>
    public class SimpleTaskMissingOptionsException : SimpleTaskException
    {
        /// <summary>
        /// Gets the options which are required, but missing
        /// </summary>
        public IReadOnlyList<string> Options { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskMissingOptionsException"/>
        /// </summary>
        /// <param name="groups">Groups of missing option to tasks which have that option</param>
        public SimpleTaskMissingOptionsException(List<IGrouping<string, SimpleTask>> groups)
            : base($"Missing option{(groups.Count == 1 ? "" : "s")}: " +
                  $"{string.Join(", ", groups.Select(FormatSingle))}")
        {
            this.Options = groups.Select(x => x.Key).ToList();
        }

        private static string FormatSingle(IGrouping<string, SimpleTask> group) =>
            $"\"{group.Key}\" (required by {string.Join(", ", group.Select(x => $"\"{x.Name}\""))})";
    }

    /// <summary>
    /// Exception thrown when one or more unknown options are passed
    /// </summary>
    public class SimpleTaskUnknownOptionsException : SimpleTaskException
    {
        /// <summary>
        /// Gets the unknown options which were passed
        /// </summary>
        public IReadOnlyList<string> Options { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskUnknownOptionsException"/> class
        /// </summary>
        /// <param name="options">Unknown options which were passed</param>
        public SimpleTaskUnknownOptionsException(List<string> options)
            : base($"Unknown option{(options.Count == 1 ? "" : "s")}: " +
                  $"{string.Join(", ", options.Select(x => $"\"{x}\""))}")
        {
            this.Options = options;
        }
    }

    /// <summary>
    /// Exception thrown when a circular dependency between tasks is detected
    /// </summary>
    public class SimpleTaskCircularDependencyException : SimpleTaskException
    {
        /// <summary>
        /// Gets the tasks involved in the circular dependency
        /// </summary>
        public IReadOnlyList<SimpleTask> Tasks { get; }

        internal SimpleTaskCircularDependencyException(IReadOnlyList<SimpleTask> tasks)
            : base($"Recursive dependency found: {string.Join(" -> ", tasks.Select(x => x.Name))}")
        {
            this.Tasks = tasks;
        }
    }

    /// <summary>
    /// Exception thrown when there is a problem with an option
    /// </summary>
    public class SimpleTaskOptionException : SimpleTaskException
    {
        /// <summary>
        /// Gets the name of the option which has a problem
        /// </summary>
        public string? OptionName => ((OptionException)this.InnerException).OptionName;

        /// <summary>
        /// Gets the task which has the option
        /// </summary>
        public SimpleTask Task { get; }

        internal SimpleTaskOptionException(SimpleTask task, OptionException innerException)
            : base($"Task: \"{task.Name}\": {innerException.Message}", innerException)
        {
            this.Task = task;
        }
    }

    /// <summary>
    /// Exception thrown when the user has requested that help is displayed
    /// </summary>
    public class SimpleTaskHelpRequiredException : SimpleTaskException
    {
        /// <summary>
        /// Gets the help message to be displayed to the user
        /// </summary>
        public string HelpMessage { get; }

        /// <summary>
        /// Initialises a new instance of the <see cref="SimpleTaskHelpRequiredException"/>
        /// </summary>
        /// <param name="helpMessage">Help message to be displayed to the user</param>
        public SimpleTaskHelpRequiredException(string helpMessage)
            : base("User requested help")
        {
            this.HelpMessage = helpMessage;
        }
    }
}
