using System;
using System.Runtime.InteropServices;
using System.Text;
using DBC.ECS;
using NUnit.Framework;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schedulers;
using Assert = NUnit.Framework.Assert;

namespace Svelto.ECS.Tests.Messy
{
    [TestFixture]
    public class TestSveltoECS
    {
        static readonly ExclusiveGroup group1  = new ExclusiveGroup();
        static readonly ExclusiveGroup group2  = new ExclusiveGroup();
        static readonly ExclusiveGroup group3  = new ExclusiveGroup();
        static readonly ExclusiveGroup group6  = new ExclusiveGroup();
        static readonly ExclusiveGroup group7  = new ExclusiveGroup();
        static readonly ExclusiveGroup group8  = new ExclusiveGroup();
        static readonly ExclusiveGroup group0  = new ExclusiveGroup();
        static readonly ExclusiveGroup groupR4 = new ExclusiveGroup(4);

        [SetUp]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot                         = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest         = new TestEngine();

            _enginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            _entityFactory   = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
        }

        [TearDown]
        public void Dispose() { _enginesRoot.Dispose(); }
        
        [TestCase]
        public void TestBuildEntityViewComponentWithoutImplementors()
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(1, group1));
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.Throws(typeof(PreconditionException), CheckFunction); 
        }
        
        [TestCase]
        public void TestBuildEntityViewComponentWithWrongImplementors()
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorWrongEntityViewInterface>(new EGID(1, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.Throws(typeof(TypeInitializationException), CheckFunction); 
        }

        [TestCase]
        public void TestWrongEntityViewComponent()
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorWrongEntityView>(new EGID(1, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities(); 
            }

            Assert.Throws<TypeInitializationException>(CheckFunction); //it's TypeInitializationException because the Type is not being constructed due to the ECSException
        }
        
        [TestCase]
        public void TestWrongEntityViewComponent2()
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorWrongEntityView>(new EGID(1, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.Throws<TypeInitializationException>(CheckFunction); 
        }
        
        [TestCase]
        public void TestWrongEntityViewComponent3()
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorWrongEntityViewInterface>(new EGID(1, group1), new[] {new TestItWrong()});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.Throws<TypeInitializationException>(CheckFunction); 
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(uint id)
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.Throws(typeof(ECSException), CheckFunction);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestCreationAndRemovalOfDynamicEntityDescriptors(uint id)
        {
            var ded = new DynamicEntityDescriptor<TestDescriptorEntityView>(new IComponentBuilder[]
            {
                new ComponentBuilder<TestEntityComponent>()
            });

            bool hasit;
            //Build Entity id, group0
            {
                _entityFactory.BuildEntity(new EGID(id, group0), ded, new[] {new TestIt(2)});

                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(id, group0));

                Assert.IsTrue(hasit);
            }

            //Swap Entity id, group0 to group 3
            {
                _entityFunctions.SwapEntityGroup<TestDescriptorEntityView>(new EGID(id, group0), group3);

                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(id, group3));

                Assert.IsTrue(hasit);
            }

            _entityFunctions.Remove<TestDescriptorEntityView>(new EGID(id, group3));

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(id, group3));

            Assert.IsFalse(hasit);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(uint id)
        {
            void CheckFunction()
            {
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});

                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                _entityFactory.BuildEntity<TestDescriptorEntityView2>(new EGID(id, group0), new[] {new TestIt(2)});

                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }

            Assert.That(CheckFunction, Throws.TypeOf<ECSException>());
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroup(uint id)
        {
            bool crashed = false;

            try
            {
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});
                _entityFactory.BuildEntity<TestDescriptorEntityView2>(new EGID(id, group0), new[] {new TestIt(2)});
            }
            catch
            {
                crashed = true;
            }

            Assert.IsTrue(crashed);
        }
        
        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroup(uint id)
        {
            bool crashed = false;

            try
            {
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});
                _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});
            }
            catch
            {
                crashed = true;
            }

            Assert.IsTrue(crashed);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestTwoEntitiesWithSameIDWorksOnDifferentGroups(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestRemove(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.Remove<TestDescriptorEntityView>(new EGID(id, group1));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntity(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityWithImplementor(uint id)
        {
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));

            var entityView =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<TestEntityViewComponent>(new EGID(id, group1));
            Assert.AreEqual(entityView.TestIt.value, 2);

            uint index;
            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group1), out index)[index].TestIt.value, 2);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityViewComponent(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntitytruct(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntity>(new EGID(id, group1));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityComponentWithInitializer(uint id)
        {
            var init = _entityFactory.BuildEntity<TestDescriptorEntity>(new EGID(id, group1));
            init.Init(new TestEntityComponent(3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityComponent>(group1));
            uint index;
            Assert.IsTrue(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityComponent>(
                    new EGID(id, group1), out index)[index].value == 3);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityMixed(uint id)
        {
            TestIt testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id, group1), new[] {testIt});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityComponent>(group1));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
            var (entityCollection, count) =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewComponent>(group1);
            Assert.AreSame(entityCollection[0].TestIt, testIt);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityWithViewStructWithImplementorAndTestQueryEntitiesAndIndex(uint id)
        {
            var testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {testIt});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));

            uint index;
            var testEntityView2 =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group1), out index)[index];

            Assert.AreEqual(testEntityView2.TestIt, testIt);
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityToGroupWithDescriptorInfo(uint id)
        {
            _entityFactory.BuildEntity(new EGID(id, group1), new TestDescriptorEntityView(), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestBuildEntityInAddFunction(uint id)
        {
            _enginesRoot.AddEngine(new TestEngineAdd(_entityFactory));
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities(); //submit the entities
            _simpleSubmissionEntityViewScheduler.SubmitEntities(); //now submit the entities added by the engines
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(100, group0)));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestRemoveFromGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));

            _entityFunctions.Remove<TestDescriptorEntityView>(id, group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestRemoveGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));

            _entityFunctions.RemoveEntitiesFromGroup(group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestRemoveAndAddAgainEntity(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.Remove<TestDescriptorEntityView>(id, group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group1));
        }

        [TestCase((uint) 0)]
        [TestCase((uint) 1)]
        [TestCase((uint) 2)]
        public void TestSwapGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptorEntityView>(new EGID(id, group0), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.SwapEntityGroup<TestDescriptorEntityView>(id, group0, group3);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group0)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group0));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group3));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group3)));

            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group3), out var index)[index].ID.entityID, id);
            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group3), out index)[index].ID.groupID.id, (group3.id));

            _entityFunctions.SwapEntityGroup<TestDescriptorEntityView>(id, group3, group0);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group3));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(id, group3)));

            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group0), out index)[index].ID.entityID, id);
            Assert.AreEqual(
                  _neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewComponent>(
                    new EGID(id, group0), out index)[index].ID.groupID.id, group0.id);
        }

        [TestCase((uint) 0, (uint) 1, (uint) 2, (uint) 3)]
        [TestCase((uint) 4, (uint) 5, (uint) 6, (uint) 7)]
        [TestCase((uint) 8, (uint) 9, (uint) 10, (uint) 11)]
        public void TestExecuteOnAllTheEntities(uint id, uint id2, uint id3, uint id4)
        {
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id, groupR4), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id2, groupR4 + 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id3, groupR4 + 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponent>(
                new EGID(id4, groupR4 + 3), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            foreach (var ((entity, groupCount), _) in _neverDoThisIsJustForTheTest.entitiesDB
               .QueryEntities<TestEntityViewComponent>())
            {
                for (int i = 0; i < groupCount; i++)
                    entity[i].TestIt.value = entity[i].ID.entityID;
            }

            ;

            foreach (var ((entity, groupCount), _) in _neverDoThisIsJustForTheTest.entitiesDB
               .QueryEntities<TestEntityComponent>())
            {
                for (int i = 0; i < groupCount; i++)
                    entity[i].value = entity[i].ID.entityID;
            }

            ;

            for (uint i = 0; i < 4; i++)
            {
                var (buffer1, count) =
                    _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityComponent>(groupR4 + i);
                var (buffer2, count2) =
                    _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewComponent>(groupR4 + i);

                Assert.AreEqual(count, 1);
                Assert.AreEqual(count2, 1);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(buffer1[j].value, buffer1[j].ID.entityID);
                    Assert.AreEqual(buffer2[j].TestIt.value, buffer2[j].ID.entityID);
                }
            }

            _entityFunctions.Remove<TestEntityWithComponentViewAndComponent>(new EGID(id, groupR4));
            _entityFunctions.Remove<TestEntityWithComponentViewAndComponent>(new EGID(id2, groupR4 + 1));
            _entityFunctions.Remove<TestEntityWithComponentViewAndComponent>(new EGID(id3, groupR4 + 2));
            _entityFunctions.Remove<TestEntityWithComponentViewAndComponent>(new EGID(id4, groupR4 + 3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            foreach (var (_, _) in _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewComponent>())
            {
                Assert.Fail();
            }

            foreach (var (_, _) in _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityComponent>())
            {
                Assert.Fail();
            }
        }

        [TestCase]
        public void QueryingNotExistingViewsInAnExistingGroupMustNotCrash()
        {
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewComponent>(group0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroupArray<TestEntityViewComponent>(group0));
        }

        [TestCase]
        public void TestExtendibleDescriptor()
        {
            _entityFactory.BuildEntity<B>(new EGID(1, group0));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<A>(new EGID(1, group0), group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<EVS2>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<EVS2>(new EGID(1, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<EVS1>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<EVS1>(new EGID(1, group1)));
        }

        [TestCase]
        public void TestExtendibleDescriptor2()
        {
            _entityFactory.BuildEntity<B2>(new EGID(1, group0), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<A2>(new EGID(1, group0), group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewComponent>(new EGID(1, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityComponent>(new EGID(1, group1)));
        }

        [TestCase]
        public void TestQueryEntitiesWithMultipleParamsTwoStructs()
        {
            for (int i = 0; i < 100; i++)
            {
                var init = _entityFactory.BuildEntity<TestDescriptorWith2Components>(new EGID((uint) i, group0));
                init.Init(new TestEntityComponent((uint) (i)));
                init.Init(new TestEntityComponent2((uint) (i + 100)));
            }

            for (int i = 0; i < 100; i++)
            {
                var init = _entityFactory.BuildEntity<TestDescriptorWith2Components>(new EGID((uint) i, group1));
                init.Init(new TestEntityComponent((uint) (i + 200)));
                init.Init(new TestEntityComponent2((uint) (i + 300)));
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            var iterators = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityComponent, TestEntityComponent2>(
                new LocalFasterReadOnlyList<ExclusiveGroupStruct>(
                    new FasterList<ExclusiveGroupStruct>(new ExclusiveGroupStruct[] {group0, group1})));

            uint index = 0;

            foreach (var ((iteratorentityComponentA, iteratorentityComponentB, count), exclusiveGroupStruct) in
                iterators)
            {
                for (int i = 0; i < count; i++)
                {
                    if (exclusiveGroupStruct == group0)
                    {
                        Assert.AreEqual(iteratorentityComponentA[i].value, index);
                        Assert.AreEqual(iteratorentityComponentB[i].value, index + 100);
                    }
                    else
                    {
                        Assert.AreEqual(iteratorentityComponentA[i].value, index + 200);
                        Assert.AreEqual(iteratorentityComponentB[i].value, index + 300);
                    }

                    index = ++index % 100;
                }
            }
        }

        [TestCase]
        public void TestQueryEntitiesWithMultipleParamsOneStruct()
        {
            for (int i = 0; i < 100; i++)
            {
                var init = _entityFactory.BuildEntity<TestDescriptorWith2Components>(new EGID((uint) i, group0));
                init.Init(new TestEntityComponent((uint) (i)));
                init.Init(new TestEntityComponent2((uint) (i + 100)));
            }

            for (int i = 0; i < 100; i++)
            {
                var init = _entityFactory.BuildEntity<TestDescriptorWith2Components>(new EGID((uint) i, group1));
                init.Init(new TestEntityComponent((uint) (i + 200)));
                init.Init(new TestEntityComponent2((uint) (i + 300)));
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<ExclusiveGroupStruct> groupStructId =
                new FasterList<ExclusiveGroupStruct>(new ExclusiveGroupStruct[] {group0, group1});
            var iterators = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityComponent>(groupStructId);

            uint index = 0;

            foreach (var ((iterator, count), exclusiveGroupStruct) in iterators)
            {
                for (int i = 0; i < count; i++)
                {
                    if (iterator[i].ID.groupID == group0)
                        Assert.IsTrue(iterator[i].value == index);
                    else
                        Assert.IsTrue(iterator[i].value == (index + 200));

                    index = ++index % 100;
                }
            }
        }

        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _entityFactory;
        IEntityFunctions                  _entityFunctions;
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                        _neverDoThisIsJustForTheTest;

        class TestEngineAdd : IReactOnAddAndRemove<TestEntityViewComponent>
        {
            public TestEngineAdd(IEntityFactory entityFactory) { _entityFactory = entityFactory; }

            public void Add(ref TestEntityViewComponent entityView, EGID egid)
            {
                _entityFactory.BuildEntity<TestDescriptorEntity>(new EGID(100, group0));
            }

            public void Remove(ref TestEntityViewComponent entityView, EGID egid)
            {
                // Svelto.ECS.Tests\Svelto.ECS\DataStructures\TypeSafeDictionary.cs:line 196
                // calls Remove - throwing NotImplementedException here causes test host to
                // crash in Visual Studio or when using "dotnet test" from the command line
                // throw new NotImplementedException();
            }

            readonly IEntityFactory _entityFactory;
        }

        internal class TestEngine : IQueryingEntitiesEngine
        {
            public EntitiesDB entitiesDB { get; set; }
            public void       Ready()    { }

            public bool HasEntity<T>(EGID ID) where T : struct, IEntityComponent { return entitiesDB.Exists<T>(ID); }

            public bool HasAnyEntityInGroup<T>(ExclusiveGroup groupID) where T : struct, IEntityComponent
            {
                return entitiesDB.QueryEntities<T>(groupID).count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(ExclusiveGroup groupID) where T : struct, IEntityComponent
            {
                return entitiesDB.QueryEntities<T>(groupID).count > 0;
            }
        }
    }

    struct EVS1 : IEntityComponent
    { }

    struct EVS2 : IEntityComponent
    { }

    class A : GenericEntityDescriptor<EVS1> { }

    class B : ExtendibleEntityDescriptor<A>
    {
        static readonly IComponentBuilder[] _nodesToBuild;

        static B() { _nodesToBuild = new IComponentBuilder[] {new ComponentBuilder<EVS2>(),}; }

        public B() : base(_nodesToBuild) { }
    }

    class A2 : GenericEntityDescriptor<TestEntityViewComponent> { }

    class B2 : ExtendibleEntityDescriptor<A2>
    {
        static readonly IComponentBuilder[] _nodesToBuild;

        static B2() { _nodesToBuild = new IComponentBuilder[] {new ComponentBuilder<TestEntityComponent>(),}; }

        public B2() : base(_nodesToBuild) { }
    }

    class TestDescriptorEntityView : GenericEntityDescriptor<TestEntityViewComponent> { }
    class TestDescriptorEntityView2 : GenericEntityDescriptor<TestEntityViewComponent> { }
    class TestDescriptorEntity : GenericEntityDescriptor<TestEntityComponent> { }
    class TestEntityWithComponentViewAndComponent : GenericEntityDescriptor<TestEntityViewComponent, TestEntityComponent
    > { }
    class TestDescriptorWith2Components : GenericEntityDescriptor<TestEntityComponent, TestEntityComponent2> { }
    class TestDescriptorWrongEntityView : GenericEntityDescriptor<TestWrongComponent> { }
    class TestDescriptorWrongEntityViewInterface : GenericEntityDescriptor<TestEntityViewComponentWrongInterface> { }

    struct TestWrongComponent : IEntityViewComponent
    {
        public EGID ID { get; set; }
    }
    
    struct TestEntityComponent : IEntityComponent, INeedEGID
    {
        public uint value;

        public TestEntityComponent(uint value) : this() { this.value = value; }

        public EGID ID { get; set; }
    }

    struct TestEntityComponent2 : IEntityComponent
    {
        public uint value;

        public TestEntityComponent2(uint value) : this() { this.value = value; }
    }

    struct TestEntityViewComponent : IEntityViewComponent
    {
#pragma warning disable 649
        public ITestIt TestIt;
#pragma warning restore 649

        public EGID ID { get; set; }
    }
    
    struct TestEntityViewComponentWrongInterface : IEntityViewComponent
    {
#pragma warning disable 649
        public ITestItWrong TestIt;
#pragma warning restore 649

        public EGID ID { get; set; }
    }

    interface ITestItWrong
    {
        object doc               { get; set; } //field necessary for tests
    }

    class Transform : IImplementor
    {
        public readonly int value;
        public Transform(int i) { value = i; }
    }

    class TestItWrong : ITestItWrong
    {
        public object doc { get; set; }
    }

    interface ITestIt
    {
        float                  value     { get; set; }
        int                    testValue { get; }
    }

    class TestIt : ITestIt, IImplementor
    {
        public TestIt(int i)
        {
            value = i;
        }

        public float value     { get; set; }
        public int   testValue { get; }
    }
}