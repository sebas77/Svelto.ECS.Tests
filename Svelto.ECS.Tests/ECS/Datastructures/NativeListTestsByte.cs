using NUnit.Framework;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures;
using Assert = NUnit.Framework.Assert;

namespace Svelto.ECS.Tests.NativeDataStructures
{
    [TestFixture]
    class NativeListTestsByte
    {
        [TestCase]
        public void TestAllocationSize0()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);

            Assert.That(fasterList.capacity, Is.EqualTo(0));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestAllocationSize1()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(1, Allocator.Persistent);

            Assert.That(fasterList.capacity, Is.EqualTo(1));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestResize()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
            
            fasterList.Resize(10);

            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        [TestCase]
        public void TestResizeTo0()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(10, Allocator.Persistent);
            
            fasterList.Resize(0);

            Assert.That(fasterList.capacity, Is.EqualTo(0));
            Assert.That(fasterList.count, Is.EqualTo(0));
            
            fasterList.Dispose();
        }
        
        // [TestCase]
        // public void TestExpandTo()
        // {
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
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
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
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
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
        //     
        //     fasterList.ExpandTo(10);
        //     
        //     for (var i = 0; i < 10; i++)
        //         fasterList[i] = (byte) i;
        //     
        //     for (var i = 0; i < 10; i++)
        //         Assert.That(fasterList[i], Is.EqualTo(i));
        //     
        //     fasterList.Dispose();
        // }

        [TestCase]
        public void TestAdd()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);

            for (var i = 0; i < 10; i++)
                fasterList.Add((byte) i);
            
            for (var i = 0; i < 10; i++)
                Assert.That(fasterList[i], Is.EqualTo(i));

            Assert.That(fasterList.capacity, Is.EqualTo(10));
            Assert.That(fasterList.count, Is.EqualTo(10));
            
            fasterList.Dispose();
        }
        
        // [TestCase]
        // public void TestExpandBy()
        // {
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(3, Allocator.Persistent);
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
        //     NativeDynamicArrayCast<byte> fasterListA = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
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
        //     NativeDynamicArrayCast<byte> fasterListA = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
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
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
        //
        //     for (var i = 0; i < 10; i++)
        //         fasterList.Push((byte) i);
        //     
        //     for (var i = 0; i < 10; i++)
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
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
        //
        //     for (var i = 0; i < 10; i++)
        //         fasterList.Push((byte) i);
        //     
        //     for (var i = 9; i >= 0; i--)
        //         Assert.That(fasterList.Pop(), Is.EqualTo(i));
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(0));
        //     
        //     fasterList.Dispose();
        // }
        
        // [TestCase]
        // public void TestPeek()
        // {
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
        //
        //     for (var i = 0; i < 10; i++)
        //         fasterList.Push((byte) i);
        //     
        //     Assert.That(fasterList.Peek(), Is.EqualTo(9));
        //
        //     Assert.That(fasterList.capacity, Is.EqualTo(10));
        //     Assert.That(fasterList.count, Is.EqualTo(10));
        //     
        //     fasterList.Dispose();
        // }
        
        [TestCase]
        public void TestRemoveAt()
        {
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);

            for (var i = 0; i < 10; i++)
                fasterList.Add((byte) i);
            
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
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);

            for (var i = 0; i < 10; i++)
                fasterList.Add((byte) i);
            
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
        //     NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
        //
        //     for (var i = 0; i < 10; i++)
        //         fasterList.Add((byte) i);
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
            NativeDynamicArrayCast<byte> fasterList = new NativeDynamicArrayCast<byte>(0, Allocator.Persistent);
            
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