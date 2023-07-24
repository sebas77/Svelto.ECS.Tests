using NUnit.Framework;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS;

[TestFixture]
public class DisableEnableEntitiesFilterTests
{
    private EnginesRoot                       _enginesRoot;
    private IUnitTestingInterface             _entitiesDB;
    private IEntityFactory                    _factory;
    private IEntityFunctions                  _functions;
    private SimpleEntitiesSubmissionScheduler _scheduler;

    [SetUp]
    public void SetUp()
    {
        _scheduler   = new();
        _enginesRoot = new(_scheduler);
        _factory     = _enginesRoot.GenerateEntityFactory();
        _functions   = _enginesRoot.GenerateEntityFunctions();
        _entitiesDB  = _enginesRoot;
    }

    [Test]
    public void TestEntitiesGroupChange_EnableFirst()
    {
        //engines
        var changeValueEngine = new ChangeActiveEntityValueEngine();
        var syncEngine        = new SyncEntitiesStateEngine(_functions, true); //-> we setup filters swap engine to run enable first
        _enginesRoot.AddEngine(changeValueEngine);
        _enginesRoot.AddEngine(syncEngine);
        _enginesRoot.AddEngine(new SyncFiltersEngine());

        RunTestLogic(syncEngine, changeValueEngine);
    }

    [Test]
    public void TestEntitiesGroupChange_DisableFirst()
    {
        //engines
        var changeValueEngine = new ChangeActiveEntityValueEngine();
        var syncEngine        = new SyncEntitiesStateEngine(_functions, false); //-> we setup filters swap engine to run disable first
        _enginesRoot.AddEngine(changeValueEngine);
        _enginesRoot.AddEngine(syncEngine);
        _enginesRoot.AddEngine(new SyncFiltersEngine());

        RunTestLogic(syncEngine, changeValueEngine);
    }

    private void RunTestLogic(SyncEntitiesStateEngine syncEngine, ChangeActiveEntityValueEngine changeValueEngine)
    {
        //We build two entities
        var initializer0 = _factory.BuildEntity<TestEntityDescriptor>(0, TestGroups.TestGroupTag.BuildGroup);
        var initializer1 = _factory.BuildEntity<TestEntityDescriptor>(1, TestGroups.TestGroupTag.BuildGroup);

        //Both entities are enabled, first one has value 0, second one has value 2
        initializer0.Init(new TestEntityComponent { Enabled = true, SomeValue = 0 });
        initializer1.Init(new TestEntityComponent { Enabled = true, SomeValue = 2 });

        //Store references for future query
        var entityRef0 = initializer0.reference;
        var entityRef1 = initializer1.reference;

        //Run some frames after entities creation
        //We always run 2 frames to ensure that full cycle of group swap and engines run happens before assertions
        RunSingleFrame(syncEngine, changeValueEngine);
        RunSingleFrame(syncEngine, changeValueEngine);
        CheckAssertions(entityRef0, entityRef1);

        //Disable flag in entity 1
        ChangeEntityEnabledFlag(entityRef1, false);
        RunSingleFrame(syncEngine, changeValueEngine);
        RunSingleFrame(syncEngine, changeValueEngine);
        CheckAssertions(entityRef0, entityRef1);

        //Swap entity enable
        ChangeEntityEnabledFlag(entityRef0, false);
        ChangeEntityEnabledFlag(entityRef1, true);
        RunSingleFrame(syncEngine, changeValueEngine);
        RunSingleFrame(syncEngine, changeValueEngine);
        CheckAssertions(entityRef0, entityRef1);
    }

    private void CheckAssertions(EntityReference entityRef0, EntityReference entityRef1)
    {
        //query entities data from previously stored references
        var     egid0   = entityRef0.ToEGID(_entitiesDB.entitiesForTesting);
        var     egid1   = entityRef1.ToEGID(_entitiesDB.entitiesForTesting);
        ref var entity0 = ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(egid0);
        ref var entity1 = ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(egid1);

        var filter = _entitiesDB.entitiesForTesting.GetFilters()
            .GetPersistentFilter<TestEntityComponent>(TestFilters.TestID);

        foreach (var (indices, group) in filter)
        {
            var count = indices.count;

            var (buffer, entityIDs, _) = _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(group);

            var assertionsCount = 0;

            for (var i = 0; i < count; i++)
            {
                var filterEntity     = buffer[indices[i]];
                var filterEntityEgid = new EGID(entityIDs[indices[i]], group);

                //we compare egid from filters with ones from references.
                //if we find the same entity - compare values
                if (filterEntityEgid.Equals(egid0))
                {
                    Assert.AreEqual(entity0.SomeValue, filterEntity.SomeValue);
                    assertionsCount++;
                }

                if (filterEntityEgid.Equals(egid1))
                {
                    Assert.AreEqual(entity1.SomeValue, filterEntity.SomeValue);
                    assertionsCount++;
                }
            }

            //we count assertions because there should be exactly one assertion for every entity in filter
            Assert.AreEqual(count, assertionsCount);
        }
    }

    private void ChangeEntityEnabledFlag(EntityReference entityRef, bool enabled)
    {
        //query entity with given ref and swap enabled flag
        ref var entity =
            ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(
                entityRef.ToEGID(_entitiesDB.entitiesForTesting));
        entity.Enabled = enabled;
    }

    private void RunSingleFrame(IStepEngine syncEngine, IStepEngine changeValueEngine)
    {
        //single frame consists always from three elements:
        syncEngine.Step(); //-> run filter add/remove sync engine
        changeValueEngine.Step(); //-> change values on enabled entities
        _scheduler.SubmitEntities(); //-> submit
    }

    private struct TestEntityComponent : IEntityComponent
    {
        internal bool Enabled;
        internal int  SomeValue;
    }

    private static class TestGroups
    {
        internal static readonly ExclusiveGroupStruct Disabled = new ExclusiveGroup(ExclusiveGroupBitmask.DISABLED_BIT);

        internal class TestGroupTag : GroupTag<TestGroupTag> { }
    }

    private static class TestFilters
    {
        private static readonly  FilterContextID  TestContext = EntitiesDB.SveltoFilters.GetNewContextID();
        internal static readonly CombinedFilterID TestID      = new(0, TestContext);
    }

    private class TestEntityDescriptor : GenericEntityDescriptor<TestEntityComponent> { }

    private class ChangeActiveEntityValueEngine : IStepEngine, IQueryingEntitiesEngine
    {
        public string name => nameof(ChangeActiveEntityValueEngine);

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            var query = entitiesDB.QueryEntities<TestEntityComponent>(TestGroups.TestGroupTag.Groups);

            foreach (var ((buffer, count), _) in query)
            {
                for (var i = 0; i < count; i++)
                {
                    //This engine queries all enabled entities and increases their value by 1
                    ref var entity = ref buffer[i];
                    entity.SomeValue++;
                }
            }
        }
    }

    private class SyncEntitiesStateEngine : IStepEngine, IQueryingEntitiesEngine
    {
        private readonly bool _enableFirst;

        private readonly IEntityFunctions _entityFunctions;

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

        private void DisableEntities()
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
                        _entityFunctions.SwapEntityGroup<TestEntityDescriptor>(
                            new(entityIDs[i], group),
                            TestGroups.Disabled);
                    }
                }
            }
        }

        private void EnableEntities()
        {
            //Query only disabled entities
            var (entities, entityIDs, count) = entitiesDB.QueryEntities<TestEntityComponent>(TestGroups.Disabled);

            for (var i = 0; i < count; i++)
            {
                ref var entity = ref entities[i];

                if (entity.Enabled)
                {
                    //Enable disabled entities
                    _entityFunctions.SwapEntityGroup<TestEntityDescriptor>(
                        new(entityIDs[i], TestGroups.Disabled),
                        TestGroups.TestGroupTag.BuildGroup);
                }
            }
        }
    }

    private class SyncFiltersEngine : IQueryingEntitiesEngine,
        IReactOnAddEx<TestEntityComponent>,
        IReactOnSwapEx<TestEntityComponent>
    {
        public EntitiesDB entitiesDB { get; set; }

        public void Add(
            (uint start, uint end) rangeOfEntities,
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
                AddToFilter(entityIDs[i], groupID, i);
            }
        }

        public void MovedTo(
            (uint start, uint end) rangeOfEntities,
            in EntityCollection<TestEntityComponent> entities,
            ExclusiveGroupStruct fromGroup,
            ExclusiveGroupStruct toGroup)
        {
            var disabled = toGroup.Equals(TestGroups.Disabled);
            var enabled  = fromGroup.Equals(TestGroups.Disabled);

            var (_, entityIDs, _) = entities;

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                //if we enable entity - add to filter
                if (enabled)
                {
                    AddToFilter(entityIDs[i], toGroup, i);
                }

                //if we disable entity - remove from filter
                if (disabled)
                {
                    RemoveFromFilter(entityIDs[i], toGroup);
                }
            }
        }

        public void Ready() { }

        private void AddToFilter(in uint entityID, in ExclusiveGroupStruct groupID, in uint index)
        {
            var     filters = entitiesDB.GetFilters();
            ref var filter  = ref filters.GetOrCreatePersistentFilter<TestEntityComponent>(TestFilters.TestID);
            filter.Add(new(entityID, groupID), index);
        }

        private void RemoveFromFilter(in uint entityID, in ExclusiveGroupStruct groupID)
        {
            var     filters = entitiesDB.GetFilters();
            ref var filter  = ref filters.GetOrCreatePersistentFilter<TestEntityComponent>(TestFilters.TestID);
            EGID    egid    = new(entityID, groupID);
            filter.Remove(egid);
        }
    }
}
