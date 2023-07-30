using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS;

public partial class DisableEnableEntitiesFilterTests
{
    class SyncFiltersEngine: IQueryingEntitiesEngine,
            IReactOnAddEx<TestEntityComponent>,
            IReactOnSwapEx<TestEntityComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Add((uint start, uint end) rangeOfEntities,
            in EntityCollection<TestEntityComponent> entities,
            ExclusiveGroupStruct groupID)
        {
            //if we add entity from disabled group - do not add to filters
            if (!groupID.IsEnabled())
            {
                return;
            }

            var (_, entityIDs, _) = entities;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                uint entityId = entityIDs[i];
                var filters = entitiesDB.GetFilters();
                ref var filter = ref filters.GetOrCreatePersistentFilter<TestEntityComponent>(TestFilters.TestID);
                filter.Add(new(entityId, groupID), i);
            }
        }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var willBeDisabled = toGroup.Equals(TestGroups.Disabled);
            var willBeEnabled = fromGroup.Equals(TestGroups.Disabled);

            var (buffer, entityIDs, count) = entities;

            var filters = entitiesDB.GetFilters();
            ref var filter = ref filters.GetOrCreatePersistentFilter<TestEntityComponent>(TestFilters.TestID);

            var map = entitiesDB.QueryMappedEntities<TestEntityComponent>(toGroup);

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                //if we enable entity - add to filter
                var entityID = entityIDs[i];
                var entityId = new EGID(entityID, toGroup);
                if (willBeEnabled)
                {
                    var index = map.GetIndex(entityID);
                    Assert.That(index, Is.EqualTo(i));
                    filter.Add(entityId, i);
                }

                //if we disable entity - remove from filter
                if (willBeDisabled)
                {
                    filter.Remove(entityId);
                }
            }
        }

        public void Ready() { }
    }
    
    class SimplerEngine: IQueryingEntitiesEngine,
            IReactOnSwapEx<TestEntityComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> entities,
            ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
        {
            var willBeEnabled = fromGroup.Equals(TestGroups.Disabled);

            var (buffer, entityIDs, count) = entities;

            var map = entitiesDB.QueryMappedEntities<TestEntityComponent>(toGroup);

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                //if we enable entity - add to filter
                var entityID = entityIDs[i];
                var index = map.GetIndex(entityID);
                Assert.That(index, Is.EqualTo(i));
            }
        }

        public void Ready() { }
    }
}