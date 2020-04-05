using System;
using NUnit.Framework;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.DataStructures;

namespace UnitTests
{
    [TestFixture]
    public class SimpleNativeDataStructures
    {
        [SetUp]
        public void Init() { }

        [Test]
        public void TestByteReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((byte) 0);

                Assert.That(_simpleNativeBag.count, Is.EqualTo(128));
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

                    for (var i = 0; i < 3; i++)
                    {
                        _simpleNativeBag.Enqueue((byte) 0);
                        _simpleNativeBag.Dequeue<byte>();
                        _simpleNativeBag.Dequeue<byte>();
                    }

                    Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
                }
            });
        }

        [Test]
        public void TestCaseReaderGreaterThanWriter() { }

        [Test]
        public void TestDoofusesScenario()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((uint) i);
                    _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                }

                var index = 0;

                while (_simpleNativeBag.IsEmpty() == false)
                {
                    Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(index));
                    var dequeue = _simpleNativeBag.Dequeue<EGID>();
                    index++;
                    Assert.That(_simpleNativeBag.count == 32 * 12 - index * 12);
                    Assert.That(dequeue.entityID, Is.EqualTo(1));
                    Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                }
            }
        }

        [Test]
        public void TestDoofusesScenario2()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                var dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                _simpleNativeBag.Enqueue((uint) 1);
                _simpleNativeBag.Enqueue(new EGID(1, new ExclusiveGroupStruct()));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
                Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(1));
                dequeue = _simpleNativeBag.Dequeue<EGID>();
                Assert.That(dequeue.entityID, Is.EqualTo(1));
                Assert.That((uint) dequeue.groupID, Is.EqualTo(0));
            }
        }

        [Test]
        public void TestEnqueueDequeueWontAlloc()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Dequeue<byte>();
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(4));
            }
        }

        [Test]
        public void TestEnqueueDequeueWontAllocTooMuch()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
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

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestEnqueueTwiceDequeueOnceLeavesWithHalfOfTheEntities()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Dequeue<byte>();
                }

                Assert.That(_simpleNativeBag.count, Is.EqualTo(128));
            }
        }

        [Test]
        public void TestLongReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((long) 0);

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32 * 8));
            }
        }

        [Test]
        public void TestMixedReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 2; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((uint) 0);
                    _simpleNativeBag.Enqueue((long) 0);
                }

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
            }
        }

        [Test]
        public void TestUintReallocWorks()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                    _simpleNativeBag.Enqueue((uint) 0);

                Assert.That(_simpleNativeBag.count, Is.EqualTo(32 * 4));
            }
        }

        [Test]
        public void TestWhatYouEnqueueIsWhatIDequeue()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
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

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(16));
            }
        }

        [Test]
        public void TestWhatYouEnqueueIsWhatIDequeue2()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }

                for (var i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }

                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }

                for (var i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }

                for (var i = 0; i < 32; i++)
                {
                    _simpleNativeBag.Enqueue((byte) 0);
                    _simpleNativeBag.Enqueue((byte) 1);
                    _simpleNativeBag.Enqueue((byte) 2);
                    _simpleNativeBag.Enqueue((byte) 3);
                }

                for (var i = 0; i < 32; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(0));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(1));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(2));
                    Assert.That(_simpleNativeBag.Dequeue<byte>(), Is.EqualTo(3));
                }

                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(556));
            }
        }
        
        [Test]
        public void TestReaderGreaterThanWriter()
        {
            using (var _simpleNativeBag = new SimpleNativeBag(new Allocator()))
            {
                //write 16 uint. The writerHead will be at the end of the array
                for (var i = 0; i < 16; i++)
                {
                    _simpleNativeBag.Enqueue((uint) i);
                }

                Assert.That(_simpleNativeBag.count, Is.EqualTo(64));
                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(96));
                
                //read 8 uint, the readerHead will be in the middle of the array
                for (var i = 0; i < 8; i++)
                {
                    Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(i));
                }
                
                Assert.That(_simpleNativeBag.count, Is.EqualTo(32));
                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(96));
                
                //write 4 uint, now the writer head wrapped and it's before the reader head
                //capacity must stay unchanged
                for (var i = 16; i < 16+7; i++)
                {
                    _simpleNativeBag.Enqueue((uint) i);
                }
                
                Assert.That(_simpleNativeBag.count, Is.EqualTo(60));
                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(96));
                
                //now I will surpass reader, so it won't change the capacity because there is enough space
                for (var i = 16+7; i < 16+7+2; i++)
                {
                    _simpleNativeBag.Enqueue((uint) i);
                }
                
                Assert.That(_simpleNativeBag.count, Is.EqualTo(68));
                Assert.That(_simpleNativeBag.capacity, Is.EqualTo(96));
                
                //dequeue everything and verify values
                int index = 8;
                while (_simpleNativeBag.IsEmpty())
                {
                    Assert.That(_simpleNativeBag.Dequeue<uint>(), Is.EqualTo(index));
                    index++;
                }
            }
        }
    }
}