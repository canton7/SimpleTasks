using System;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    public class ExceptionTests
    {
        private SimpleTaskSet taskSet;

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
            Assert.AreEqual("Test", e.TaskName);
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
    }
}