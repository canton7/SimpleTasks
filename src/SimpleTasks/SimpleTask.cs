﻿using System;
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
        /// Gets the name of this task
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description for this task, if any
        /// </summary>
        public string? Description { get; }

        private readonly List<string> dependencies = new();
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
        /// Create a new <see cref="SimpleTask"/> in <see cref="SimpleTaskSet.Default"/>
        /// </summary>
        /// <param name="name">Name of the task</param>
        /// <param name="description">Description of the task, if any</param>
        /// <returns>The created <see cref="SimpleTask"/>, for method chaining</returns>
        public static SimpleTask CreateTask(string name, string? description = null) =>
            SimpleTaskSet.Default.Create(name, description);

        /// <summary>
        /// Invoke the tasks as specified by the command-line parameters <paramref name="args"/> in
        /// <see cref="SimpleTaskSet.Default"/>
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public static int InvokeTask(params string[] args) =>
            SimpleTaskSet.Default.Invoke(args);

        /// <summary>
        /// Invoke the tasks as specified by the command-line parameters <paramref name="args"/> in
        /// <see cref="SimpleTaskSet.Default"/>
        /// </summary>
        /// <param name="args">Command-line parameters</param>
        public static int InvokeTask(IEnumerable<string> args) =>
            SimpleTaskSet.Default.Invoke(args);

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
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7>(Action<T1, T2, T3, T4, T5, T6, T7> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8>(Action<T1, T2, T3, T4, T5, T6, T7, T8> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>().Arg<T12>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>().Arg<T12>().Arg<T13>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>().Arg<T12>().Arg<T13>().Arg<T14>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>().Arg<T12>().Arg<T13>().Arg<T14>().Arg<T15>();
            return this;
        }

        /// <summary>
        /// Specify the method which should be executed when this task is invoked
        /// </summary>
        /// <param name="action">Method which must be executed when this task is invoked</param>
        /// <returns>The current <see cref="SimpleTask"/>, for method chaining</returns>
        public SimpleTask Run<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(Action<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action)
        {
            this.Invoker = new RunMethodInvoker(action).Arg<T1>().Arg<T2>().Arg<T3>().Arg<T4>().Arg<T5>().Arg<T6>().Arg<T7>().Arg<T8>().Arg<T9>().Arg<T10>().Arg<T11>().Arg<T12>().Arg<T13>().Arg<T14>().Arg<T15>().Arg<T16>();
            return this;
        }
    }
}
