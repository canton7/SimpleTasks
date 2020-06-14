using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleTasks
{
    /// <summary>
    /// A task, which can be executed
    /// </summary>
    public class SimpleTask
    {
        /// <summary>
        /// Gets the default <see cref="SimpleTaskSet"/>, used by the static methods <see cref="Create(string, string?)"/>
        /// and <see cref="Invoke(string[])"/> on this type
        /// </summary>
        public static SimpleTaskSet DefaultSet { get; } = new SimpleTaskSet();

        /// <summary>
        /// Gets the name of this task
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description for this task, if any
        /// </summary>
        public string? Description { get; }

        private readonly List<string> dependencies = new List<string>();
        /// <summary>
        /// Gets the dependency which must be run before this task is run
        /// </summary>
        public IReadOnlyList<string> Dependencies => this.dependencies;

        internal RunMethodInvoker? Invoker { get; private set; }

        internal SimpleTask(string name, string? description)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
            this.Description = description;
        }

        /// <summary>
        /// Declare that this task depends on one or more other tasks
        /// </summary>
        /// <param name="dependencies">Tasks which must run before this task</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask DependsOn(params string[] dependencies)
        {
            if (dependencies == null || dependencies.Contains(null!))
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            this.dependencies.AddRange(dependencies);
            return this;
        }

        /// <summary>
        /// Declare that this task depends on one or more other tasks
        /// </summary>
        /// <param name="dependencies">Tasks which must run before this task</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask DependsOn(params SimpleTask[] dependencies)
        {
            if (dependencies == null || dependencies.Contains(null!))
            {
                throw new ArgumentNullException(nameof(dependencies));
            }
            this.dependencies.AddRange(dependencies.Select(x => x.Name));
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run(Action action)
        {
            this.Invoker = new RunMethodInvoker(action);
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T>(Action<T> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2>(Action<T1, T2> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>();
            return this;
        }

        /// <summary>
        /// Create a new <see cref="SimpleTask"/> in <see cref="DefaultSet"/>
        /// </summary>
        /// <param name="name">Name of the task</param>
        /// <param name="description">Description of the task, if any</param>
        /// <returns>The created <see cref="SimpleTask"/>, for method chaining</returns>
        public static SimpleTask Create(string name, string? description = null) =>
            DefaultSet.Create(name, description);

        /// <summary>
        /// Invoke the tasks as specified by the command-line parameters <paramref name="args"/>
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public static int Invoke(params string[] args) =>
            DefaultSet.Invoke(args);
    }
}
