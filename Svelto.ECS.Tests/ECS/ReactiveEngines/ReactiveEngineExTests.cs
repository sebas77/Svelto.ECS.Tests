using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class ReactiveEngineExTests : GenericTestsBaseClass
    {
        [TestCase]
        public void Test_ReactiveEngineEx_AddCallback()
        {
            var engine = new ReactOnAddExEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            for (uint i = 0; i < 5; i++)
                CreateTestEntity(i, GroupB, (int) i);

            _scheduler.SubmitEntities();

            Assert.AreEqual(15, engine.addedCount);
        }

        [TestCase]
        public void Test_ReactiveEngineEx_MoveCallback()
        {
            var engine = new ReactOnMoveExEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            for (uint i = 10; i < 20; i++)
                CreateTestEntity(i, GroupB, (int) i);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; ++i)
            {
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupA, GroupB);
            }

            for (uint i = 10; i < 12; ++i)
            {
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupB, GroupA);
            }

            _scheduler.SubmitEntities();

            Assert.AreEqual(7, engine.movedCount);
        }

        [TestCase]
        public void Test_ReactiveEngineEx_RemoveCallback()
        {
            var engine = new ReactOnRemoveExEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            for (uint i = 10; i < 20; i++)
                CreateTestEntity(i, GroupB, (int) i);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; ++i)
            {
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupA);
            }

            for (uint i = 10; i < 12; ++i)
            {
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupB);
            }

            _scheduler.SubmitEntities();

            Assert.AreEqual(7, engine.removedCount);
        }

        public class ReactOnAddExEngine : IReactOnAddEx<TestEntityComponent>
        {
            public uint addedCount = 0;

            public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection, ExclusiveGroupStruct groupID)
            {
                addedCount += rangeOfEntities.end - rangeOfEntities.start;
            }
        }

        public class ReactOnMoveExEngine : IReactOnSwapEx<TestEntityComponent>
        {
            public uint movedCount = 0;

            public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection, ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
            {
                movedCount += rangeOfEntities.end - rangeOfEntities.start;
            }
        }

        public class ReactOnRemoveExEngine : IReactOnRemoveEx<TestEntityComponent>
        {
            public uint removedCount = 0;

            public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection, ExclusiveGroupStruct groupID)
            {
                removedCount += rangeOfEntities.end - rangeOfEntities.start;
            }
        }
    }
}