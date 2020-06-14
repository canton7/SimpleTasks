using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace SimpleTasks.UnitTests
{
    [TestFixture]
    public class OptionTests
    {
        private SimpleTaskSet taskSet = null!;

        [SetUp]
        public void SetUp()
        {
            this.taskSet = new SimpleTaskSet();
            this.runWithDefaultIntValue = null;
            this.runWithDefaultStringValue = null;
        }

        [Test]
        public void ParsesBooleanPresent()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.InvokeAdvanced("Test", "-b");
            Assert.True(value);
        }

        [Test]
        public void ParsesBooleanPresentWithPlus()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.InvokeAdvanced("Test", "-b+");
            Assert.True(value);
        }

        [Test]
        public void ParsesBooleanAbsent()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.InvokeAdvanced("Test");
            Assert.False(value);
        }

        [Test]
        public void ParsesBooleanPresentWithMinus()
        {
            bool? value = null;
            this.taskSet.Create("Test").Run((bool b) => value = b);
            this.taskSet.InvokeAdvanced("Test", "-b-");
            Assert.False(value);
        }

        [Test]
        public void ThrowsIfOptionMissingValue()
        {
            this.taskSet.Create("Test").Run((string s) => { });
            var e = Assert.Throws<SimpleTaskOptionException>(() => this.taskSet.InvokeAdvanced("Test", "-s"));
            Assert.AreEqual("Test", e.Task.Name);
            Assert.AreEqual("-s", e.OptionName);
            StringAssert.Contains("Missing required value", e.Message);
        }

        [Test]
        public void ThrowsIfOptionCouldNotBeParsed()
        {
            this.taskSet.Create("Test").Run((int i) => { });
            var e = Assert.Throws<SimpleTaskOptionException>(() => this.taskSet.InvokeAdvanced("Test", "-i", "foo"));
            Assert.AreEqual("Test", e.Task.Name);
            Assert.AreEqual("-i", e.OptionName);
            StringAssert.Contains("Could not convert", e.Message);
        }

        [Test]
        public void TreatsValueTypeAsRequired()
        {
            this.taskSet.Create("Test").Run((int i) => { });
            Assert.Throws<SimpleTaskMissingOptionsException>(() => this.taskSet.InvokeAdvanced("Test"));
        }

        [Test]
        public void TreatsNullableValueTypeAsOptional()
        {
            int? value = null;
            this.taskSet.Create("Test").Run((int? i) => value = i);
            this.taskSet.InvokeAdvanced("Test");
            Assert.Null(value);
        }

        private int? runWithDefaultIntValue;
        private void RunWithDefaultInt(int i = 3) => this.runWithDefaultIntValue = i;

        [Test]
        public void TreatsValueTypeWithDefaultAsOptional()
        {
            this.taskSet.Create("Test").Run<int>(this.RunWithDefaultInt);
            this.taskSet.InvokeAdvanced("Test");
            Assert.AreEqual(3, this.runWithDefaultIntValue);
        }

        [Test]
        public void TreatsValueTypeNameEndingInOptAsOptional()
        {
            int? foo = null;
            int? bar = null;
            this.taskSet.Create("Test").Run((int fooOpt, int? barOpt) => (foo, bar) = (fooOpt, barOpt));
            this.taskSet.InvokeAdvanced("Test");
            Assert.AreEqual(0, foo);
            Assert.AreEqual(null, bar);
        }

        [Test]
        public void TreatsReferenceTypeAsRequired()
        {
            this.taskSet.Create("Test").Run((string s) => { });
            Assert.Throws<SimpleTaskMissingOptionsException>(() => this.taskSet.InvokeAdvanced("Test"));
        }

        private string? runWithDefaultStringValue;
        private void RunWithDefaultString(string s = "foo") => this.runWithDefaultStringValue = s;

        [Test]
        public void TreatsReferenceTypeWithDefaultAsOptional()
        {
            this.taskSet.Create("Test").Run<string>(this.RunWithDefaultString);
            this.taskSet.InvokeAdvanced("Test");
            Assert.AreEqual("foo", this.runWithDefaultStringValue);
        }

        [Test]
        public void TreatsReferenceTypeNameEndingInOptAsOptional()
        {
            string? foo = null;
            string? bar = null;
            this.taskSet.Create("Test").Run((string fooOpt, string? barOpt) => (foo, bar) = (fooOpt, barOpt));
            this.taskSet.InvokeAdvanced("Test");
            Assert.AreEqual(null, foo);
            Assert.AreEqual(null, bar);
        }
    }
}
