using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class ReactiveEngineTests : GenericTestsBaseClass
    {
        [TestCase]
        public void Test_ReactiveEngine_AddCallback()
        {
            var engine = new ReactOnAddEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            for (uint i = 0; i < 5; i++)
                CreateTestEntity(i, GroupB, (int) i);

            _scheduler.SubmitEntities();

            Assert.AreEqual(15, engine.addedCount);
        }

        [TestCase]
        public void Test_ReactiveEngine_MoveCallback()
        {
            var engine = new ReactOnMoveEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; ++i)
            {
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupA, GroupB);
            }

            _scheduler.SubmitEntities();

            Assert.AreEqual(5, engine.movedCount);
        }

        [TestCase]
        public void Test_ReactiveEngine_RemoveCallback()
        {
            var engine = new ReactOnRemoveEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, GroupA, (int) i);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; ++i)
            {
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(
                    i, GroupA);
            }

            _scheduler.SubmitEntities();

            Assert.AreEqual(5, engine.removedCount);
        }

        public class ReactOnAddEngine : IReactOnAdd<TestEntityComponent>
        {
            public int addedCount = 0;

            public void Add(ref TestEntityComponent entityComponent, EGID egid)
            {
                addedCount++;
            }
        }

        public class ReactOnMoveEngine : IReactOnSwap<TestEntityComponent>
        {
            public int movedCount = 0;

            public void MovedTo(ref TestEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
            {
                movedCount++;
            }
        }

        public class ReactOnRemoveEngine : IReactOnRemove<TestEntityComponent>
        {
            public int removedCount = 0;

            public void Remove(ref TestEntityComponent entityComponent, EGID egid)
            {
                removedCount++;
            }
        }
    }
}