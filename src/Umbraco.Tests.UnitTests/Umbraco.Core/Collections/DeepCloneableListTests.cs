﻿using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Collections;
using Umbraco.Tests.Common;

namespace Umbraco.Tests.UnitTests.Umbraco.Core.Collections
{
    [TestFixture]
    public class DeepCloneableListTests
    {
        [Test]
        public void Deep_Clones_Each_Item_Once()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.CloneOnce);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(list.SequenceEqual(cloned));

            //Test that each instance in the list is not the same one
            foreach (var item in list)
            {
                var clone = cloned.Single(x => x.Id == item.Id);
                Assert.AreNotSame(item, clone);
            }

            //clone again from the clone - since it's clone once the items should be the same
            var cloned2 = cloned.DeepClone() as DeepCloneableList<TestClone>;

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(cloned.SequenceEqual(cloned2));

            //Test that each instance in the list is the same one
            foreach (var item in cloned)
            {
                var clone = cloned2.Single(x => x.Id == item.Id);
                Assert.AreSame(item, clone);
            }
        }

        [Test]
        public void Deep_Clones_All_Elements()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = list.DeepClone() as DeepCloneableList<TestClone>;

            Assert.IsNotNull(cloned);
            Assert.AreNotSame(list, cloned);
            Assert.AreEqual(list.Count, cloned.Count);
        }

        [Test]
        public void Clones_Each_Item()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = (DeepCloneableList<TestClone>) list.DeepClone();

            foreach (var item in cloned)
            {
                Assert.IsTrue(item.IsClone);
            }
        }

        [Test]
        public void Cloned_Sequence_Equals()
        {
            var list = new DeepCloneableList<TestClone>(ListCloneBehavior.Always);
            list.Add(new TestClone());
            list.Add(new TestClone());
            list.Add(new TestClone());

            var cloned = (DeepCloneableList<TestClone>) list.DeepClone();

            //Test that each item in the sequence is equal - based on the equality comparer of TestClone (i.e. it's ID)
            Assert.IsTrue(list.SequenceEqual(cloned));

            //Test that each instance in the list is not the same one
            foreach (var item in list)
            {
                var clone = cloned.Single(x => x.Id == item.Id);
                Assert.AreNotSame(item, clone);
            }
        }
    }
}
