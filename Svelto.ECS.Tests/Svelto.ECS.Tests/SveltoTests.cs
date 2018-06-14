using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.ECS;
using Svelto.DataStructures;

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
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleAddIntervaled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleAdd(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleEntityViewAddIntervalled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor2>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionThrownOnDoubleEntityViewAdd(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor2>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDThrowsIntervalled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor3>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDThrows(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(id, new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor3>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDWorksOnDifferentGroups(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(id, id, new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor3>(id, id+1, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id+1)));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity(id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntityInStandardGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityWithImplementor(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));

            var entityView = _neverDoThisIsJustForTheTest.entityViewsDB.QueryEntityView<TestEntityView>(new EGID(id, id));
            Assert.AreEqual(entityView.TestIt.value, 2);
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityViewStruct(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor4>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView2>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityMixed(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor5>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView2>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityWithViewStructWithImplementor(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor4>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView2>(id));

            uint index;
            var testEntityView2 = _neverDoThisIsJustForTheTest.entityViewsDB.QueryEntities<TestEntityView2>(new EGID(id, id), out index)[index];
            Assert.AreEqual(testEntityView2.TestIt.value, 2);
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityToGroupWithDescriptorInfo(int id)
        {
            _entityFactory.BuildEntity(id, id, EntityDescriptorTemplate<TestDescriptor>.Info.entityViewsToBuild, new[] {new TestIt(2)});
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
            _entityFactory.BuildEntity<TestDescriptor>(id, id, new[] {new TestIt(2)});
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
            _entityFactory.BuildEntity<TestDescriptor>(id, id, new[] {new TestIt(2)});
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
            _entityFactory.BuildEntity<TestDescriptor>(id, id, new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.SwapEntityGroup(id, id, 3);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(3));
            //check egid is correct
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
        
        class TestDescriptors : GenericEntityDescriptor<TestEntityView>
        {}
        
        class TestDescriptor2s : GenericEntityDescriptor<TestEntityView>
        {}
        
        class TestDescriptor3s : GenericEntityDescriptor<TestEntityView>
        {}
        
        class TestDescriptor4 : GenericEntityDescriptor<TestEntityView2>
        {}
        
        class TestDescriptor5 : GenericEntityDescriptor<TestEntityView2, TestEntityView>
        {}
        
        class TestDescriptor6 : GenericEntityDescriptor<TestEntityView2, TestEntityViewS>
        {}

        class TestEntityView : EntityView
        {
            public ITestIt TestIt;
        }
        
        class TestEntityViewS : IEntityStruct
        {
            public ITestIt TestIt;
            public EGID ID { get; set; }
        }
        
        struct TestEntityView2 : IEntityStruct
        {
            public ITestIt TestIt;
            public EGID ID { get; set; }
        }

        interface ITestIt
        {
            float value { get; }
        }

        class TestIt : ITestIt
        {
            public TestIt(int i)
            {
                value = i;
            }

            public float value { get; }
        }

        class TestEngine: IQueryingEntityViewEngine
        {
            public IEntityViewsDB entityViewsDB { get; set; }
            public void Ready() {}

            public bool HasEntity<T>(EGID ID) where T : EntityView
            {
                return entityViewsDB.QueryEntityView<T>(ID) != null;
            }

            public bool HasEntityInStandardGroup<T>(int ID) where T : EntityView
            {
                return entityViewsDB.QueryEntityViews<T>().Count != 0;
            }

            public bool HasAnyEntityInGroup<T>(int groupID) where T : IEntityStruct
            {
                int count;
                entityViewsDB.QueryEntities<T>(groupID, out count);
                return count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(int groupID) where T:IEntityStruct
            {
                int count;
                entityViewsDB.QueryEntities<T>(groupID, out count);

                return count != 0;
            }
        }
    }
}
