using NUnit.Framework;
using Svelto.Common;
using Svelto.ECS.Native;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests
{ }

namespace Svelto.ECS.Tests.ECS.Filters
{
    [TestFixture]
    public partial class EntityFiltersTest
    {
        [SetUp]
        public void SetUp()
        {
            _scheduler   = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            _factory     = _enginesRoot.GenerateEntityFactory();
            _functions   = _enginesRoot.GenerateEntityFunctions();
            _entitiesDB  = ((IUnitTestingInterface)_enginesRoot).entitiesForTesting;

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

            _scheduler.SubmitEntities();
        }

        [TearDown]
        public void TearDown()
        {
            _enginesRoot.Dispose();
            _scheduler.Dispose();
        }

        void Test_TransientFilter_Is_Not_Found_After_Submission()
        {
        }

        readonly FilterContextID testFilterContext = EntitiesDB.SveltoFilters.GetNewContextID();

        [Test]
        public void Test_AddingFilter_SingleEntity()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>((_persistentFilter1, testFilterContext));
            filter.Add(EgidA1, _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(Groups.GroupA));

            var iterator = filter.GetEnumerator();

            // Iterate a single group and check filter values.
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(1, indices.count);
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(Groups.GroupA.id, group.id);

            // No more groups to iterate.
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_AddingFilter_ManyEntities()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>((_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            
            filter.Add(EgidB4, mmap);
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA3, mmap);
            
            mmap.Dispose();
            
            var iterator = filter.GetEnumerator();

            // Check group B filters.
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(3, indices.count);
            Assert.AreEqual(4, indices[0]);
            Assert.AreEqual(0, indices[1]);
            Assert.AreEqual(2, indices[2]);
            Assert.AreEqual(Groups.GroupB.id, group.id);

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, group) = iterator.Current;
            Assert.AreEqual(2, indices.count);
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(3, indices[1]);
            Assert.AreEqual(Groups.GroupA.id, group.id);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_RemovingFilter_SingleEntity()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>((_persistentFilter1, testFilterContext));
            var mmapA  = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(Groups.GroupA);
            filter.Add(EgidA1, mmapA);
            filter.Add(EgidA3, mmapA);
            filter.Add(EgidA4, mmapA);

            // Remove entity.
            filter.Remove(EgidA1);

            var iterator = filter.GetEnumerator();

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count);
            Assert.AreEqual(4, indices[0]); // There was a swap back.
            Assert.AreEqual(3, indices[1]);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_RemovingFilter_ManyEntities()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>((_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            filter.Add(EgidB4, mmap);
            filter.Add(EgidB3, mmap);
            filter.Add(EgidB2, mmap);
            
            mmap.Dispose();

            // Remove entity.
            filter.Remove(EgidB3);
            filter.Remove(EgidB2);
            filter.Remove(EgidA3);

            var iterator = filter.GetEnumerator();

            // Check group A filters
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count);
            Assert.AreEqual(1, indices[0]);
            Assert.AreEqual(4, indices[1]);

            // Check group B filters
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, _) = iterator.Current;
            Assert.AreEqual(1, indices.count);
            Assert.AreEqual(4, indices[0]);

            // No more groups to iterate
            Assert.AreEqual(false, iterator.MoveNext());
        }

        [Test]
        public void Test_ClearingFilter()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>((_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            filter.Add(EgidB4, mmap);
            filter.Add(EgidB3, mmap);
            filter.Add(EgidB2, mmap);
            
            mmap.Dispose();

            // Remove entity.
            filter.Clear();

            var iterator = filter.GetEnumerator();

            // Nothing to iterate.
            Assert.AreEqual(false, iterator.MoveNext());
        }

        SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _factory;
        IEntityFunctions                  _functions;
        EntitiesDB                        _entitiesDB;

        int _persistentFilter1 = 0;
        int _transientFilter1  = 0;
        
        static readonly EGID EgidA0 = new EGID(45872, Groups.GroupA);
        static readonly EGID EgidA1 = new EGID(28577, Groups.GroupA);
        static readonly EGID EgidA2 = new EGID(95323, Groups.GroupA);
        static readonly EGID EgidA3 = new EGID(68465, Groups.GroupA);
        static readonly EGID EgidA4 = new EGID(12335, Groups.GroupA);

        static readonly EGID EgidB0 = new EGID(45873, Groups.GroupB);
        static readonly EGID EgidB1 = new EGID(28578, Groups.GroupB);
        static readonly EGID EgidB2 = new EGID(95324, Groups.GroupB);
        static readonly EGID EgidB3 = new EGID(68466, Groups.GroupB);
        static readonly EGID EgidB4 = new EGID(12336, Groups.GroupB);
    }
}