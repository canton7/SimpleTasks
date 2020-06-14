using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    [TestFixture]
    public class OptionTests
    {
        private SimpleTaskSet taskSet;

        [SetUp]
        public void SetUp()
        {
            this.taskSet = new SimpleTaskSet();
        }

        [Test]
        public void ParsesBooleanPresent()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.Invoke("Test", "-b");
            Assert.True(value);
        }

        [Test]
        public void ParsesBooleanPresentWithPlus()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.Invoke("Test", "-b+");
            Assert.True(value);
        }

        [Test]
        public void ParsesBooleanAbsent()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.Invoke("Test");
            Assert.False(value);
        }

        [Test]
        public void ParsesBooleanPresentWithMinus()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.Invoke("Test", "-b-");
            Assert.False(value);
        }

        [Test]
        public void ThrowsIfOptionMissingValue()
        {
            this.taskSet.Create("Test").Run((string s) => { });
            var e = Assert.Throws<SimpleTaskOptionException>(() => this.taskSet.Invoke("Test", "-s"));
            Assert.AreEqual("Test", e.Task.Name);
            Assert.AreEqual("-s", e.OptionName);
            StringAssert.Contains("Missing required value", e.Message);
        }

        [Test]
        public void ThrowsIfOptionCouldNotBeParsed()
        {
            this.taskSet.Create("Test").Run((int i) => { });
            var e = Assert.Throws<SimpleTaskOptionException>(() => this.taskSet.Invoke("Test", "-i", "foo"));
            Assert.AreEqual("Test", e.Task.Name);
            Assert.AreEqual("-i", e.OptionName);
            StringAssert.Contains("Could not convert", e.Message);
        }
    }
}
