using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class EnginesRoot_GenericEntityFunctionsTests
    {
        [SetUp]
        public void Init()
        {
            _scheduler   = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            _factory = _enginesRoot.GenerateEntityFactory();
            _functions = _enginesRoot.GenerateEntityFunctions();
            _engine = new TestEngine();

            _enginesRoot.AddEngine(_engine);
        }

        [TearDown]
        public void Cleanup()
        {
            _functions.RemoveAllEntities<TestEntityWithComponentViewAndComponentStruct>();
            _scheduler.SubmitEntities();
        }

        [Test]
        public void TestRemoveEntityWithEntityIdAndGroup()
        {
            CreateTestEntity(0, GroupA);
            CreateTestEntity(1, GroupA);
            _scheduler.SubmitEntities();

            _functions.RemoveEntity<TestEntityWithComponentViewAndComponentStruct>(0, GroupA);
            _scheduler.SubmitEntities();

            var exists = _engine.entitiesDB.Exists<TestEntityStruct>(0, GroupA);
            Assert.IsFalse(exists, "Entity should be removed from target group");

            var count = _engine.entitiesDB.Count<TestEntityStruct>(GroupA);
            Assert.AreEqual(1, count, "Other entities should not be removed");

            void RemoveEntityNotFound()
            {
                _functions.RemoveEntity<TestEntityWithComponentViewAndComponentStruct>(0, GroupA);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(RemoveEntityNotFound, "When removing non created entities an exception should be thrown");
        }

        [Test]
        public void TestRemoveEntityWithEgid()
        {
            CreateTestEntity(0, GroupA);
            CreateTestEntity(1, GroupA);
            _scheduler.SubmitEntities();

            var egid = new EGID(0, GroupA);
            _functions.RemoveEntity<TestEntityWithComponentViewAndComponentStruct>(egid);
            _scheduler.SubmitEntities();

            var exists = _engine.entitiesDB.Exists<TestEntityStruct>(0, GroupA);
            Assert.IsFalse(exists, "Entity should be removed from target group");

            var count = _engine.entitiesDB.Count<TestEntityStruct>(GroupA);
            Assert.AreEqual(1, count, "Other entities should not be removed");

            void RemoveEntityNotFound()
            {
                _functions.RemoveEntity<TestEntityWithComponentViewAndComponentStruct>(new EGID(0, GroupA));
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(RemoveEntityNotFound, "When removing non created entities an exception should be thrown");
        }

        [Test]
        public void TestRemoveAllEntitiesWithGroup()
        {
            CreateTestEntity(0, GroupA);
            CreateTestEntity(1, GroupA);
            CreateTestEntity(2, GroupA);
            _scheduler.SubmitEntities();

            _functions.RemoveAllEntities<TestEntityWithComponentViewAndComponentStruct>(GroupA);
            _scheduler.SubmitEntities();

            var countA = _engine.entitiesDB.Count<TestEntityStruct>(GroupA);
            Assert.AreEqual(0, countA, "Target group should be empty after remove all entities");
        }

        [Test]
        public void TestSwapEntityFromEgidToEgid()
        {
            CreateTestEntity(0, GroupA);

            _scheduler.SubmitEntities();

            var fromEgid = new EGID(0, GroupA);
            var toEgid = new EGID(1, GroupB);
            _functions.SwapEntityGroup<TestEntityWithComponentViewAndComponentStruct>(fromEgid, toEgid);

            _scheduler.SubmitEntities();

            var (componentB, componentViewB, countB) = _engine.entitiesDB.QueryEntities<TestEntityStruct, TestEntityViewStruct>(GroupB);

            Assert.AreEqual(1, countB, "An entity should exist in target Group");
            Assert.AreEqual(toEgid.entityID, componentB[0].ID.entityID, "Swapped entity should have the target entityID");
            Assert.AreEqual(1f, componentB[0].floatValue, "Component values should be copied");
            Assert.AreEqual(1, componentB[0].intValue, "Component values should be copied");
            Assert.AreEqual(1f, componentViewB[0].TestFloatValue.Value, "ViewComponent values should be copied");
            Assert.AreEqual(1, componentViewB[0].TestIntValue.Value, "ViewComponent values should be copied");

            var existsA = _engine.entitiesDB.Exists<TestEntityStruct>(0, GroupA);
            Assert.IsFalse(existsA, "Entity should not be present in source Group anymore");

            void SwapEntityAlreadyExists()
            {
                CreateTestEntity(2, GroupA);
                CreateTestEntity(2, GroupB);
                fromEgid = new EGID(0, GroupA);
                toEgid = new EGID(0, GroupB);
                _functions.SwapEntityGroup<TestEntityWithComponentViewAndComponentStruct>(fromEgid, toEgid);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(SwapEntityAlreadyExists, "When target EGID already exists it should throw an exception");

            void SwapEntityNotFound()
            {
                fromEgid = new EGID(3, GroupA);
                toEgid = new EGID(3, GroupB);
                _functions.SwapEntityGroup<TestEntityWithComponentViewAndComponentStruct>(fromEgid, toEgid);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(SwapEntityAlreadyExists, "When source EGID doesn't exists it should throw an exception");
        }

        EntityComponentInitializer CreateTestEntity(uint entityId, ExclusiveGroupStruct group, int value = 1)
        {
            var initializer = _factory.BuildEntity<TestEntityWithComponentViewAndComponentStruct>
                (entityId, group, new object[] {new TestFloatValue(value), new TestIntValue(value)});
            initializer.Init(new TestEntityStruct(value, value));
            return initializer;
        }

        SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot _enginesRoot;
        IEntityFactory _factory;
        IEntityFunctions _functions;
        TestEngine _engine;

        static ExclusiveGroup GroupA = new ExclusiveGroup();
        static ExclusiveGroup GroupB = new ExclusiveGroup();
    }
}