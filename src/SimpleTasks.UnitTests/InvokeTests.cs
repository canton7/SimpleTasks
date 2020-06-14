﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    [TestFixture]
    public class InvokeTests
    {
        private SimpleTaskSet taskSet = null!;

        [SetUp]
        public void SetUp()
        {
            this.taskSet = new SimpleTaskSet();
        }

        [Test]
        public void InvokesTasksInOrder()
        {
            var output = new List<string>();
            this.taskSet.Create("Task1").Run(() => output.Add("Task1"));
            this.taskSet.Create("Task2").Run(() => output.Add("Task2"));
            this.taskSet.Invoke("Task1", "Task2");
            CollectionAssert.AreEqual(new[] { "Task1", "Task2" }, output);
        }
    }
}
