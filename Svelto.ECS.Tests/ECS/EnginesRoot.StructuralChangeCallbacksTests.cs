using System;
using NUnit.Framework;
using Svelto.ECS.Experimental;

namespace Svelto.ECS.Tests.ECS
{
    public class MegaReactEngine : IReactOnAddAndRemove<TestEntityComponent>, IReactOnDispose<TestEntityComponent>, IReactOnSubmission,
                                   IReactOnSwap<TestEntityComponent>, IReactOnAddEx<TestEntityComponent>, IReactOnRemoveEx<TestEntityComponent>,
                                   IReactOnSwapEx<TestEntityComponent>, IReactOnDisposeEx<TestEntityComponent>, IDisposableEngine
    {
        public int  addCounter;
        public int  removeCounter;
        
        public int  legacyAddCounter;
        public int  legacyRemoveCounter;
        
        public int legacySwapCounter;
        public int swapCounter;
        
        public bool entitySubmittedIsCalled;
        public int  legacyRemoveCounterOnDispose;
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
            legacyRemoveCounterOnDispose += entityComponent.intValue;
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
                if (isDisposing)
                    removeCounterOnDispose += entityComponent.intValue;
                else
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

        public void Dispose()
        {
            
        }

        public bool isDisposing { get; set; }
    }

    public class MegaReactEngineCallback : IReactOnAddEx<TestEntityViewComponent>,
                                       IReactOnRemoveEx<TestEntityViewComponent>, IReactOnSwapEx<TestEntityViewComponent>
    {
        readonly IEntityFunctions _generateEntityFunctions;
        readonly IEntityFactory _generateEntityFactory;

        public MegaReactEngineCallback(IEntityFunctions generateEntityFunctions, IEntityFactory generateEntityFactory)
        {
            _generateEntityFunctions = generateEntityFunctions;
            _generateEntityFactory = generateEntityFactory;
        }

        public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (_, ids, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                _generateEntityFunctions.SwapEntityGroup<EntityDescriptorWithComponents>(ids[i], groupID, Groups.GroupB);
            }
        }

        public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct groupID)
        {
            var (_, ids, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                _generateEntityFactory.BuildEntity<EntityDescriptorWithComponents>(ids[i], groupID);
            }
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var (_, ids, _) = collection;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                _generateEntityFunctions.RemoveEntity<EntityDescriptorWithComponents>(ids[i], toGroup);
            }
        }
    }

    [TestFixture]
    public class EnginesRoot_StructuralChangeCallbacksTests : GenericTestsBaseClass
    {
        [Test]
        public void TestStructuralChangesInCallbacks()
        {
            var megaReactEngine = new MegaReactEngineCallback(_enginesRoot.GenerateEntityFunctions(), _enginesRoot.GenerateEntityFactory());

            _enginesRoot.AddEngine(megaReactEngine);

            for (uint i = 0; i < 100; i++)
            {
                _factory.BuildEntity<EntityDescriptorWithComponents>(i, Groups.GroupA);
            }
            
            //the add callback will swap the entity to group B, the move callback will remove the entity from group B
            //the remove will add them back
            Assert.DoesNotThrow(_scheduler.SubmitEntities);
            var count = new QueryGroups(_entitiesDB.entitiesForTesting.FindGroups<TestEntityComponent>()).Evaluate().Count<TestEntityComponent>(_entitiesDB.entitiesForTesting);
            
            Assert.That(count, Is.EqualTo(100));
        }

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

            Assert.That(megaReactEngine.legacyRemoveCounterOnDispose, Is.EqualTo(100));
            Assert.That(megaReactEngine.removeCounterOnDispose, Is.EqualTo(100));
        }
        
        [Test]
        public void TestCallbacksAreCorrectlyCalledGroup()
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

            _functions.SwapEntitiesInGroup(Groups.GroupA, Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacySwapCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.swapCounter, Is.EqualTo(total));

            _functions.RemoveEntitiesFromGroup(Groups.GroupB);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total));
            
            Assert.That(megaReactEngine.entitySubmittedIsCalled, Is.EqualTo(true));
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

            // Swap specifically EGID to EGID with different Ids
            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>
                    (new EGID(i, Groups.GroupB), new EGID(i + 200,Groups.GroupA));
            _scheduler.SubmitEntities();
            
            _functions.RemoveEntitiesFromGroup(Groups.GroupA);
            _scheduler.SubmitEntities();
            
            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total * 3));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total * 3));
            
            for (uint i = 0; i < 100; i++)
            {
                CreateTestEntity(i, Groups.GroupA, (int)i);
            }
            _scheduler.SubmitEntities();
            _enginesRoot.Dispose();

            Assert.That(megaReactEngine.removeCounterOnDispose, Is.EqualTo(total));
            Assert.That(megaReactEngine.legacyRemoveCounterOnDispose, Is.EqualTo(total));
            
        }
    }
}