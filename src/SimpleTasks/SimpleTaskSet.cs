using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public void Invoke(string[] args)
        {
            var taskInvocations = new Dictionary<string, TaskInvocation>();
            foreach (var task in this.tasks)
            {
                if (task.Invocation == null)
                    continue;

                taskInvocations.Add(task.Name, new TaskInvocation(task));
            }

            bool showHelp = false;

            var rootOptions = new OptionSet();
            rootOptions.Add("h|help", "Show help", _ => showHelp = true);

            var extra = rootOptions.Parse(args);

            if (showHelp)
            {
                foreach (var command in taskInvocations.Values.OrderBy(x => x.Task.Name).Select(x => x.Command))
                {
                    Console.WriteLine($"  {command.Name,-30} {command.Help}");
                    command.Options.WriteOptionDescriptions(Console.Out);
                    Console.WriteLine();
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
                        // TODO: Proper exception
                        throw new Exception($"No task {extra[i]}");
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

            // TODO: Dependency resolution, ordering
            this.RunTasks(extra, tasksToRun);
        }

        private void RunTasks(List<string> args, List<TaskInvocation> taskInvocations)
        {
            var argsWithNoMatches = new HashSet<string>(args);
            foreach (var taskInvocation in taskInvocations)
            {
                var extra = taskInvocation.Command.Options.Parse(args);
                argsWithNoMatches.IntersectWith(extra);
            }

            if (argsWithNoMatches.Count > 0)
            {
                // TODO: Proper exception, add "=="
                throw new Exception($"Unknown option{(argsWithNoMatches.Count == 1 ? "" : "s")}: {string.Join(" ", argsWithNoMatches)}");
            }

            var missingArgs = taskInvocations.SelectMany(x => x.GetMissingArguments()).ToList();
            if (missingArgs.Count > 0)
            {
                // TODO: Proper exception, add "--"
                throw new Exception($"Missing option{(missingArgs.Count == 1 ? "" : "s")}: {string.Join(" ", missingArgs)}");
            }

            foreach (var taskInvocation in taskInvocations)
            {
                taskInvocation.Invoke();
            }
        }
    }
}
