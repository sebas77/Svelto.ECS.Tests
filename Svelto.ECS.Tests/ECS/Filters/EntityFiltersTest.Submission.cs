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
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreateTransientFilter<TestEntityComponent>(
                                         (_transientFilter1, testFilterContext));
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
            Assert.AreEqual(Groups.GroupA.id, group.id);
            Assert.AreEqual(2, indices.count);

            // Group B
            Assert.AreEqual(true, iterator.MoveNext());

            (indices, group) = iterator.Current;
            Assert.AreEqual(Groups.GroupB.id, group.id);
            Assert.AreEqual(3, indices.count);

            _scheduler.SubmitEntities();

            iterator.Reset();
            Assert.AreEqual(false, iterator.MoveNext(), "Transient filters must be cleared after submission.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
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
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA2);

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Filter must not be removed before submission");

            var (components, ids, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA4, components[indices[2]].ID
                          , "Unexpected entity will get swapped, test is invalidated");
#endif
            Assert.AreEqual(EgidA4.entityID, ids[indices[2]]
              , "Unexpected entity will get swapped, test is invalidated");

            // Perform entity removal.
            _scheduler.SubmitEntities();

            iterator.Reset();
            iterator.MoveNext();

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            (components, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(2, indices[0], "Removed index must be reused for the last entity");
#if SLOW_SVELTO_SUBMISSION               
            Assert.AreEqual(EgidA4, components[indices[0]].ID, "Reused index must point to the previous last entity");
#endif            
            Assert.AreEqual(EgidA4.entityID, ids[indices[0]], "Reused index must point to the previous last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
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
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Filter count must not have changed");

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(0, indices[2], "Removed index must be reused for last entity.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA4, components[indices[2]].ID, "Reused index must be pointing to last entity");
#endif
            Assert.AreEqual(EgidA4.entityID, entities[indices[2]], "Reused index must be pointing to last entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_FilteredEntity_WithoutSwapBack()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
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
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
#endif
            Assert.AreEqual(EgidA1.entityID, entities[indices[0]], "Indices kept must not have changed");
            Assert.AreEqual(EgidA2.entityID, entities[indices[1]], "Indices kept must not have changed");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterRemoving_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmapA = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(Groups.GroupA);
            filter.Add(EgidA1, mmapA);
            filter.Add(EgidA2, mmapA);

            // Remove last entity in group that is not filtered.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
            _scheduler.SubmitEntities();

            // Check that filters haven't changed.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Filter count must decrease by one after remove");

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA1, components[indices[0]].ID, "Indices kept must not have changed");
            Assert.AreEqual(EgidA2, components[indices[1]].ID, "Indices kept must not have changed");
#endif            
            Assert.AreEqual(EgidA1.entityID, entities[indices[0]], "Indices kept must not have changed");
            Assert.AreEqual(EgidA2.entityID, entities[indices[1]], "Indices kept must not have changed");

        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB1, Groups.GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, entityIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group A count must increase after swap.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB1.entityID, Groups.GroupA), components[indices[2]].ID
                          , "Swapped entity must be added to the last index.");
#endif
            Assert.AreEqual(EgidA0.entityID, entityIDs[indices[0]], "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3.entityID, entityIDs[indices[1]], "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidB1.entityID, entityIDs[indices[2]]
              , "Swapped entity must be added to the last index.");
            // Check groups B.
            iterator.MoveNext();

            (components, entityIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(1, indices.count, "Group B count must decrease after swap.");
            Assert.AreEqual(1, indices[0], "Index of last entity must have updated.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidB4, components[indices[0]].ID //
                          , "Entity pointed by index must be the previuos last index.");
#endif
            Assert.AreEqual(EgidB4.entityID, entityIDs[indices[0]] //
              , "Entity pointed by index must be the previuos last index.");

        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithSwapBack()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was not filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB0, Groups.GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, entitiesIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group A count must not have changed after swap.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
#endif
            Assert.AreEqual(EgidA0.entityID, entitiesIDs[indices[0]], "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3.entityID, entitiesIDs[indices[1]], "Previous filtered entities must not have changed.");
            // Check groups B.
            iterator.MoveNext();

            (components, entitiesIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupB);
            (indices, _) = iterator.Current; //filters linked to the groupB
            
            
            // entities are built in this order
            // (EgidB0);
            // (EgidB1); //value in filter[0] = 1
            // (EgidB2);
            // (EgidB3);
            // (EgidB4); //value in filter[1] = 4
            //so after removing B0 I expect the order to shuffle like:
            // (EgidB4); //value in filter[1] = 0
            // (EgidB1); //value in filter[0] = 1
            // (EgidB2);
            // (EgidB3);
            
            
            Assert.AreEqual(2, indices.count, "Group B count must not have changed after swap.");
            Assert.AreEqual(1, indices[0], "Index of first entity must not have updated.");
            Assert.AreEqual(0, indices[1], "Index of last entity must have changed to the swapped index.");
#if SLOW_SVELTO_SUBMISSION
            Assert.AreEqual(EgidB4, components[indices[1]].ID
                          , "Changed index must be still pointing to the correct entity");
#endif                          
            Assert.AreEqual(EgidB4.entityID, entitiesIDs[indices[1]]
              , "Changed index must be still pointing to the correct entity");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_FilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidB4, Groups.GroupA);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, entityIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group A count must increase after swap.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not have changed.");
            Assert.AreEqual(new EGID(EgidB4.entityID, Groups.GroupA), components[indices[2]].ID
                          , "Swapped entity must be added to the last index.");
#endif                          
            Assert.AreEqual(EgidA0.entityID, entityIDs[indices[0]], "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidA3.entityID, entityIDs[indices[1]], "Previous filtered entities must not have changed.");
            Assert.AreEqual(EgidB4.entityID, entityIDs[indices[2]], "Swapped entity must be added to the last index.");

            // Check groups B.
            iterator.MoveNext();

            (components, entityIDs, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group B count must decrease after swap.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not have changed.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not have changed.");
#endif            
            Assert.AreEqual(EgidB2.entityID, entityIDs[indices[0]], "Group B filters must not have changed.");
            Assert.AreEqual(EgidB1.entityID, entityIDs[indices[1]], "Group B filters must not have changed.");
        }

        [Test]
        public void Test_PersistentFilter_UpdateAfterSwapping_NonFilteredEntity_WithoutSwapBack()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidB2, mmap);
            filter.Add(EgidB1, mmap);
            filter.Add(EgidB4, mmap);
            mmap.Dispose();
            // Swap an entity that was filtered and test changes.
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA4, Groups.GroupB);
            _scheduler.SubmitEntities();

            // Check groups A.
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);

            var (indices, _) = iterator.Current;
            Assert.AreEqual(2, indices.count, "Group A count must not change.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[0]].ID, "Previous filtered entities must not change.");
            Assert.AreEqual(EgidA3, components[indices[1]].ID, "Previous filtered entities must not change.");
#endif            
            Assert.AreEqual(EgidA0.entityID, entities[indices[0]], "Previous filtered entities must not change.");
            Assert.AreEqual(EgidA3.entityID, entities[indices[1]], "Previous filtered entities must not change.");

            // Check groups B.
            iterator.MoveNext();

            (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupB);

            (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Group B count must not change.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidB2, components[indices[0]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB1, components[indices[1]].ID, "Group B filters must not change.");
            Assert.AreEqual(EgidB4, components[indices[2]].ID, "Group B filters must not change.");
#endif            
            Assert.AreEqual(EgidB2.entityID, entities[indices[0]], "Group B filters must not change.");
            Assert.AreEqual(EgidB1.entityID, entities[indices[1]], "Group B filters must not change.");
            Assert.AreEqual(EgidB4.entityID, entities[indices[2]], "Group B filters must not change.");
        }

        [Test]
        public void Test_PersistentFilter_UpdatesAfterRemove_WithSwapBack_EnsureReverseMapIsKeptValid()
        {
            // Create filters.
            var mmap = _entitiesDB.QueryMappedEntities<TestEntityComponent>(_entitiesDB.FindGroups<TestEntityComponent>());
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            filter.Add(EgidA2, mmap);
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);
            // Add filters to other group just to make sure there is no interference.
            filter.Add(EgidB0, mmap);
            filter.Add(EgidB3, mmap);

            // Remove an entity that is not filtered and this should also cause a swap back.
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            // Add a new entity to take the place of the swapped back index.
            _factory.BuildEntity<EntityDescriptorWithComponents>(EgidA0);
            _scheduler.SubmitEntities();

            // Add new entity to filter.
            filter.Add(EgidA0, mmap);
            
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(4, indices.count, "Filter count must have changed by one");

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(4, indices[3], "Last index must point to the last entity added.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[3]].ID
                          , "Last entity index must point to the last entity added.");
#endif                          
            Assert.AreEqual(EgidA0.entityID, entities[indices[3]]
              , "Last entity index must point to the last entity added.");
        }
        
         [Test]
        public void TestPersistentWithIndices()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            filter.Add(EgidA0, 0);
            filter.Add(EgidA1, 1);
            filter.Add(EgidA2, 2);
            
            var iterator = filter.GetEnumerator();
            iterator.MoveNext();

            var (indices, _) = iterator.Current;
            Assert.AreEqual(3, indices.count, "Filter count must have changed by one");

            var (components, entities, _) = _entitiesDB.QueryEntities<TestEntityComponent>(Groups.GroupA);
            Assert.AreEqual(2, indices[2], "Last index must point to the last entity added.");
#if SLOW_SVELTO_SUBMISSION            
            Assert.AreEqual(EgidA0, components[indices[0]].ID
                          , "Last entity index must point to the last entity added.");
#endif                          
            Assert.AreEqual(EgidA0.entityID, entities[indices[0]]
              , "Last entity index must point to the last entity added.");
        }
        
        [Test]
        public void TestAddingEntityInFilterTwice()
        {
            // Create filters.
            var filter = _entitiesDB.GetFilters()
                   .GetOrCreatePersistentFilter<TestEntityComponent>(
                        (_persistentFilter1, testFilterContext));
            filter.Add(EgidA0, 0);
            filter.Add(EgidA0, 0);
        }

        [Test]
        public void Test_EntitySwapsWithFilterRemoval()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);

            // Adding 4 entities to filter
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA2, mmap); // Will not be swapped
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);

            // Swapping all but one entity to other group and removing from filter at the same time
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA0, Groups.GroupB);
            filter.Remove(EgidA0);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA1, Groups.GroupB);
            filter.Remove(EgidA1);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA3, Groups.GroupB);
            filter.Remove(EgidA3);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA4, Groups.GroupB);
            filter.Remove(EgidA4);

            _scheduler.SubmitEntities();

            var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

            Assert.AreEqual(aGroupFilter.indices.count, 1, "Filter should contain 1 entity");

            var bufferIndex = mmap.GetIndex(EgidA2);
            mmap.Dispose();
            var filterIndex =  aGroupFilter[EgidA2.entityID];
            Assert.AreEqual(bufferIndex, filterIndex, "Filter index does not match expected buffer index");
        }

        [Test]
        public void Test_EntitySwapsWithFilterRemoval_InvertedOrder()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);

            // Adding 4 entities to filter
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA2, mmap); // Will not be swapped
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);

            // Swapping all but one entity to other group and removing from filter at the same time
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA4, Groups.GroupB);
            filter.Remove(EgidA4);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA3, Groups.GroupB);
            filter.Remove(EgidA3);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA1, Groups.GroupB);
            filter.Remove(EgidA1);
            _functions.SwapEntityGroup<EntityDescriptorWithComponents>(EgidA0, Groups.GroupB);
            filter.Remove(EgidA0);

            _scheduler.SubmitEntities();

            var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

            Assert.AreEqual(aGroupFilter.indices.count, 1, "Filter should contain 1 entity");

            var bufferIndex = mmap.GetIndex(EgidA2);
            mmap.Dispose();
            var filterIndex = aGroupFilter.indices[0];
            Assert.AreEqual(bufferIndex, filterIndex, "Filter index does not match expected buffer index");
        }

        [Test]
        public void Test_EntityRemovalWithFilterRemoval()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            
            using (var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                       _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent))
            {

                // Adding 4 entities to filter
                filter.Add(EgidA0, mmap);
                filter.Add(EgidA1, mmap);
                filter.Add(EgidA2, mmap); // Will not be removed
                filter.Add(EgidA3, mmap);
                filter.Add(EgidA4, mmap);

                // Swapping all but one entity to other group and removing from filter at the same time
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA1);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA3);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
                
                //the entities are removed from the filter before the submission, this means
                //that during the submission these entities will not be found in the filter
                //we need still to be sure that the index of EgidA2 will be updated!
                filter.Remove(EgidA0);
                filter.Remove(EgidA1);
                filter.Remove(EgidA3);
                filter.Remove(EgidA4);
                
                _scheduler.SubmitEntities();

                var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

                Assert.AreEqual(aGroupFilter.indices.count, 1, "Filter should contain 1 entity");

                //multi map are still valid even after a submission, this should be a separate unit test actually
                var entityIndex = mmap.GetIndex(EgidA2);
                var filterIndex = aGroupFilter[EgidA2.entityID];
                
                Assert.AreEqual(entityIndex, filterIndex, "Filter index does not match expected buffer index");
            }
        }
        
         [Test]
        public void Test_RemoveEntityThatIsNotInTheFilterButWillAffectOtherEntitiesInTheFilter()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            
            using (var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                       _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent))
            {
                filter.Add(EgidA0, mmap);
                filter.Add(EgidA2, mmap);
                filter.Add(EgidA3, mmap);
                filter.Add(EgidA4, mmap);

                // EgidA1 is not in the filter, but will affect the index of EgidA4 that is in the filter instead
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA1);
                
                _scheduler.SubmitEntities();

                var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

                //multi map are still valid even after a submission, this should be a separate unit test actually
                var entityIndex = mmap.GetIndex(EgidA2);
                var filterIndex = aGroupFilter[EgidA2.entityID];
                
                Assert.AreEqual(entityIndex, filterIndex, "Filter index does not match expected buffer index");
            }
        }
        
        [Test]
        public void Test_RemoveEntityThatIsNotInTheFilterButWillAffectOtherEntitiesInTheFilterCase2()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            
            using (var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                       _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent))
            {
                filter.Add(EgidA0, mmap);
                filter.Add(EgidA2, mmap);
                filter.Add(EgidA3, mmap);

                // EgidA1 is not in the filter, but will affect the index of EgidA4
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA1);
                // EgidA4 is not in the filter but it will affect the index of EgidA3 that is in the filter!
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
                
                _scheduler.SubmitEntities();

                var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

                //multi map are still valid even after a submission, this should be a separate unit test actually
                var entityIndex = mmap.GetIndex(EgidA2);
                var filterIndex = aGroupFilter[EgidA2.entityID];
                
                Assert.AreEqual(entityIndex, filterIndex, "Filter index does not match expected buffer index");
            }
        }
        
        [Test]
        public void Test_EntityRemovalWithFilterRemovalCase2()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            
            using (var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                       _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent))
            {

                // Adding 4 entities to filter
                filter.Add(EgidA0, mmap); // Will not be removed
                filter.Add(EgidA1, mmap);
                filter.Add(EgidA2, mmap); // Will not be removed
                filter.Add(EgidA3, mmap); 
                filter.Add(EgidA4, mmap); // Will not be removed

                // Swapping all but one entity to other group and removing from filter at the same time
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA1);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA3);
                _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
                
                //the entities are removed from the filter before the submission, this means
                //that during the submission these entities will not be found in the filter
                //we need still to be sure that the index of EgidA2 will be updated!
                filter.Remove(EgidA1);
                filter.Remove(EgidA3);

                _scheduler.SubmitEntities();

                var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

                Assert.AreEqual(aGroupFilter.indices.count, 1, "Filter should contain 1 entity");

                var bufferIndex = mmap.GetIndex(EgidA2);
                var filterIndex = aGroupFilter[EgidA2.entityID];
                
                Assert.AreEqual(bufferIndex, filterIndex, "Filter index does not match expected buffer index");
            }
        }

        [Test]
        public void Test_EntityRemovalWithFilterRemoval_InvertedOrder()
        {
            var filter = _entitiesDB.GetFilters()
                                    .GetOrCreatePersistentFilter<TestEntityComponent>(
                                         (_persistentFilter1, testFilterContext));
            var mmap = _entitiesDB.QueryNativeMappedEntities<TestEntityComponent>(
                _entitiesDB.FindGroups<TestEntityComponent>(), Allocator.Persistent);

            // Adding 4 entities to filter
            filter.Add(EgidA0, mmap);
            filter.Add(EgidA1, mmap);
            filter.Add(EgidA2, mmap); // Will not be swapped
            filter.Add(EgidA3, mmap);
            filter.Add(EgidA4, mmap);

            // Swapping all but one entity to other group and removing from filter at the same time - in reverse order of buffer
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA4);
            filter.Remove(EgidA4);
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA3);
            filter.Remove(EgidA3);
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA1);
            filter.Remove(EgidA1);
            _functions.RemoveEntity<EntityDescriptorWithComponents>(EgidA0);
            filter.Remove(EgidA0);

            _scheduler.SubmitEntities();

            var aGroupFilter = filter.GetGroupFilter(Groups.GroupA);

            Assert.AreEqual(aGroupFilter.indices.count, 1, "Filter should contain 1 entity");

            var bufferIndex = mmap.GetIndex(EgidA2);
            mmap.Dispose();
            var filterIndex = aGroupFilter.indices[0];
            Assert.AreEqual(bufferIndex, filterIndex, "Filter index does not match expected buffer index");
        }
    }
}