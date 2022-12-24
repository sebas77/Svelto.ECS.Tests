using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.DataStructures.Native;

namespace Svelto.Common.Tests.Datastructures
{
    [TestFixture]
    public class TestValueContainer
    {
        private class Test
        {
            private uint _id;

            public Test(uint id)
            {
                _id = id;
            }
        }
        
        [Test]
        public void TestResize()
        {
            uint initialCount = 5;

            ValueContainer<Test, ManagedStrategy<Test>, NativeStrategy<SparseIndex>> testContainer = new(initialCount);

            for (uint i = 0; i < initialCount + 1; i++)
            {
                Assert.DoesNotThrow(() => testContainer.Add(new(i)));
                Assert.IsTrue(testContainer.capacity >= initialCount);
            }
        }
        
        [Test]
        public void TestRemoveSequence()
        {
            ValueContainer<Test, ManagedStrategy<Test>, NativeStrategy<SparseIndex>> testContainer = new(10);

            ValueIndex[] indexArray = new ValueIndex[10];

            for (uint i = 0; i < 10; i++)
            {
                indexArray[i] = testContainer.Add(new(i));
            }
            
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[9]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[8]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[7]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[6]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[5]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[4]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[3]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[2]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[1]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[0]));
        }
        
        [Test]
        public void TestRemoveUnsequenced()
        {
            ValueContainer<Test, ManagedStrategy<Test>, NativeStrategy<SparseIndex>> testContainer = new(10);

            ValueIndex[] indexArray = new ValueIndex[10];

            for (uint i = 0; i < 10; i++)
            {
                indexArray[i] = testContainer.Add(new(i));
            }
            
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[9]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[7]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[6]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[4]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[1]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[8]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[5]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[3]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[0]));
            Assert.DoesNotThrow(() => testContainer.Remove(indexArray[2]));
        }
        
        [Test]
        public void TestGetAfterRemoveUnsequenced()
        {
            ValueContainer<Test, ManagedStrategy<Test>, NativeStrategy<SparseIndex>> testContainer = new(10);

            ValueIndex[] indexArray = new ValueIndex[10];
            Test[] testArray = new Test[10];

            for (uint i = 0; i < 10; i++)
            {
                testArray[i]  = new(i);
                indexArray[i] = testContainer.Add(testArray[i]);
            }

            testContainer.Remove(indexArray[7]);
            Assert.AreEqual(testArray[9], testContainer[indexArray[9]]);
        }
        
        [Test]
        public void TestGetAfterRemoveSequenced()
        {
            ValueContainer<Test, ManagedStrategy<Test>, NativeStrategy<SparseIndex>> testContainer = new(10);

            ValueIndex[] indexArray = new ValueIndex[10];
            Test[]       testArray  = new Test[10];

            for (uint i = 0; i < 10; i++)
            {
                testArray[i]  = new(i);
                indexArray[i] = testContainer.Add(testArray[i]);
            }

            testContainer.Remove(indexArray[9]);
            Assert.AreEqual(testArray[7], testContainer[indexArray[7]]);
        }
    }
}