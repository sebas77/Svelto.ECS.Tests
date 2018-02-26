using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.ECS;

namespace UnitTests
{
    public class TestAddAndRemove
    {
        public TestAddAndRemove()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleSubmissionEntityViewScheduler();
            _enginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
            
            _enginesRoot.AddEngine(new TestRemoveEntityEngine(_entityFactory, _entityFunctions));
        }

        EnginesRoot _enginesRoot;
        IEntityFactory _entityFactory;
        IEntityFunctions _entityFunctions;
        SimpleSubmissionEntityViewScheduler _simpleSubmissionEntityViewScheduler;
    }

    public class TestRemoveEntityEngine : IQueryingEntityViewEngine
    {
        public TestRemoveEntityEngine(IEntityFactory entityFactory, IEntityFunctions entityFunctions)
        {
            TestMethod();
        }

        void TestMethod()
        {
            throw new System.NotImplementedException();
        }

        public IEntityViewsDB entityViewsDB { get; set; }
        public void Ready()
        {}
    }

    class TestDescriptor:GenericEntityDescriptor<TestEntityView>
    {
    }

    class TestEntityView : EntityView
    {
    }
}
