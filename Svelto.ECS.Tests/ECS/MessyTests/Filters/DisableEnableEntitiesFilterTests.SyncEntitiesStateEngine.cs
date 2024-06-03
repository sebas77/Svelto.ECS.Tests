namespace Svelto.ECS.Tests.ECS;

public partial class DisableEnableEntitiesFilterTests
{
    class SyncEntitiesStateEngine : IStepEngine, IQueryingEntitiesEngine
    {
        readonly bool _enableFirst;

        readonly IEntityFunctions _entityFunctions;

        internal SyncEntitiesStateEngine(IEntityFunctions entityFunctions, bool enableFirst)
        {
            _entityFunctions = entityFunctions;
            _enableFirst     = enableFirst;
        }

        public string name => nameof(SyncEntitiesStateEngine);

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            //base of the test setup - we enable first or disable first
            if (_enableFirst)
            {
                EnableEntities();
                DisableEntities();
            }
            else
            {
                DisableEntities();
                EnableEntities();
            }
        }

        void DisableEntities()
        {
            //Query only enabled entities
            var query = entitiesDB.QueryEntities<TestEntityComponent>(TestGroups.TestGroupTag.Groups);

            foreach (var ((entities, entityIDs, count), group) in query)
            {
                for (var i = 0; i < count; i++)
                {
                    ref var entity = ref entities[i];

                    if (!entity.Enabled)
                    {
                        //Disable enabled entities
                        _entityFunctions.SwapEntityGroup<TestEntityDescriptor>(new(entityIDs[i], group), TestGroups.Disabled);
                    }
                }
            }
        }

        void EnableEntities()
        {
            //Query only disabled entities
            var (entities, entityIDs, count) = entitiesDB.QueryEntities<TestEntityComponent>(TestGroups.Disabled);

            for (var i = 0; i < count; i++)
            {
                ref var entity = ref entities[i];

                if (entity.Enabled)
                {
                    //Enable disabled entities
                    _entityFunctions.SwapEntityGroup<TestEntityDescriptor>(new(entityIDs[i], TestGroups.Disabled), TestGroups.TestGroupTag.BuildGroup);
                }
            }
        }
    }
}