using System;
using System.Linq;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    public class ExceptionTests
    {
        private SimpleTaskSet taskSet = null!;

        [SetUp]
        public void SetUp()
        {
            this.taskSet = new SimpleTaskSet();
        }

        [Test]
        public void ThrowsIfTwoTasksWithTheSameName()
        {
            this.taskSet.Create("Test").Run(() => { });
            this.taskSet.Create("Test").Run(() => { });
            var e = Assert.Throws<SimpleTaskDuplicateTaskNameException>(() => this.taskSet.Invoke("Test"));
            Assert.AreEqual("Test", e.TaskName);
        }

        [Test]
        public void ThrowsIfTaskHasNoInvocation()
        {
            this.taskSet.Create("Test");
            var e = Assert.Throws<SimpleTaskHasNoInvocationException>(() => this.taskSet.Invoke("Test"));
            Assert.AreEqual("Test", e.Task.Name);
        }

        [Test]
        public void ThrowsIfTaskNotFound()
        {
            var e = Assert.Throws< SimpleTaskNotFoundException>(() => this.taskSet.Invoke("Test"));
            Assert.AreEqual("Test", e.TaskName);
        }

        [Test]
        public void ThrowsIfDependencyNotFound()
        {
            this.taskSet.Create("Test").DependsOn("NonExistent").Run(() => { });
            var e = Assert.Throws<SimpleTaskDependencyNotFoundException>(() => this.taskSet.Invoke("Test"));
            Assert.AreEqual("Test", e.Task.Name);
            Assert.AreEqual("NonExistent", e.DependencyName);
        }

        [Test]
        public void ThrowsIfRequiredArgumentNotPassed()
        {
            this.taskSet.Create("Test").DependsOn("Test2").Run((string a, int foo) => { });
            this.taskSet.Create("Test2").Run((string a) => { });
            var e = Assert.Throws<SimpleTaskMissingOptionsException>(() => this.taskSet.Invoke("Test"));
            CollectionAssert.AreEqual(new[] { "-a", "--foo" }, e.Options);
        }

        [Test]
        public void ThrowsIfArgumentNotKnown()
        {
            this.taskSet.Create("Test").Run((string a) => { });
            var e = Assert.Throws<SimpleTaskUnknownOptionsException>(() =>
                this.taskSet.Invoke("Test", "--foo", "-a", "value", "--bar"));
            CollectionAssert.AreEqual(new[] { "--foo", "--bar" }, e.Options);
        }

        [Test]
        public void ThrowsIfCircularDependency()
        {
            this.taskSet.Create("Task1").DependsOn("Task2").Run(() => { });
            this.taskSet.Create("Task2").DependsOn("Task3").Run(() => { });
            this.taskSet.Create("Task3").DependsOn("Task2").Run(() => { });

            var e = Assert.Throws<SimpleTaskCircularDependencyException>(() => this.taskSet.Invoke("Task1"));
            CollectionAssert.AreEqual(new[] { "Task1", "Task2", "Task3", "Task2" }, e.Tasks.Select(x => x.Name));
        }

        [Test]
        public void ThrowsIfNoTaskNameGiven()
        {
            this.taskSet.Create("Test").Run((string s) => { });
            Assert.Throws< SimpleTaskNoTaskNameSpecifiedException>(() => this.taskSet.Invoke("-s", "foo"));
        }
    }
}