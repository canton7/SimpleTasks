using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    // These don't test anything, just output stuff for inspection
    public class HelpOutputTests
    {
        private SimpleTaskSet taskSet = null!;

        [SetUp]
        public void SetUp()
        {
            this.taskSet = new SimpleTaskSet();
        }

        private static void RunWithParams(
            int bar,
            [Option("foo", "Description")] string? foo = null)
        {
        }

        [Test]
        public void Help()
        {
            this.taskSet.Create("Task1", "This is the first task").Run<int, string?>(RunWithParams);
            this.taskSet.Create("Task2", "This is the second task").Run(() => { });
            var e = Assert.Throws<SimpleTaskHelpRequiredException>(() => this.taskSet.InvokeAdvanced("--help"));
            TestContext.Out.Write(e.HelpMessage);
        }

        [Test]
        public void ListTasks()
        {
            this.taskSet.Create("Task1", "This is the first task").Run<int, string?>(RunWithParams);
            this.taskSet.Create("Task2", "This is the second task").Run(() => { });
            var e = Assert.Throws<SimpleTaskHelpRequiredException>(() => this.taskSet.InvokeAdvanced("--list-tasks"));
            TestContext.Out.Write(e.HelpMessage);
        }

        [Test]
        public void CommandHelp()
        {
            this.taskSet.Create("Task1", "This is the first task").Run<int, string?>(RunWithParams);

            var e = Assert.Throws<SimpleTaskHelpRequiredException>(() => this.taskSet.InvokeAdvanced("Task1", "--help"));
            TestContext.Out.Write(e.HelpMessage);
        }

        [Test]
        public void HelpWithDefaultTask()
        {
            this.taskSet.Create("default", "This is the first task").Run<int, string?>(RunWithParams);
            this.taskSet.Create("Task2", "This is the second task").Run(() => { });
            var e = Assert.Throws<SimpleTaskHelpRequiredException>(() => this.taskSet.InvokeAdvanced("--help"));
            TestContext.Out.Write(e.HelpMessage);
        }

        [Test]
        public void ListTasksWithDefaultTask()
        {
            this.taskSet.Create("default", "This is the first task").Run<int, string?>(RunWithParams);
            this.taskSet.Create("Task2", "This is the second task").Run(() => { });
            var e = Assert.Throws<SimpleTaskHelpRequiredException>(() => this.taskSet.InvokeAdvanced("--list-tasks"));
            TestContext.Out.Write(e.HelpMessage);
        }
    }
}
