using NUnit.Framework;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS.Filters
{
    [TestFixture]
    public partial class EntityFiltersTest
    {
        [SetUp]
        public void SetUp()
        {
            _scheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            _factory = _enginesRoot.GenerateEntityFactory();
            _functions = _enginesRoot.GenerateEntityFunctions();
            _entitiesDB = ((IUnitTestingInterface)_enginesRoot).entitiesForTesting;

            // Create entities.
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA0);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA1);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA2);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA3);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA4);

            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidB0);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidB1);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidB2);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidB3);
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidB4);

            _transientFilter1 = _enginesRoot.CreateTransientFilter<TestEntityComponent>();
            _transientFilter2 = _enginesRoot.CreateTransientFilter<TestEntityComponent>();
            _persistentFilter1 = _enginesRoot.CreatePersistentFilter<TestEntityComponent>();
            _persistentFilter2 = _enginesRoot.CreatePersistentFilter<TestEntityComponent>();

            _scheduler.SubmitEntities();
        }

        [TearDown]
        public void TearDown()
        {
            _enginesRoot.Dispose();
            _scheduler.Dispose();
        }

        [Test]
        public void Test_AddingFilter_SingleEntity()
        {
            var filter = _entitiesDB.GetFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);

            var iterator = filter.iterator.GetEnumerator();

            // Iterate a single group and check filter values.
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(1, indices.Count());
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(GroupA.id, group.id);

            // No more groups to iterate.
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_AddingFilter_ManyEntities()
        {
            var filter = _entitiesDB.GetFilter(_persistentFilter1);
            filter.AddEntity(EgidB4);
            filter.AddEntity(EgidB0);
            filter.AddEntity(EgidB2);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA3);

            var iterator = filter.iterator.GetEnumerator();

            // Check group B filters.
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(3, indices.Count());
            Assert.AreEqual(4, indices[0]);
            Assert.AreEqual(0, indices[1]);
            Assert.AreEqual(2, indices[2]);
            Assert.AreEqual(GroupB.id, group.id);

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, group) = iterator.Current;
            Assert.AreEqual(2, indices.Count());
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(3, indices[1]);
            Assert.AreEqual(GroupA.id, group.id);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_RemovingFilter_SingleEntity()
        {
            var filter = _entitiesDB.GetFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidA4);

            // Remove entity.
            filter.RemoveEntity(EgidA1);

            var iterator = filter.iterator.GetEnumerator();

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count());
            Assert.AreEqual(4, indices[0]); // There was a swap back.
            Assert.AreEqual(3, indices[1]);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_RemovingFilter_ManyEntities()
        {
            var filter = _entitiesDB.GetFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidA4);
            filter.AddEntity(EgidB4);
            filter.AddEntity(EgidB3);
            filter.AddEntity(EgidB2);

            // Remove entity.
            filter.RemoveEntity(EgidB3);
            filter.RemoveEntity(EgidB2);
            filter.RemoveEntity(EgidA3);

            var iterator = filter.iterator.GetEnumerator();

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count());
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(4, indices[1]);

            // Check group B filters
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, _) = iterator.Current;
            Assert.AreEqual(1, indices.Count());
            Assert.AreEqual(4, indices[0]);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_ClearingFilter()
        {
            var filter = _entitiesDB.GetFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidA4);
            filter.AddEntity(EgidB4);
            filter.AddEntity(EgidB3);
            filter.AddEntity(EgidB2);

            // Remove entity.
            filter.Clear();

            var iterator = filter.iterator.GetEnumerator();

            // Nothing to iterate.
            Assert.AreEqual(false, iterator.MoveNext());
        }

        static ExclusiveGroup GroupA = new ExclusiveGroup();
        static ExclusiveGroup GroupB = new ExclusiveGroup();

        SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _factory;
        IEntityFunctions                  _functions;
        EntitiesDB                        _entitiesDB;

        EntityFilterID _persistentFilter1;
        EntityFilterID _persistentFilter2;
        EntityFilterID _transientFilter1;
        EntityFilterID _transientFilter2;

        static EGID EgidA0 = new EGID(45872, GroupA);
        static EGID EgidA1 = new EGID(28577, GroupA);
        static EGID EgidA2 = new EGID(95323, GroupA);
        static EGID EgidA3 = new EGID(68465, GroupA);
        static EGID EgidA4 = new EGID(12335, GroupA);

        static EGID EgidB0 = new EGID(45873, GroupB);
        static EGID EgidB1 = new EGID(28578, GroupB);
        static EGID EgidB2 = new EGID(95324, GroupB);
        static EGID EgidB3 = new EGID(68466, GroupB);
        static EGID EgidB4 = new EGID(12336, GroupB);
    }
}