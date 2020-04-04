using System;
using NUnit.Framework;
using Svelto.Common;
using Svelto.ECS.DataStructures;

namespace UnitTests
{
    [TestFixture]
    public class SimpleNativeDataStructures
    {
        [SetUp]
        public void Init()
        {
        }
        
        [Test]
        public void TestByteReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((byte) 0);

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
            }
        }
        
        [Test]
        public void TestUintReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((uint) 0);

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32 * 4));
            }
        }
        
        [Test]
        public void TestLongReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((long)0);
            
                Assert.That(_simpleNativeBag.count, Is.EqualTo(32*8));
            }
        }
        
        [Test]
        public void TestMixedReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 2; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((uint) 0);
                    _simpleNativeBag.Enqueue((long) 0);
                }

                Assert.That(_simpleNativeBag.count, Is.EqualTo(26));
            }
        }
        
        [Test]
        public void TestEnqueueDequeueWontAlloc()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Dequeue<byte>();
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(1));
            }
        }
        
        [Test]
        public void TestEnqueueDequeueWontAllocTooMuch()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Dequeue<byte>();
                    _simpleNativeBag.Dequeue<byte>();
                    _simpleNativeBag.Dequeue<byte>();
                    _simpleNativeBag.Dequeue<byte>();
                    
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(6));
            }
        }
        
        [Test]
        public void TestWhatYouEnqueueIsWhatIDequeue()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                    
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(6));
            }
        }
        
        [Test]
        public void TestWhatYouEnqueueIsWhatIDequeue2()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }
                
                for (int i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }
                
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }
                
                for (int i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }
                
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }
                
                for (int i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(138));
            }
        }
        
        [Test]
        public void TestEnqueueTwiceDequeueOnceLeavesWithHalfOfTheEntities()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (int i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Dequeue<byte>();
                }

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
            }
        }
        
        [Test]
        public void TestCantDequeMoreThanQueue()
        {
            Assert.Throws<Exception>(() =>
            {
                using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);


                    for (int i = 0; i < 3; i++)
                    {
                        _simpleNativeBag.Enqueue((byte) 0);
                        _simpleNativeBag.Dequeue<byte>();
                        _simpleNativeBag.Dequeue<byte>();
                    }

                    Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
                }
            });
        }
    }
}