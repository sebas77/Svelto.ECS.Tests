using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS.Filters
{
    public partial class EntityFiltersTest
    {
        [Test]
        public void Test_TransientFilter_UpdateAfterSubmission()
        {
            var filter = _entitiesDB.GetTransientFilter(_transientFilter1);
            filter.AddEntity(EgidA0);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidB1);
            filter.AddEntity(EgidB2);
            filter.AddEntity(EgidB4);

            var iterator = filter.iterator.GetEnumerator();

            // Group A
            Assert.AreEqual(true, iterator.MoveNext());

            var (indices, group) = iterator.Current;
            Assert.AreEqual(GroupA.id, group.id);
            Assert.AreEqual(2, indices.Count());

            // Group B
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, group) = iterator.Current;
            Assert.AreEqual(GroupB.id, group.id);
            Assert.AreEqual(3, indices.Count());

            _scheduler.SubmitEntities();

            iterator.Reset();
            Assert.AreEqual(false, iterator.MoveNext(), "Transient filters must be cleared after submission.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA2);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidA4);
            // Add filters to other group just to make sure there is no interference.
            filter.AddEntity(EgidB0);
            filter.AddEntity(EgidB3);

            // Remove first entity to make sure the swap is being handled after the submit.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA2);

            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.Count(), "Filter must not be removed before submission");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA4, components[indices[2]].ID, "Unexpected entity will get swapped, test is invalidated");

            // Perform entity removal.
            _scheduler.SubmitEntities();

            iterator.Reset();
            iterator.MoveNext();

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Filter count must decrease by one after remove");

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(2, indices[0], "Removed index must be reused for the last entity");
            Assert.AreEqual(EgidA4, components[indices[0]].ID, "Reused index must point to the previous last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA2);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidA4);
            // Add filters to other group just to make sure there is no interference.
            filter.AddEntity(EgidB0);
            filter.AddEntity(EgidB3);

            // Remove an entity that is not filtered and this should also cause a swap back.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.Count(), "Filter count must not have changed");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(0, indices[2], "Removed index must be reused for last entity.");
            Assert.AreEqual(EgidA4, components[indices[2]].ID, "Reused index must be pointing to last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithoutSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA2);
            filter.AddEntity(EgidA4);
            // Add filters to other group just to make sure there is no interference.
            filter.AddEntity(EgidB0);
            filter.AddEntity(EgidB3);

            // Remove last entity which won't cause a swap.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Filter count must decrease by one after remove");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA1);
            filter.AddEntity(EgidA2);

            // Remove last entity in group that is not filtered.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            // Check that filters haven't changed.
            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Filter count must decrease by one after remove");

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA0);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidB1);
            filter.AddEntity(EgidB4);

            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB1, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.Count(), "Group A count must increase after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB1.entityID, GroupA), components[indices[2]].ID, "Swapped entity must be added to the last index.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(1, indices.Count(), "Group B count must decrease after swap.");
            Assert.AreEqual(1, indices[0], "Index of last entity must have updated.");
            Assert.AreEqual(EgidB4, components[indices[0]].ID, "Entity pointed by index must be the previuos last index.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA0);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidB1);
            filter.AddEntity(EgidB4);

            // Swap an entity that was not filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB0, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Group A count must not have changed after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Group B count must not have changed after swap.");
            Assert.AreEqual(1, indices[0], "Index of first entity must not have updated.");
            Assert.AreEqual(0, indices[1], "Index of last entity must have changed to the swapped index.");
            Assert.AreEqual(EgidB4, components[indices[1]].ID, "Changed index must be still pointing to the correct entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA0);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidB2);
            filter.AddEntity(EgidB1);
            filter.AddEntity(EgidB4);

            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB4, GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.Count(), "Group A count must increase after swap.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB4.entityID, GroupA), components[indices[2]].ID, "Swapped entity must be added to the last index.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Group B count must decrease after swap.");
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not have changed.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not have changed.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetPersistentFilter(_persistentFilter1);
            filter.AddEntity(EgidA0);
            filter.AddEntity(EgidA3);
            filter.AddEntity(EgidB2);
            filter.AddEntity(EgidB1);
            filter.AddEntity(EgidB4);

            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA4, GroupB);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.iterator.GetEnumerator();
            iterator.MoveNext();

            var (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.Count(), "Group A count must not change.");
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not change.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not change.");

            // Check groups B.
            iterator.MoveNext();

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.Count(), "Group B count must not change.");
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB4, components[indices[2]].ID, "Group B filters must not change.");
        }
    }
}