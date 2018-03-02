using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.ECS;

namespace UnitTests
{
    [TestClass]
    public class TestAddAndRemove
    {
        [TestInitialize]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleSubmissionEntityViewScheduler();
            _enginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();
            _enginesRoot.AddEngine(_neverDoThisIsJustForTheTest);
            _entityFactory = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
        }

        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestExceptionThrownOnDoubleAddIntervalled()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestExceptionThrownOnDoubleAdd()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestExceptionThrownOnDoubleEntityViewAddIntervalled()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor2>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestExceptionThrownOnDoubleEntityViewAdd()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _entityFactory.BuildEntity<TestDescriptor2>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestTwoEntitiesWithSameIDThrowsIntervalled()
        {
            _entityFactory.BuildEntity<TestDescriptor2>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor3>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestTwoEntitiesWithSameIDThrows()
        {
            _entityFactory.BuildEntity<TestDescriptor2>(0, null);
            _entityFactory.BuildEntity<TestDescriptor3>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        public void TestMetaEntityAndEntityIsOkWithSameID()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _entityFactory.BuildMetaEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(0);
            _neverDoThisIsJustForTheTest.HasMetaEntity<TestEntityView>(0);
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        public void TestTwoEntitiesWithSameIDRegardlessTheGroupThrows()
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor2>(0, 0, null);
            _entityFactory.BuildEntityInGroup<TestDescriptor3>(0, 1, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        public void TestRemoveEntity()
        {
            _entityFactory.BuildEntity<TestDescriptor>(0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity(0);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(0));
        }
        
        [TestMethod]
        public void TestAddEntityToGroup()
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(0, 0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(0));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntitiesInGroup<TestEntityView>(0));
        }

        [TestMethod]
        public void TestRemoveEntityFromGroup()
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(0, 0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity(0);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntitiesInGroup<TestEntityView>(0));
        }
        
        [TestMethod]
        public void TestRemoveEntityGroup()
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(0, 0, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveGroupAndEntities(0);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntitiesInGroup<TestEntityView>(0));
        }

        EnginesRoot                         _enginesRoot;
        IEntityFactory                      _entityFactory;
        IEntityFunctions                    _entityFunctions;
        SimpleSubmissionEntityViewScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                          _neverDoThisIsJustForTheTest;

        class TestDescriptor : GenericEntityDescriptor<TestEntityView>
        {}
        
        class TestDescriptor2 : GenericEntityDescriptor<TestEntityView>
        {}
        
        class TestDescriptor3 : GenericEntityDescriptor<TestEntityView2>
        {}

        class TestEntityView : EntityView
        {}
        
        class TestEntityView2 : EntityView
        {}

        class TestEngine: IQueryingEntityViewEngine
        {
            public IEntityViewsDB entityViewsDB { get; set; }
            public void Ready() {}

            public bool HasEntity<T>(int ID) where T : EntityView
            {
                T view;
                return entityViewsDB.TryQueryEntityView(ID, out view);
            }
            
            public bool HasEntitiesInGroup<T>(int ID) where T : EntityView
            {
                return entityViewsDB.QueryGroupedEntityViews<T>(ID).Count != 0;
            }

            public bool HasMetaEntity<T>(int ID) where T : EntityView
            {
                T view;
                return entityViewsDB.TryQueryMetaEntityView(ID, out view);
            }
        }
    }
}
