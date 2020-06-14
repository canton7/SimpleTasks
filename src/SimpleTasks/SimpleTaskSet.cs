using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace SimpleTasks
{
    /// <summary>
    /// A task set, in which tasks can be defined, and on which tasks can be executed
    /// </summary>
    public class SimpleTaskSet
    {
        /// <summary>
        /// Gets the name of the default task
        /// </summary>
        /// <remarks>
        /// If a task of this name is defined, and the user doesn't specify the names of any tasks to run,
        /// then this task is run.
        /// </remarks>
        public const string DefaultTaskName = "default";

        private readonly List<SimpleTask> tasks = new List<SimpleTask>();

        /// <summary>
        /// Create a new <see cref="SimpleTask"/>
        /// </summary>
        /// <param name="name">Name of the task</param>
        /// <param name="description">Description of the task, if any</param>
        /// <returns>The created <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Create(string name, string? description = null)
        {
            var task = new SimpleTask(name, description);
            this.tasks.Add(task);
            return task;
        }

        /// <summary>
        /// Invoke the tasks as specified by the command-line parameters <paramref name="args"/>
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        /// <returns>Exit status, for returning from Main</returns>
        public int Invoke(params string[] args)
        {
            try
            {
                this.InvokeAdvanced(args);
                return 0;
            }
            catch (SimpleTaskHelpRequiredException e)
            {
                Console.Error.Write(e.HelpMessage);
                return 0;
            }
            catch (SimpleTaskException e)
            {
                Console.Error.WriteLine(e.Message);
                return -1;  
            }
        }

        /// <summary>
        /// Similar to <see cref="Invoke(string[])"/>, but nothing is written to <see cref="Console"/>, and
        /// instead <see cref="SimpleTaskException"/> subclasses are thrown.
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public void InvokeAdvanced(params string[] args)
        {
            var localArgs = args.ToList();

            var taskInvocations = this.CreateTaskInvocations();
            var tasksToRun = GetTasksToRun(localArgs, taskInvocations);

            if (tasksToRun.Count == 0)
            {
                ShowHelpIfRequired(localArgs, taskInvocations);

                if (taskInvocations.TryGetValue(DefaultTaskName, out var taskInvocation))
                {
                    tasksToRun.Add(taskInvocation);
                }
            }

            if (tasksToRun.Count == 0)
            {
                throw new SimpleTaskNoTaskNameSpecifiedException();
            }

            var tasksToRunWithDependencies = AddDependencies(tasksToRun);
            RunTasks(localArgs, tasksToRunWithDependencies);
        }

        private static List<TaskInvocation> GetTasksToRun(List<string> args, Dictionary<string, TaskInvocation> taskInvocations)
        {
            var tasksToRun = new List<TaskInvocation>();
            for (int i = 0; i < args.Count; i++)
            {
                if (!args[i].StartsWith("-"))
                {
                    if (!taskInvocations.TryGetValue(args[i], out var taskInvocation))
                    {
                        throw new SimpleTaskNotFoundException(args[i]);
                    }
                    tasksToRun.Add(taskInvocation);
                    args.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }
            }

            return tasksToRun;
        }

        private static void ShowHelpIfRequired(List<string> args, Dictionary<string, TaskInvocation> taskInvocations)
        {
            bool showHelp = false;
            bool listTasks = false;

            var rootOptions = new OptionSet()
            {
                $"USAGE: {GetExeName()} [commands...] [options]",
                "",
                "Common options:",
                { "h|help", "Show help", x => showHelp = x != null },
                { "T|list-tasks", "List tasks", x => listTasks = x != null },
            };

            var extra = rootOptions.Parse(args);
            args.Clear();
            args.AddRange(extra);

            if (showHelp || listTasks)
            {
                var writer = new StringWriter();
                if (showHelp)
                {
                    rootOptions.WriteOptionDescriptions(writer);
                    writer.WriteLine();
                    writer.WriteLine("Commands:");
                    writer.WriteLine();
                }

                foreach (var command in taskInvocations.Values.OrderBy(x => x.Task.Name).Select(x => x.Command))
                {
                    if (showHelp)
                    {
                        command.Options.WriteOptionDescriptions(writer);
                        writer.WriteLine();
                    }
                    else
                    {
                        writer.WriteLine($"{command.Name,-28} {command.Help}");
                    }
                }

                throw new SimpleTaskHelpRequiredException(writer.ToString());
            }
        }

        private Dictionary<string, TaskInvocation> CreateTaskInvocations()
        {
            // Mapping of task name -> invocation for that task
            var taskInvocations = new Dictionary<string, TaskInvocation>();
            foreach (var task in this.tasks)
            {
                if (task.Invoker == null)
                {
                    throw new SimpleTaskHasNoInvocationException(task);
                }

                if (taskInvocations.ContainsKey(task.Name))
                {
                    throw new SimpleTaskDuplicateTaskNameException(task.Name);
                }

                taskInvocations.Add(task.Name, new TaskInvocation(task));
            }

            foreach (var invocation in taskInvocations.Values)
            {
                foreach (string dependency in invocation.Task.Dependencies)
                {
                    if (taskInvocations.TryGetValue(dependency, out var dependencyInvocation))
                    {
                        invocation.Prerequisites.Add(dependencyInvocation);
                    }
                    else
                    {
                        throw new SimpleTaskDependencyNotFoundException(invocation.Task, dependency);
                    }
                }
            }

            return taskInvocations;
        }

        private static List<TaskInvocation> AddDependencies(IEnumerable<TaskInvocation> tasksToRun)
        {
            // We choose a depth-first search (https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search) as it doesn't
            // need to start with a set of all nodes with no incoming edge, and it nicely detects circular references.
            // If we hit a circular dependency, the chain leading up to the dependency is sitting on the stack, so we can
            // just unwind it using exceptions -- it's a bit ugly, but we're not going to hit it often.

            var result = new List<TaskInvocation>();

            foreach (var taskToRun in tasksToRun)
            {
                Visit(taskToRun);
            }

            void Visit(TaskInvocation node)
            {
                if (node.Mark == TaskInvocationMark.Permanent)
                {
                    return;
                }
                if (node.Mark == TaskInvocationMark.Temporary)
                {
                    throw new SimpleTaskCircularDependencyException(new[] { node.Task });
                }

                node.Mark = TaskInvocationMark.Temporary;

                foreach (var prerequisite in node.Prerequisites)
                {
                    try
                    {
                        Visit(prerequisite);
                    }
                    catch (SimpleTaskCircularDependencyException e)
                    {
                        throw new SimpleTaskCircularDependencyException(e.Tasks.Prepend(node.Task).ToList());
                    }
                }

                node.Mark = TaskInvocationMark.Permanent;
                result.Add(node);
            }

            result.Reverse();
            return result;
        }

        private static void RunTasks(List<string> args, List<TaskInvocation> taskInvocations)
        {
            var argsWithNoMatches = new HashSet<string>(args);
            foreach (var taskInvocation in taskInvocations)
            {
                try
                {
                    var extra = taskInvocation.Command.Options.Parse(args);
                    argsWithNoMatches.IntersectWith(extra);
                }
                catch (OptionException e)
                {
                    throw new SimpleTaskOptionException(taskInvocation.Task, e);
                }
            }

            if (argsWithNoMatches.Count > 0)
            {
                throw new SimpleTaskUnknownOptionsException(argsWithNoMatches.ToList());
            }

            var missingArgs = taskInvocations
                .SelectMany(inv => inv.GetMissingArguments().Select(arg => (task: inv.Task, arg: arg)))
                .GroupBy(x => FormatArg(x.arg), x => x.task)
                .ToList();
            if (missingArgs.Count > 0)
            {
                throw new SimpleTaskMissingOptionsException(missingArgs);
            }

            foreach (var taskInvocation in taskInvocations)
            {
                taskInvocation.Invoke();
            }
        }

        private static string GetExeName()
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly == null ? "" : Path.GetFileNameWithoutExtension(assembly.Location);
        }

        private static string FormatArg(string arg) => arg.Length == 1 ? $"-{arg}" : $"--{arg}";
    }
}
