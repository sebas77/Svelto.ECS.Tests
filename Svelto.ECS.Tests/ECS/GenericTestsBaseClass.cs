using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS
{
    public class GenericTestsBaseClass
    {
        [SetUp]
        public void Init()
        {
            _scheduler         = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot       = new EnginesRoot(_scheduler);
            _factory           = _enginesRoot.GenerateEntityFactory();
            _functions         = _enginesRoot.GenerateEntityFunctions();
            _neverdoThisEngine = new TestEngine();

            _enginesRoot.AddEngine(_neverdoThisEngine);
        }

        [TearDown]
        public void Cleanup()
        {
            _functions.RemoveEntitiesFromGroup(GroupA);
            _functions.RemoveEntitiesFromGroup(GroupB);
            _scheduler.SubmitEntities();
        }

        protected EntityComponentInitializer CreateTestEntity(uint entityId, ExclusiveGroupStruct group, int value = 1)
        {
            var initializer = _factory.BuildEntity<EntityDescriptorWithComponentAndViewComponent>
                (entityId, group, new object[] {new TestFloatValue(value), new TestIntValue(value)});
            initializer.Init(new TestEntityComponent(value, value));
            return initializer;
        }

        protected SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot                                 _enginesRoot;
        IEntityFactory                              _factory;
        protected IEntityFunctions                  _functions;
        protected TestEngine                        _neverdoThisEngine;

        protected static ExclusiveGroup                   GroupA  = new ExclusiveGroup();
        protected static ExclusiveGroup                   GroupB  = new ExclusiveGroup();
        protected static FasterList<ExclusiveGroupStruct> GroupAB = new FasterList<ExclusiveGroupStruct>().Add(GroupA).Add(GroupB);
    }
}