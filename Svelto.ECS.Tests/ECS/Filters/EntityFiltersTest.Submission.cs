using NUnit.Framework;
using Svelto.Common;
using Svelto.ECS.Native;

namespace Svelto.ECS.Tests.ECS.Filters
{
    public partial class EntityFiltersTest
    {
        [Test]
        public void Test_TransientFilter_UpdateAfterSubmission()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreateTransientFilter<TestEntityComponent>(_transientFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidB4, mmap);

            var iterator = filter.GetEnumerator();
            mmap.Dispose();
            

            // Group A
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(GroupA.id, group.id);
            Assert.AreEqual(2, indices.count);

            // Group B
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, group) = iterator.Current;
            Assert.AreEqual(GroupB.id, group.id);
            Assert.AreEqual(3, indices.count);

            _scheduler.SubmitEntities();

            iterator.Reset();
            Assert.AreEqual(false, iterator.MoveNext(), "Transient filters must be cleared after submission.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA2, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            // Add filters to other group just to make sure there is no interference.
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB3, mmap);
            mmap.Dispose();
            // Remove first entity to make sure the swap is being handled after the submit.
            _functions.Remove<EntityDescriptorWithComponents>(EgidA2);

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Filter must not be removed before submission");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA4, components[indices[2]].ID, "Unexpected entity will get swapped, test is invalidated");

            // Perform entity removal.
            _scheduler.SubmitEntities();

            iterator.Reset();
            iterator.MoveNext();

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(2, indices[0], "Removed index must be reused for the last entity");
            Assert.AreEqual(EgidA4, components[indices[0]].ID, "Reused index must point to the previous last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA2, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            // Add filters to other group just to make sure there is no interference.
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB3, mmap);
            mmap.Dispose();
            // Remove an entity that is not filtered and this should also cause a swap back.
            _functions.Remove<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Filter count must not have changed");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(0, indices[2], "Removed index must be reused for last entity.");
            Assert.AreEqual(EgidA4, components[indices[2]].ID, "Reused index must be pointing to last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithoutSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA2, mmap);
            filter.Add(EgidA4, mmap);
            // Add filters to other group just to make sure there is no interference.
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB3, mmap);
            mmap.Dispose();
            // Remove last entity which won't cause a swap.
            _functions.Remove<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmapA  = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(GroupA);
            filter.Add(EgidA1, mmapA);
            filter.Add(EgidA2, mmapA);

            // Remove last entity in group that is not filtered.
            _functions.Remove<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            // Check that filters haven't changed.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB1, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group A count must increase after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB1.entityID, GroupA), components[indices[2]].ID, "Swapped entity must be added to the last index.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(1, indices.count, "Group B count must decrease after swap.");
            Assert.AreEqual(1, indices[0], "Index of last entity must have updated.");
            Assert.AreEqual(EgidB4, components[indices[0]].ID, "Entity pointed by index must be the previuos last index.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was not filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB0, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group A count must not have changed after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group B count must not have changed after swap.");
            Assert.AreEqual(1, indices[0], "Index of first entity must not have updated.");
            Assert.AreEqual(0, indices[1], "Index of last entity must have changed to the swapped index.");
            Assert.AreEqual(EgidB4, components[indices[1]].ID, "Changed index must be still pointing to the correct entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB4, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group A count must increase after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB4.entityID, GroupA), components[indices[2]].ID, "Swapped entity must be added to the last index.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group B count must decrease after swap.");
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not have changed.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not have changed.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA4, GroupB);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group A count must not change.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not change.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not change.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group B count must not change.");
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB4, components[indices[2]].ID, "Group B filters must not change.");
        }

        [Test]
        public void Test_PersistentFilter_UpdatesAfterRemove_WithSwapBack_EnsureReverseMapIsKeptValid()
        {
            // Create filters.
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            var filter = _entitiesDB.GetFilters().GetOrCreatePersistentFilter<TestEntityComponent>(_persistentFilter1);
            filter.Add(EgidA2, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            // Add filters to other group just to make sure there is no interference.
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB3, mmap);
            
            // Remove an entity that is not filtered and this should also cause a swap back.
            _functions.Remove<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            // Add a new entity to take the place of the swapped back index.
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            // Add new entity to filter.
            filter.Add(EgidA0, mmap);
            mmap.Dispose();
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(4, indices.count, "Filter count must have changed by one");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(4, indices[3], "Last index must point to the last entity added.");
            Assert.AreEqual(EgidA0, components[indices[3]].ID, "Last entity index must point to the last entity added.");
        }
    }
}