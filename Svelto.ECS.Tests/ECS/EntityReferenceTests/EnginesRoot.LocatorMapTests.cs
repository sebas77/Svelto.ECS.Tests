using NUnit.Framework;
using Svelto.ECS.Reference;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS
{
    public class EnginesRoot_LocatorMapTests: GenericTestsBaseClass
    {
        [Test]
        public void TestEntityReferenceSequenceCreation()
        {
            for (uint i = 0; i < 10; i++)
            {
                _factory.BuildEntity<TestDescriptor>(i, TestGroupA);
            }

            _scheduler.SubmitEntities();

            var (_, ids, count) = _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(TestGroupA);
            for (uint i = 0; i < count; i++)
            {
                Assert.AreEqual(new EntityReference(i + 1, 0), _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(ids[i], TestGroupA)));
            }
        }

        [Test]
        public void TestEntityReferenceInvalidation()
        {
            var egid = new EGID(0, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(egid.entityID, egid.groupID);
            _scheduler.SubmitEntities();

            var entityReference = _entitiesDB.entitiesForTesting.GetEntityReference(egid);
            _functions.RemoveEntity<TestDescriptor>(egid);

            var found = _entitiesDB.entitiesForTesting.TryGetEGID(entityReference, out var foundEgid);
            Assert.IsTrue(found, "Entity reference should still be valid before submit.");

            _scheduler.SubmitEntities();

            found = _entitiesDB.entitiesForTesting.TryGetEGID(entityReference, out foundEgid);
            Assert.IsFalse(found, "Entity reference should be invalidated after removal submission");
        }

        [Test]
        public void TestEntityReferenceReuse()
        {
            var egidA = new EGID(0, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(egidA.entityID, egidA.groupID);
            _scheduler.SubmitEntities();

            _functions.RemoveEntity<TestDescriptor>(egidA);
            _scheduler.SubmitEntities();

            var egidB = new EGID(0, TestGroupB);
            _factory.BuildEntity<TestDescriptor>(egidB.entityID, egidB.groupID);
            _scheduler.SubmitEntities();

            Assert.AreEqual(new EntityReference(1, 1), _entitiesDB.entitiesForTesting.GetEntityReference(egidB));
        }

        [Test]
        public void TestEntityReferenceUpdate()
        {
            var egidA = new EGID(0, TestGroupA);
            var egidB = new EGID(0, TestGroupB);

            _factory.BuildEntity<TestDescriptor>(egidA.entityID, egidA.groupID);
            _scheduler.SubmitEntities();

            var entityReference = _entitiesDB.entitiesForTesting.GetEntityReference(egidA);

            _functions.SwapEntityGroup<TestDescriptor>(egidA, TestGroupB);
            _scheduler.SubmitEntities();

            var found = _entitiesDB.entitiesForTesting.TryGetEGID(entityReference, out var foundEgid);
            Assert.AreEqual(egidB, foundEgid);

            _functions.SwapEntityGroup<TestDescriptor>(egidB, TestGroupA);
            _scheduler.SubmitEntities();

            found = _entitiesDB.entitiesForTesting.TryGetEGID(entityReference, out foundEgid);
            Assert.AreEqual(egidA, foundEgid);
        }

        [Test]
        public void TestEntityReferenceGroupUpdate()
        {
            var egids = new EGID[10];
            var references = new EntityReference[10];
            for (uint i = 0; i < 10; i++)
            {
                egids[i] = new EGID(i, TestGroupA);
                _factory.BuildEntity<TestDescriptor>(egids[i].entityID, TestGroupA);
                references[i] = _entitiesDB.entitiesForTesting.GetEntityReference(egids[i]);
            }

            _scheduler.SubmitEntities();

            _functions.SwapEntitiesInGroup(TestGroupA, TestGroupB);
            _scheduler.SubmitEntities();

            for (var i = 0; i < 10; i++)
            {
                var found = _entitiesDB.entitiesForTesting.TryGetEGID(references[i], out var foundEgid);
                Assert.AreEqual((uint)TestGroupB.id, (uint)foundEgid.groupID.id);
            }
        }

        [Test]
        public void TestRemoveGroupUpdatesReferences()
        {
            var egids = new EGID[10];
            var references = new EntityReference[10];
            for (uint i = 0; i < 10; i++)
            {
                egids[i] = new EGID(i, TestGroupA);
                _factory.BuildEntity<TestDescriptor>(egids[i].entityID, TestGroupA);
                references[i] = _entitiesDB.entitiesForTesting.GetEntityReference(egids[i]);
            }

            _scheduler.SubmitEntities();

            for (var i = 0; i < 10; i++)
            {
                var found = _entitiesDB.entitiesForTesting.TryGetEGID(references[i], out _);
                Assert.That(found, Is.EqualTo(true));
            }

            _functions.RemoveEntitiesFromGroup(TestGroupA);
            _scheduler.SubmitEntities();

            for (var i = 0; i < 10; i++)
            {
                var found = _entitiesDB.entitiesForTesting.TryGetEGID(references[i], out _);
                Assert.That(found, Is.EqualTo(false));
            }
        }

        [Test]
        public void TestRemoveGroupUpdatesReferencesInsideCallbackLegacy()
        {
            _enginesRoot.AddEngine(new RemoveEntityEngineLegacy());

            var egids = new EGID[10];
            var references = new EntityReference[10];
            for (uint i = 0; i < 10; i++)
            {
                egids[i] = new EGID(i, TestGroupA);
                _factory.BuildEntity<TestDescriptor>(egids[i].entityID, TestGroupA);
                var init = _factory.BuildEntity<TestDescriptorWithReference>(egids[i].entityID, TestGroupB);
                references[i] = _entitiesDB.entitiesForTesting.GetEntityReference(egids[i]);
                init.Init(
                    new EntityReferenceComponent()
                    {
                            reference = references[i]
                    });
            }

            _scheduler.SubmitEntities();

            for (var i = 0; i < 10; i++)
            {
                var found = _entitiesDB.entitiesForTesting.TryGetEGID(references[i], out _);
                Assert.That(found, Is.EqualTo(true));
            }

            _functions.RemoveEntitiesFromGroup(TestGroupA);
            _functions.RemoveEntitiesFromGroup(TestGroupB);
            _scheduler.SubmitEntities();
        }

        [Test]
        public void TestRemoveGroupUpdatesReferencesInsideCallback()
        {
            _enginesRoot.AddEngine(new RemoveEntityEngine());

            var egids = new EGID[10];
            var references = new EntityReference[10];
            for (uint i = 0; i < 10; i++)
            {
                egids[i] = new EGID(i, TestGroupA);
                _factory.BuildEntity<TestDescriptor>(egids[i].entityID, TestGroupA);

                var init = _factory.BuildEntity<TestDescriptorWithReference>(egids[i].entityID, TestGroupB);
                references[i] = _entitiesDB.entitiesForTesting.GetEntityReference(egids[i]);
                init.Init(
                    new EntityReferenceComponent()
                    {
                            reference = references[i]
                    });
            }

            _scheduler.SubmitEntities();

            for (var i = 0; i < 10; i++)
            {
                var found = _entitiesDB.entitiesForTesting.TryGetEGID(references[i], out _);
                Assert.That(found, Is.EqualTo(true));
            }

            _functions.RemoveEntitiesFromGroup(TestGroupA);

            _functions.RemoveEntitiesFromGroup(TestGroupB);

            _scheduler.SubmitEntities();
        }

        [Test]
        public void TestAssortedCreationAndRemoval()
        {
            // First submission
            _factory.BuildEntity<TestDescriptor>(0, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(1, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(2, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(0, TestGroupB);
            _scheduler.SubmitEntities();

            // Second submission
            _functions.RemoveEntity<TestDescriptor>(1, TestGroupA);
            _functions.RemoveEntity<TestDescriptor>(0, TestGroupB);
            _factory.BuildEntity<TestDescriptor>(3, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(4, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(1, TestGroupB);
            _scheduler.SubmitEntities();

            // Third submission
            _functions.RemoveEntity<TestDescriptor>(3, TestGroupA);
            _functions.RemoveEntity<TestDescriptor>(1, TestGroupB);
            _factory.BuildEntity<TestDescriptor>(5, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(2, TestGroupB);
            _scheduler.SubmitEntities();

            // Fourth submission
            _factory.BuildEntity<TestDescriptor>(3, TestGroupB);
            _factory.BuildEntity<TestDescriptor>(4, TestGroupB);
            _factory.BuildEntity<TestDescriptor>(6, TestGroupA);
            _factory.BuildEntity<TestDescriptor>(7, TestGroupA);
            _scheduler.SubmitEntities();

            // First submission entities.
            Assert.AreEqual(
                new EntityReference(1, 0),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(0, TestGroupA)));
            Assert.AreEqual(
                new EntityReference(3, 0),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(2, TestGroupA)));

            // Second submission entities.
            Assert.AreEqual(
                new EntityReference(6, 0),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(4, TestGroupA)));

            // Third submission entities.
            Assert.AreEqual(
                new EntityReference(4, 1),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(5, TestGroupA)));
            Assert.AreEqual(
                new EntityReference(2, 1),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(2, TestGroupB)));

            // Fourth submission entities.
            Assert.AreEqual(
                new EntityReference(7, 1),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(3, TestGroupB)));
            Assert.AreEqual(
                new EntityReference(5, 1),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(4, TestGroupB)));
            Assert.AreEqual(
                new EntityReference(8, 0),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(6, TestGroupA)));
            Assert.AreEqual(
                new EntityReference(9, 0),
                _entitiesDB.entitiesForTesting.GetEntityReference(new EGID(7, TestGroupA)));
        }

        public static readonly ExclusiveGroup TestGroupA = new ExclusiveGroup();
        public static readonly ExclusiveGroup TestGroupB = new ExclusiveGroup();

        class TestDescriptor: IEntityDescriptor
        {
            public IComponentBuilder[] componentsToBuild
            {
                get => new IComponentBuilder[]
                {
#if SLOW_SVELTO_SUBMISSION
                    new ComponentBuilder<EGIDComponent>(),
#endif
                        new ComponentBuilder<TestEntityComponentInt>()
                };
            }
        }

        public struct EntityReferenceComponent: IEntityComponent
        {
            public EntityReference reference { get; set; }
        }

        class TestDescriptorWithReference: IEntityDescriptor
        {
            public IComponentBuilder[] componentsToBuild
            {
                get => new IComponentBuilder[]
                {
                        new ComponentBuilder<EntityReferenceComponent>(),
                };
            }
        }

        public class RemoveEntityEngine: IReactOnRemoveEx<EntityReferenceComponent>, IQueryingEntitiesEngine
        {
            public void Remove((uint start, uint end) rangeOfEntities,
                in EntityCollection<EntityReferenceComponent> collection, ExclusiveGroupStruct groupID)
            {
                var (buffer, _) = collection;

                for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
                {
                    if (buffer[i].reference.ToEGID(entitiesDB, out _) == true)
                    {
                        Assert.Fail();
                    }
                }
            }

            public void Ready() { }

            public EntitiesDB entitiesDB { get; set; }
        }

        public class RemoveEntityEngineLegacy: IReactOnRemove<EntityReferenceComponent>, IQueryingEntitiesEngine
        {
            public void Ready() { }

            public EntitiesDB entitiesDB { get; set; }

            public void Remove(ref EntityReferenceComponent entityComponent, EGID egid)
            {
                if (entityComponent.reference.ToEGID(entitiesDB, out _) == true)
                {
                    Assert.Fail();
                }
            }
        }
    }
}