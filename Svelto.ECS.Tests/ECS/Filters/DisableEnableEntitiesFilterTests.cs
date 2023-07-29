using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS;

[TestFixture]
public partial class DisableEnableEntitiesFilterTests
{
    EnginesRoot _enginesRoot;
    IUnitTestingInterface _entitiesDB;
    IEntityFactory _factory;
    IEntityFunctions _functions;
    SimpleEntitiesSubmissionScheduler _scheduler;

    [SetUp]
    public void SetUp()
    {
        _scheduler = new();
        _enginesRoot = new(_scheduler);
        _factory = _enginesRoot.GenerateEntityFactory();
        _functions = _enginesRoot.GenerateEntityFunctions();
        _entitiesDB = _enginesRoot;
    }

    [Test]
    public void TestEntitiesGroupChange_EnableFirst()
    {
        //engines
        var changeValueEngine = new ChangeActiveEntityValueEngine();
        var syncEngine = new SyncEntitiesStateEngine(_functions, true); //-> we setup filters swap engine to run enable first
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
        var syncEngine = new SyncEntitiesStateEngine(_functions, false); //-> we setup filters swap engine to run disable first
        _enginesRoot.AddEngine(changeValueEngine);
        _enginesRoot.AddEngine(syncEngine);
        _enginesRoot.AddEngine(new SyncFiltersEngine());

        RunTestLogic(syncEngine, changeValueEngine);
    }

    void RunTestLogic(SyncEntitiesStateEngine syncEngine, ChangeActiveEntityValueEngine changeValueEngine)
    {
        //We build two entities
        var initializer0 = _factory.BuildEntity<TestEntityDescriptor>(0, TestGroups.TestGroupTag.BuildGroup);
        var initializer1 = _factory.BuildEntity<TestEntityDescriptor>(1, TestGroups.TestGroupTag.BuildGroup);

        //Both entities are enabled, first one has value 0, second one has value 2
        initializer0.Init(
            new TestEntityComponent
            {
                Enabled = true,
                SomeValue = 0
            });
        initializer1.Init(
            new TestEntityComponent
            {
                Enabled = true,
                SomeValue = 2
            });

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

    void CheckAssertions(EntityReference entityRef0, EntityReference entityRef1)
    {
        //query entities data from previously stored references
        var egid0 = entityRef0.ToEGID(_entitiesDB.entitiesForTesting);
        var egid1 = entityRef1.ToEGID(_entitiesDB.entitiesForTesting);
        ref var entity0 = ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(egid0);
        ref var entity1 = ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(egid1);

        var filter = _entitiesDB.entitiesForTesting.GetFilters().GetPersistentFilter<TestEntityComponent>(TestFilters.TestID);

        foreach (var (indices, group) in filter)
        {
            Assert.IsTrue(group.IsEnabled());
            
            var count = indices.count;

            var (buffer, entityIDs, _) = _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(group);

            var assertionsCount = 0;

            for (var i = 0; i < count; i++)
            {
                var filterEntity = buffer[indices[i]];
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

    void ChangeEntityEnabledFlag(EntityReference entityRef, bool enabled)
    {
//query entity with given ref and swap enabled flag
        ref var entity =
                ref _entitiesDB.entitiesForTesting.QueryEntity<TestEntityComponent>(entityRef.ToEGID(_entitiesDB.entitiesForTesting));
        entity.Enabled = enabled;
    }

    void RunSingleFrame(IStepEngine syncEngine, IStepEngine changeValueEngine)
    {
        //single frame consists always from three elements:
        syncEngine.Step(); //-> run filter add/remove sync engine
        changeValueEngine.Step(); //-> change values on enabled entities
        _scheduler.SubmitEntities(); //-> submit
    }

    struct TestEntityComponent: IEntityComponent
    {
        internal bool Enabled;
        internal int SomeValue;
    }

    static class TestGroups
    {
        internal static readonly ExclusiveGroupStruct Disabled = new ExclusiveGroup(ExclusiveGroupBitmask.DISABLED_BIT);

        internal class TestGroupTag: GroupTag<TestGroupTag> { }
    }

    static class TestFilters
    {
        static readonly FilterContextID TestContext = EntitiesDB.SveltoFilters.GetNewContextID();
        internal static readonly CombinedFilterID TestID = new(0, TestContext);
    }

    class TestEntityDescriptor: GenericEntityDescriptor<TestEntityComponent> { }

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

            var (_, entityIDs, _) = entities;

            var filters = entitiesDB.GetFilters();
            ref var filter = ref filters.GetOrCreatePersistentFilter<TestEntityComponent>(TestFilters.TestID);

            var map = entitiesDB.QueryMappedEntities<TestEntityComponent>(toGroup);

            for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
            {
                //if we enable entity - add to filter
                var entityId = new EGID(entityIDs[i], toGroup);
                if (willBeEnabled)
                {
                    filter.Add(entityId, map);
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
}