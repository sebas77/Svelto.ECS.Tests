using NUnit.Framework;
using Svelto.ECS.Experimental;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class EnginesRoot_GenericEntityFunctionsTests : GenericTestsBaseClass
    {
        [Test]
        public void TestRemoveWithEntityIdAndGroup()
        {
            CreateTestEntity(0, Groups.GroupA);
            CreateTestEntity(1, Groups.GroupA);
            _scheduler.SubmitEntities();

            _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(0, Groups.GroupA);
            _scheduler.SubmitEntities();

            var exists = _entitiesDB.entitiesForTesting.Exists<TestEntityComponent>(0, Groups.GroupA);
            Assert.IsFalse(exists, "Entity should be removed from target group");

            var count = _entitiesDB.entitiesForTesting.Count<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(1, count, "Other entities should not be removed");

            void RemoveNotFound()
            {
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(0, Groups.GroupA);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(RemoveNotFound,
                "When removing non created entities an exception should be thrown");
        }

        [Test]
        public void TestRemoveWithEgid()
        {
            CreateTestEntity(0, Groups.GroupA);
            CreateTestEntity(1, Groups.GroupA);
            _scheduler.SubmitEntities();

            var egid = new EGID(0, Groups.GroupA);
            _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(egid);
            _scheduler.SubmitEntities();

            var exists = _entitiesDB.entitiesForTesting.Exists<TestEntityComponent>(0, Groups.GroupA);
            Assert.IsFalse(exists, "Entity should be removed from target group");

            var count = _entitiesDB.entitiesForTesting.Count<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(1, count, "Other entities should not be removed");

            void RemoveNotFound()
            {
                _functions.RemoveEntity<EntityDescriptorWithComponentAndViewComponent>(new EGID(0, Groups.GroupA));
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(RemoveNotFound,
                "When removing non created entities an exception should be thrown");
        }

        [Test]
        public void TestRemoveGroupAndEntities()
        {
            CreateTestEntity(0, Groups.GroupA);
            CreateTestEntity(1, Groups.GroupA);
            CreateTestEntity(1, Groups.GroupB);
            _scheduler.SubmitEntities();

            _functions.RemoveEntitiesFromGroup(Groups.GroupA);
            _scheduler.SubmitEntities();

            var query = new QueryGroups(GroupAB).Evaluate();

            var entityCount = query.Count<TestEntityComponent>(_entitiesDB.entitiesForTesting);
            Assert.AreEqual(1, entityCount, "Entities in the target group should be removed");
        }

        [Test]
        public void TestSwapEntitiesInGroup()
        {
            // todo: Test what happens when source group is empty?

            CreateTestEntity(0, Groups.GroupA, 0);
            CreateTestEntity(1, Groups.GroupA, 1);
            CreateTestEntity(2, Groups.GroupA, 2);
            _scheduler.SubmitEntities();

            _functions.SwapEntitiesInGroup(Groups.GroupA, Groups.GroupB);
            _scheduler.SubmitEntities();

            var countA = _entitiesDB.entitiesForTesting.Count<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(0, countA, "Source group should be empty after swap");

            var (componentsB, countB) = _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(Groups.GroupB);
            Assert.AreEqual(3, countB, "All entities should exist in target group after swap");
            Assert.AreEqual(2, componentsB[2].intValue, "Values in components should be copied after swap");

            // todo: What is the expected behaviour when source entity id is already used in target group?
        }

        [Test]
        public void TestSwapEntityFromEgidToEgid()
        {
            var initializer = CreateTestEntity(0, Groups.GroupA);
            
            var testEntityComponent     = initializer.Get<TestEntityComponent>();
            var testEntityViewComponent = initializer.Get<TestEntityViewComponent>();

            _scheduler.SubmitEntities();
            
            var fromEgid = new EGID(0, Groups.GroupA);
            var toEgid   = new EGID(1, Groups.GroupB);
            
            var (componentA, componentViewA, countA) =
                _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent, TestEntityViewComponent>(Groups.GroupA);

            Assert.AreEqual(1, countA, "An entity should exist in target Group");
            Assert.AreEqual(fromEgid.entityID, componentA[0].ID.entityID,
                "Swapped entity should have the target entityID");
            
            Assert.AreEqual(testEntityComponent.floatValue, componentA[0].floatValue, "Component values should be copied");
            Assert.AreEqual(testEntityComponent.intValue, componentA[0].intValue, "Component values should be copied");
            
            Assert.AreEqual(testEntityViewComponent.TestFloatValue.Value, componentViewA[0].TestFloatValue.Value, "ViewComponent values should be copied");
            Assert.AreEqual(testEntityViewComponent.TestIntValue.Value, componentViewA[0].TestIntValue.Value, "ViewComponent values should be copied");
            
            _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(fromEgid, toEgid);
            _scheduler.SubmitEntities();
            
            var (componentB, componentViewB, countB) =
                _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent, TestEntityViewComponent>(Groups.GroupB);

            Assert.AreEqual(1, countB, "An entity should exist in target Group");
            Assert.AreEqual(toEgid.entityID, componentB[0].ID.entityID,
                "Swapped entity should have the target entityID");
            
            Assert.AreEqual(testEntityComponent.floatValue, componentB[0].floatValue, "Component values should be copied");
            Assert.AreEqual(testEntityComponent.intValue, componentB[0].intValue, "Component values should be copied");
            
            Assert.AreEqual(testEntityViewComponent.TestFloatValue.Value, componentViewB[0].TestFloatValue.Value, "ViewComponent values should be copied");
            Assert.AreEqual(testEntityViewComponent.TestIntValue.Value, componentViewB[0].TestIntValue.Value, "ViewComponent values should be copied");

            var existsA = _entitiesDB.entitiesForTesting.Exists<TestEntityComponent>(0, Groups.GroupA);
            Assert.IsFalse(existsA, "Entity should not be present in source Group anymore");

            void SwapEntityAlreadyExists()
            {
                CreateTestEntity(2, Groups.GroupA);
                CreateTestEntity(2, Groups.GroupB);
                fromEgid = new EGID(0, Groups.GroupA);
                toEgid   = new EGID(0, Groups.GroupB);
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(fromEgid, toEgid);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(SwapEntityAlreadyExists,
                "When target EGID already exists it should throw an exception");

            void SwapEntityNotFound()
            {
                fromEgid = new EGID(3, Groups.GroupA);
                toEgid   = new EGID(3, Groups.GroupB);
                _functions.SwapEntityGroup<EntityDescriptorWithComponentAndViewComponent>(fromEgid, toEgid);
                _scheduler.SubmitEntities();
            }

            Assert.Throws<ECSException>(SwapEntityNotFound,
                "When source EGID doesn't exists it should throw an exception");
        }
    }
}