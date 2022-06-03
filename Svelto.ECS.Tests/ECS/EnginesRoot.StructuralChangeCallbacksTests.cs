using System;
using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    public class MegaReactEngine : IReactOnAddAndRemove<TestEntityComponent>, IReactOnDispose<TestEntityComponent>, IReactOnSubmission,
                                   IReactOnSwap<TestEntityComponent>, IReactOnAddEx<TestEntityComponent>, IReactOnRemoveEx<TestEntityComponent>,
                                   IReactOnSwapEx<TestEntityComponent>
    {
        public int  addCounter;
        public int  removeCounter;
        
        public int  legacyAddCounter;
        public int  legacyRemoveCounter;
        
        public int legacySwapCounter;
        public int swapCounter;
        
        public bool entitySubmittedIsCalled;
        public int  removeCounterOnDispose;
        

        public void Add(ref TestEntityComponent entityComponent, EGID egid)
        {
            legacyAddCounter += entityComponent.intValue;
        }

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                addCounter += entityComponent.intValue;
            }
        }

        void IReactOnDispose<TestEntityComponent>.Remove(ref TestEntityComponent entityComponent, EGID egid)
        {
            removeCounterOnDispose += entityComponent.intValue;
        }

        void IReactOnRemove<TestEntityComponent>.Remove(ref TestEntityComponent entityComponent, EGID egid)
        {
            legacyRemoveCounter += entityComponent.intValue;
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                removeCounter += entityComponent.intValue;
            }
        }

        public void EntitiesSubmitted()
        {
            entitySubmittedIsCalled = true;
        }

        public void MovedTo(ref TestEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            legacySwapCounter += entityComponent.intValue;
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                swapCounter += entityComponent.intValue;
            }
        }
    }

    public class MegaReactEngineView : IReactOnAddAndRemove<TestEntityViewComponent>, IReactOnDispose<TestEntityViewComponent>,
                                       IReactOnSwap<TestEntityViewComponent>, IReactOnAddEx<TestEntityViewComponent>,
                                       IReactOnRemoveEx<TestEntityViewComponent>, IReactOnSwapEx<TestEntityViewComponent>
    {
        public int  addCounter;
        public int  removeCounter;
        
        public int  legacyAddCounter;
        public int  legacyRemoveCounter;
        
        public int legacySwapCounter;
        public int swapCounter;
        
        public int  removeCounterOnDispose;
       
        public void Add(ref TestEntityViewComponent entityComponent, EGID egid)
        {
            legacyAddCounter += entityComponent.TestIntValue.Value;
        }

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                addCounter += entityComponent.TestIntValue.Value;
            }
        }

        void IReactOnRemove<TestEntityViewComponent>.Remove(ref TestEntityViewComponent entityComponent, EGID egid)
        {
            legacyRemoveCounter += entityComponent.TestIntValue.Value;
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                removeCounter += entityComponent.TestIntValue.Value;
            }
        }

        public void MovedTo(ref TestEntityViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
        {
            legacySwapCounter += entityComponent.TestIntValue.Value;
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (buffer, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                var entityComponent = buffer[i];
                swapCounter += entityComponent.TestIntValue.Value;
            }
        }
        
        void IReactOnDispose<TestEntityViewComponent>.Remove(ref TestEntityViewComponent entityComponent, EGID egid)
        {
            removeCounterOnDispose += entityComponent.TestIntValue.Value;
        }
        
    }

    [TestFixture]
    public class EnginesRoot_StructuralChangeCallbacksTests : GenericTestsBaseClass
    {
        [Test]
        public void TestCallbacksAreCorrectlyCalled()
        {
            uint total = 0;

            for (uint i = 0; i < 100; i++)
            {
                total += i;
                CreateTestEntity(i, Groups.GroupA, (int)i);
            }

            _scheduler.SubmitEntities();

            var megaReactEngine = new MegaReactEngine();

            _enginesRoot.AddEngine(megaReactEngine);

            for (uint i = 0; i < 100; i++)
                CreateTestEntity(i + 100, Groups.GroupA);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyAddCounter, Is.EqualTo(100));
            Assert.That(megaReactEngine.addCounter, Is.EqualTo(100));
            
            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(new EGID(i, Groups.GroupA), Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacySwapCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.swapCounter, Is.EqualTo(total));

            for (uint i = 0; i < 100; i++)
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(i, Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total));
            
            Assert.That(megaReactEngine.entitySubmittedIsCalled, Is.EqualTo(true));

            _enginesRoot.Dispose();

            Assert.That(megaReactEngine.removeCounterOnDispose, Is.EqualTo(100));
        }
        
        [Test]
        public void TestCallbacksAreCorrectlyCalledView()
        {
            uint total = 0;

            for (uint i = 0; i < 100; i++)
            {
                total += i;
                CreateTestEntity(i, Groups.GroupA, (int)i);
            }
            _scheduler.SubmitEntities();

            var megaReactEngine = new MegaReactEngineView();

            _enginesRoot.AddEngine(megaReactEngine);

            for (uint i = 0; i < 100; i++)
            {
                CreateTestEntity(i + 100, Groups.GroupA, (int)i);
            }

            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyAddCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.addCounter, Is.EqualTo(total));

            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(new EGID(i, Groups.GroupA), Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacySwapCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.swapCounter, Is.EqualTo(total));

            for (uint i = 0; i < 100; i++)
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(i, Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total));
            
            for (uint i = 0; i < 100; i++) 
                CreateTestEntity(i, Groups.GroupB, (int)i);
            _scheduler.SubmitEntities();
            
            _functions.SwapEntitiesInGroup(Groups.GroupB, Groups.GroupA);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacySwapCounter, Is.EqualTo(total * 2));
            Assert.That(megaReactEngine.swapCounter, Is.EqualTo(total * 2));
            
            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>
                    (new EGID(i, Groups.GroupA), Groups.GroupB);
            _scheduler.SubmitEntities();
            
            _functions.RemoveEntitiesFromGroup(Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total * 2));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total * 2));
            
            _enginesRoot.Dispose();

            Assert.That(megaReactEngine.removeCounterOnDispose, Is.EqualTo(total));
        }
    }
}