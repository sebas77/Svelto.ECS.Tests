using NUnit.Framework;
using Svelto.Common;
using Svelto.Common.DataStructures;
using Assert = NUnit.Framework.Assert;

namespace Svelto.ECS.Tests.NativeDataStructures
{
    [TestFixture]
    class NativeListTests
    {
        [TestCase]
        public void TestAllocationSize0()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);

            Assert.That(fasterList.capacity, Is.EqualTo(0));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestAllocationSize1()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(1, Allocator.Persistent);

            Assert.That(fasterList.capacity, Is.EqualTo(1));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestResize()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
            
            fasterList.Resize(10);

            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestResizeTo0()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(10, Allocator.Persistent);
            
            fasterList.Resize(0);

            Assert.That(fasterList.capacity, Is.EqualTo(0));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        // [TestCase]
        // public void TestExpandTo()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //     
        //     fasterList.ExpandTo(10);
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(10));
        //     
        //     fasterList.Dispose();
        // }
        //
        // [TestCase]
        // public void TestExpandByFromZero()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //     
        //     fasterList.ExpandBy(10);
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(10));
        //     
        //     fasterList.Dispose();
        // }
        //
        // [TestCase]
        // public void TestSet()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //     
        //     fasterList.ExpandTo(10);
        //     
        //     for (int i = 0; i < 10; i++)
        //         fasterList[i] = i;
        //     
        //     for (int i = 0; i < 10; i++)
        //         Assert.That(fasterList[i], Is.EqualTo(i));
        //     
        //     fasterList.Dispose();
        // }

        [TestCase]
        public void TestAdd()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);
            
            for (int i = 0; i < 10; i++)
                Assert.That(fasterList[i], Is.EqualTo(i));

            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(10));
            
            fasterList.Dispose();
        }
        
        // [TestCase]
        // public void TestExpandBy()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(3, Allocator.Persistent);
        //     
        //     fasterList.Add(0);
        //     fasterList.Add(1);
        //     fasterList.Add(2);
        //     
        //     fasterList.ExpandBy(10);
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(13));
        //     Assert.That(fasterList.count, Is.EqualTo(13));
        //     
        //     fasterList.Dispose();
        // }
        //
        // [TestCase]
        // public void TestEnsureCapacity()
        // {
        //     NativeDynamicArrayCast<int> fasterListA = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //     
        //     fasterListA.EnsureCapacity(10);
        //     
        //     Assert.That(fasterListA.capacity, Is.EqualTo(10));
        //     Assert.That(fasterListA.count, Is.EqualTo(0));
        //     
        //     fasterListA.Dispose();
        // }
        //
        // [TestCase]
        // public void TestEnsureExtraCapacity()
        // {
        //     NativeDynamicArrayCast<int> fasterListA = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //     
        //     fasterListA.ExpandBy(10);
        //     fasterListA.EnsureExtraCapacity(10);
        //     
        //     Assert.That(fasterListA.capacity, Is.EqualTo(20));
        //     Assert.That(fasterListA.count, Is.EqualTo(10));
        //     
        //     fasterListA.Dispose();
        // }
        //
        // [TestCase]
        // public void TestPush()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //
        //     for (int i = 0; i < 10; i++)
        //         fasterList.Push(i);
        //     
        //     for (int i = 0; i < 10; i++)
        //         Assert.That(fasterList[i], Is.EqualTo(i));
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(10));
        //     fasterList.Dispose();
        // }
        //
        // [TestCase]
        // public void TestPop()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //
        //     for (int i = 0; i < 10; i++)
        //         fasterList.Push(i);
        //     
        //     for (int i = 9; i >= 0; i--)
        //         Assert.That(fasterList.Pop(), Is.EqualTo(i));
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(0));
        //     
        //     fasterList.Dispose();
        // }
        //
        // [TestCase]
        // public void TestPeek()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //
        //     for (int i = 0; i < 10; i++)
        //         fasterList.Push(i);
        //     
        //     Assert.That(fasterList.Peek(), Is.EqualTo(9));
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(10));
        //     
        //     fasterList.Dispose();
        // }
        //
        
        [TestCase]
        public void TestRemoveAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);
            
            fasterList.RemoveAt(3);
            Assert.That(fasterList[3], Is.EqualTo(4));
            
            fasterList.RemoveAt(0);
            Assert.That(fasterList[0], Is.EqualTo(1));
            
            fasterList.RemoveAt(7);
            
            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(7));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestUnorderedRemoveAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);

            for (int i = 0; i < 10; i++)
                fasterList.Add(i);
            
            fasterList.UnorderedRemoveAt(3);
            Assert.That(fasterList[3], Is.EqualTo(9));
            
            fasterList.UnorderedRemoveAt(0);
            Assert.That(fasterList[0], Is.EqualTo(8));
            
            fasterList.UnorderedRemoveAt(7);
            
            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(7));
            
            fasterList.Dispose();
        }
        
        // [TestCase]
        // public void TestTrim()
        // {
        //     NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
        //
        //     for (int i = 0; i < 10; i++)
        //         fasterList.Add(i);
        //     
        //     fasterList.UnorderedRemoveAt(3);
        //     fasterList.UnorderedRemoveAt(0);
        //     fasterList.UnorderedRemoveAt(7);
        //     
        //     fasterList.Trim();
        //     
        //     Assert.That(fasterList.capacity, Is.EqualTo(7));
        //     Assert.That(fasterList.count, Is.EqualTo(7));
        //     
        //     fasterList.Dispose();
        // }
        
        [TestCase]
        public void TesSetAt()
        {
            NativeDynamicArrayCast<int> fasterList = new NativeDynamicArrayCast<int>(0, Allocator.Persistent);
            
            fasterList.AddAt(10) = 10;
            
            Assert.That(fasterList[10], Is.EqualTo(10));
            
            Assert.That(fasterList.capacity, Is.EqualTo(16));
            Assert.That(fasterList.count, Is.EqualTo(11));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestContains()
        {
            
        }
        [TestCase]
        public void TestFastClear()
        {
            
        }
        [TestCase]
        public void TestResetToReuse()
        {
        }
        
        [TestCase]
        public void TestReuseOneSlot()
        {
            
        }
        [TestCase]
        public void TestCopyTo()
        {
            
        }
        [TestCase]
        public void TestClear()
        {
        }
    }
}