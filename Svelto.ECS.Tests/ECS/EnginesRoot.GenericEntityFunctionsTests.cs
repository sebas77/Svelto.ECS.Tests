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
            _functions.RemoveAllEntities<TestEntityWithComponentViewAndComponentStruct>(GroupA);
            _functions.RemoveAllEntities<TestEntityWithComponentViewAndComponentStruct>(GroupB);
            _scheduler.SubmitEntities();
        }

        [Test]
        public void TestSwapEntityFromEgidToEgid()
        {
            var initializer = _factory.BuildEntity<TestEntityWithComponentViewAndComponentStruct>
                (0, GroupA, new object[] {new TestFloatValue(1f), new TestIntValue(1)});
            initializer.Init(new TestEntityStruct(1f, 1));

            _scheduler.SubmitEntities();

            var fromEgid = new EGID(0, GroupA);
            var toEgid = new EGID(1, GroupB);
            _functions.SwapEntityGroup<TestEntityWithComponentViewAndComponentStruct>(fromEgid, toEgid);

            _scheduler.SubmitEntities();

            var (componentB, componentViewB, countB) = _engine.entitiesDB.QueryEntities<TestEntityStruct, TestEntityViewStruct>(GroupB);

            Assert.AreEqual(1, countB, "An entity should exist in Group B");
            Assert.AreEqual(toEgid.entityID, componentB[0].ID.entityID, "Swapped entity should have the target entityID");
            Assert.AreEqual(1f, componentB[0].floatValue, "Component values should be copied");
            Assert.AreEqual(1, componentB[0].intValue, "Component values should be copied");
            Assert.AreEqual(1f, componentViewB[0].TestFloatValue.Value, "ViewComponent values should be copied");
            Assert.AreEqual(1, componentViewB[0].TestIntValue.Value, "ViewComponent values should be copied");
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