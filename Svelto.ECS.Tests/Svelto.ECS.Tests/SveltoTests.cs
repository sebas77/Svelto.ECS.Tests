using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.ECS;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;

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

        public void TestRemoveEntityThrowExceptionIfNotGoundInGroup()
        {}

        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(int id)
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
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroup(int id)
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
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(int id)
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
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroup(int id)
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

            var entityView = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntityView<TestEntityView>(new EGID(id, id));
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

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntitytruct(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor7>(id, id, null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(id));
        }

        [TestMethod]
        [ExpectedException(typeof(EntityStructException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildInvalidEntitytructMustThrow(int id)
        {
            try
            {
                _entityFactory.BuildEntity<TestDescriptor6>(id, id, null);
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch (Exception e)
            {
                throw e.InnerException.InnerException.InnerException;
            }

        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntitytructWithInitializer(int id)
        {
            var init = _entityFactory.BuildEntity<TestDescriptor7>(id, id, null);
            init.Init(new TestEntityStruct(3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(id));
            uint index;
            Assert.IsTrue(
                _neverDoThisIsJustForTheTest.entitiesDB.
                    QueryEntitiesAndIndex<TestEntityStruct>(new EGID(id, id),
                    out index)[index].TestIt ==  3);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityMixed(int id)
        {
            TestIt testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptor5>
                (id, id, new[] { testIt });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.
                HasAnyEntityInGroup<TestEntityViewStruct>(id));
            int count;
            Assert.AreSame(_neverDoThisIsJustForTheTest.entitiesDB.
                              QueryEntities<TestEntityViewStruct>(id, out count)
                [0].TestIt, testIt);
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityWithViewStructWithImplementorAndTestQueryEntitiesAndIndex(int id)
        {
            var testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptor4>(id, id, 
                new[] {testIt});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.
                HasAnyEntityInGroup<TestEntityViewStruct>(id));

            uint index;
            var testEntityView2 = _neverDoThisIsJustForTheTest.entitiesDB.
                QueryEntitiesAndIndex<TestEntityViewStruct>
                    (new EGID(id, id), out index)[index];

            Assert.AreEqual(testEntityView2.TestIt, testIt);
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityToGroupWithDescriptorInfo(int id)
        {
            _entityFactory.BuildEntity(id, id, EntityDescriptorTemplate<TestDescriptor>.descriptor.entitiesToBuild, new[] {new TestIt(2)});
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
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, 3)));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void QueryingNotExistingViewsInAnExistingGroupMustNotCrash(int id)
        {
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroupArray<TestEntityViewStruct>(id));
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
        
        class TestDescriptor4 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor5 : 
            GenericEntityDescriptor<TestEntityViewStruct, TestEntityView>
        {}
        
        class TestDescriptor6 : GenericEntityDescriptor<TestInvalidEntityStruct>
        {}

        class TestDescriptor7 : GenericEntityDescriptor<TestEntityStruct>
        { }

        class TestEntityView : EntityView
        {
            public ITestIt TestIt;
        }
        
        class TestInvalidEntityStruct : IEntityStruct
        {
            public ITestIt TestIt;

            public EGID ID { get; set; }
        }

        struct TestEntityStruct : IEntityStruct
        {
            public int TestIt;

            public TestEntityStruct(int testIt):this()
            {
                TestIt = testIt;
            }

            public EGID ID { get; set; }
        }

        struct TestEntityViewStruct : IEntityViewStruct
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

        class TestEngine: IQueryingEntitiesEngine
        {
            public IEntitiesDB entitiesDB { get; set; }
            public void Ready() {}

            public bool HasEntity<T>(EGID ID) where T : EntityView
            {
                return entitiesDB.QueryEntityView<T>(ID) != null;
            }

            public bool HasEntityInStandardGroup<T>(int ID) where T : EntityView
            {
                return entitiesDB.QueryEntityViews<T>().Count != 0;
            }

            public bool HasAnyEntityInGroup<T>(int groupID) where T : IEntityStruct
            {
                int count;
                entitiesDB.QueryEntities<T>(groupID, out count);
                return count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(int groupID) where T:IEntityStruct
            {
                int count;
                entitiesDB.QueryEntities<T>(groupID, out count);

                return count != 0;
            }
        }
    }
}
