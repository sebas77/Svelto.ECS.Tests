using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.ECS;
using Svelto.ECS.Internal;

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
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleAddIntervaled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleAdd(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleEntityViewAddIntervalled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor2>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleEntityViewAdd(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _entityFactory.BuildEntity<TestDescriptor2>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDThrowsIntervalled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor3>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeFasterListForECSException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDThrows(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(id, null);
            _entityFactory.BuildEntity<TestDescriptor3>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDWorksOnDifferentGroups(int id)
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor2>(id, id, null);
            _entityFactory.BuildEntityInGroup<TestDescriptor3>(id, id+1, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id+1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyNEntityInGlobalPool<TestEntityView>(2));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity(id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntityInStandardGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestAddEntityToGroup(int id)
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(id, id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestAddEntityToGroupWithDescriptorInfo(int id)
        {
            _entityFactory.BuildEntityInGroup(id, id, EntityDescriptorTemplate<TestDescriptor>.Default, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntityFromGroup(int id)
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(id, id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity(id, id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntityGroup(int id)
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(id, id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveGroupAndEntities(id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveSwapGroup(int id)
        {
            _entityFactory.BuildEntityInGroup<TestDescriptor>(id, id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.SwapEntityGroup(id, id, 3);

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(3));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void QueryingNotExistingViewsInAnExistingGroupMustNotCrash(int id)
        {
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView2>(id));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroupArray<TestEntityView2>(id));
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
        
        class TestDescriptor3 : GenericEntityDescriptor<TestEntityView>
        {}

        class TestEntityView : EntityView
        {}
        
        class TestEntityView2 : EntityView
        {}

        class TestEngine: IQueryingEntityViewEngine
        {
            public IEntityViewsDB entityViewsDB { get; set; }
            public void Ready() {}

            public bool HasAnyNEntityInGlobalPool<T>(int amount) where T : EntityView
            {
                return entityViewsDB.QueryEntityViews<T>().Count == amount;
            }

            public bool HasEntity<T>(EGID ID) where T : EntityView
            {
                return entityViewsDB.QueryEntityView<T>(ID) != null;
            }

            public bool HasEntityInStandardGroup<T>(int ID) where T : EntityView
            {
                return entityViewsDB.QueryEntityView<T>(ID) != null;
            }

            public bool HasAnyEntityInGroup<T>(int groupID) where T : EntityView
            {
                return entityViewsDB.QueryGroupedEntityViews<T>(groupID).Count != 0;
            }

            public bool HasAnyEntityInGroupArray<T>(int groupID) where T:IEntityView
            {
                int count;
                entityViewsDB.QueryGroupedEntityViewsAsArray<T>(groupID, out count);

                return count != 0;
            }
        }
    }
}
