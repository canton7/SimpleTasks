using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Mono.Options;

namespace SimpleTasks
{
    public class SimpleTaskSet
    {
        private readonly List<SimpleTask> tasks = new List<SimpleTask>();
        
        public SimpleTask Create(string name, string? description = null)
        {
            var task = new SimpleTask(name, description);
            this.tasks.Add(task);
            return task;
        }

        public void Invoke(params string[] args)
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

            bool showHelp = false;
            bool listTasks = false;

            var rootOptions = new OptionSet
            {
                { "Common options:" },
                { "h|help", "Show help", x => showHelp = x != null },
                { "L|list-tasks", "List tasks", x => listTasks = x != null },
            };

            var extra = rootOptions.Parse(args);

            if (showHelp || listTasks)
            {
                if (showHelp)
                {
                    rootOptions.WriteOptionDescriptions(Console.Out);
                    Console.WriteLine();
                }

                Console.WriteLine("Commands:");

                foreach (var command in taskInvocations.Values.OrderBy(x => x.Task.Name).Select(x => x.Command))
                {
                    Console.WriteLine($"  {command.Name,-26} {command.Help}");
                    if (showHelp)
                    {
                        command.Options.WriteOptionDescriptions(Console.Out);
                        Console.WriteLine();
                    }
                }
                return;
            }

            var tasksToRun = new List<TaskInvocation>();
            for (int i = 0; i < extra.Count; i++)
            {
                if (!extra[i].StartsWith("-"))
                {
                    if (!taskInvocations.TryGetValue(extra[i], out var taskInvocation))
                    {
                        throw new SimpleTaskNotFoundException(extra[i]);
                    }
                    tasksToRun.Add(taskInvocation);
                    extra.RemoveAt(i);
                    i--;
                }
                else
                {
                    break;
                }
            }

            if (tasksToRun.Count == 0)
            {
                if (taskInvocations.TryGetValue("default", out var taskInvocation))
                {
                    tasksToRun.Add(taskInvocation);
                }
            }
            
            if (tasksToRun.Count == 0)
            {
                throw new SimpleTaskNoTaskNameSpecifiedException();
            }

            var tasksToRunWithDependencies = this.AddDependencies(tasksToRun);

            this.RunTasks(extra, tasksToRunWithDependencies);
        }

        private List<TaskInvocation> AddDependencies(IEnumerable<TaskInvocation> tasksToRun)
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

        private void RunTasks(List<string> args, List<TaskInvocation> taskInvocations)
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

        private static string FormatArg(string arg) => arg.Length == 1 ? $"-{arg}" : $"--{arg}";
    }
}
