using System;
using NUnit.Framework;
using Svelto.ECS.Experimental;

namespace Svelto.ECS.Tests.ECS
{
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
            var count = new QueryGroups(_entitiesDB.entitiesForTesting.FindGroups<TestEntityComponent>()).Evaluate()
                   .Count<TestEntityComponent>(_entitiesDB.entitiesForTesting);

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
            Assert.That(megaReactEngine.swapCounter, Is.EqualTo(total       * 2));

            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(new EGID(i, Groups.GroupA), Groups.GroupB);
            _scheduler.SubmitEntities();

            // Swap specifically EGID to EGID with different Ids
            for (uint i = 0; i < 100; i++)
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(new EGID(i, Groups.GroupB),
                    new EGID(i + 200, Groups.GroupA));
            _scheduler.SubmitEntities();

            _functions.RemoveEntitiesFromGroup(Groups.GroupA);
            _scheduler.SubmitEntities();

            Assert.That(megaReactEngine.legacyRemoveCounter, Is.EqualTo(total * 3));
            Assert.That(megaReactEngine.removeCounter, Is.EqualTo(total       * 3));

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