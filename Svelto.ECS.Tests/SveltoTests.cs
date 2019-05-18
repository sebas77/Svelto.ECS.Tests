using System;
using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Hybrid;

namespace UnitTests
{
    [TestFixture]
    public class TestAddAndRemove
    {
        static readonly ExclusiveGroup group1 = new ExclusiveGroup();
        static readonly ExclusiveGroup group2 = new ExclusiveGroup();
        static readonly ExclusiveGroup group3 = new ExclusiveGroup();
        static readonly ExclusiveGroup group6 = new ExclusiveGroup();
        static readonly ExclusiveGroup group7 = new ExclusiveGroup();
        static readonly ExclusiveGroup group8 = new ExclusiveGroup();
        static readonly ExclusiveGroup group0 = new ExclusiveGroup();
        static readonly ExclusiveGroup groupR4 = new ExclusiveGroup(4);

        [SetUp]
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
        
        struct Test
        {
            public  int i;

            public Test(int i) : this()
            {
                this.i = i;
            }
        }

        [TestCase]
        public void TestFasterDictionary()
        {
            FasterDictionary<int, Test> test = new FasterDictionary<int, Test>();

            uint dictionarysize = 10000;

            int[] numbers = new int[dictionarysize];

            for (int i = 1; i < dictionarysize; i++)
                numbers[i] = numbers[i - 1] + i * HashHelpers.ExpandPrime((int) dictionarysize);

            for (int i = 0; i < dictionarysize; i++)
                test[i] = new Test(numbers[i]);

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            for (int i = 0; i < dictionarysize; i += 2)
                if (test.Remove(i) == false)
                    throw new Exception();

            test.Trim();
            
            for (int i = 0; i < dictionarysize; i++)
                test[i] = new Test(numbers[i]);
            
            for (int i = 1; i < dictionarysize - 1; i += 2)
                if (test[i].i != numbers[i])
                    throw new Exception();

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            for (int i = (int) (dictionarysize - 1); i >= 0; i -= 3)
                if (test.Remove(i) == false)
                    throw new Exception();

            test.Trim();

            for (int i = (int) (dictionarysize - 1); i >= 0; i -= 3)
                test[i] = new Test(numbers[i]);

            for (int i = 0; i < dictionarysize; i++)
                if (test[i].i != numbers[i])
                    throw new Exception();

            for (int i = 0; i < dictionarysize; i ++)
                if (test.Remove(i) == false)
                    throw new Exception();

            for (int i = 0; i < dictionarysize; i++)
                if (test.Remove(i) == true)
                    throw new Exception();
            
            test.Trim();

            test.Clear();
            for (int i = 0; i < dictionarysize; i++) test[numbers[i]] = new Test(i);

            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar = test[numbers[i]];
                if (JapaneseCalendar.i != i)
                    throw new Exception("read back test failed");
            }

            test = new FasterDictionary<int, Test>();
            for (int i = 0; i < dictionarysize; i++) test[numbers[i]] = new Test(i);

            for (int i = 0; i < dictionarysize; i++)
            {
                Test JapaneseCalendar = test[numbers[i]];
                if (JapaneseCalendar.i != i)
                    throw new Exception("read back test failed");
            }
            
            var test3 = new FasterDictionary<int, int>();

            test3[3] = 5;
            Assert.IsTrue(test3[3] == 5);
            test3[3] = 3;
            Assert.IsTrue(test3[3] == 3);
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(uint id)
        {
            try
            {
                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch (Exception e)
            {
                Assert.That(e.InnerException is TypeSafeDictionaryException);
                return;
            }
            
            Assert.Fail();
        }

        [TestCase]
        public void TestMegaEntitySwap()
        {
            for (int i = 0; i < 29; i++)
            {
                EGID egid = new EGID((uint) i, group1);
                _entityFactory.BuildEntity<TestDescriptor>(egid, new[] { new TestIt(2) });
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            SwapMinNeededForException(_entityFunctions);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            bool allFound = true;
            bool mustNotBeFound = false;

            for (uint i = 0; i < 29; i++)
                allFound &= _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(i, group2));

            for (int i = 0; i < 29; i++)
                mustNotBeFound |= _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID((uint) i, group1));

            
            Assert.IsTrue(allFound);
            Assert.IsTrue(mustNotBeFound == false);
        }

        void SwapMinNeededForException(IEntityFunctions entityFunctions)
        {
            entityFunctions.SwapEntityGroup<TestDescriptor>(18, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(19, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(20, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(21, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(22, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(17, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(16, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(15, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(14, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(13, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(11, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(9, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(6, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(5, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(4, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(3, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(2, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(0, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(24, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(25, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(26, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(27, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(28, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(23, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(8, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(7, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(1, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(12, group1, group2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(10, group1, group2);
        }
        
        
        [TestCase]
        public void TestMegaEntitySwap2()
        {
            unchecked
            {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(5000, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4999, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4998, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4997, group1),  new[] {new TestIt(2)});
        
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4996, group2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4995, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4994, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4993, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4992, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4991, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4990, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4988, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4987, group2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4986, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4985, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4984, group2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4980, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4977, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4976, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4974, group2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4971, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4970, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4967, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4966, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4965, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4964, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4963, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4962, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4961, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4960, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4959, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4958, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4957, group1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4955, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4954, group1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4953, group1),  new[] {new TestIt(2)});
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4996    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4995    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4991    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4984    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974    , group2    , group1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(5000    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4999    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4998    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4997    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4994    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4993    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4990    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4985    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4976    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4967    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4965    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4964    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4963    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4962    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4961    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4960    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4959    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4958    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4957    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4955    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4954    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4953    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4984    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4991    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4995    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4996    , group1    , group6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(5000    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4999    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4998    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4997    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4994    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4993    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4990    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4985    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4976    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4967    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4965    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4964    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4963    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4962    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4961    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4960    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4959    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4958    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4957    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4955    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4954    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4953    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4996    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4995    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4991    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4984    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977    , group6    , group7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974    , group6    , group7);         
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            EGID egid = new EGID(4958, group7);
            bool exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid);
            Assert.IsTrue(exists); 
        
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4961    , group7    , group8);         
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid);
                            
            

            Assert.IsTrue(exists);
            }
        }

        [TestCase]
        public void TestMegaEntitySwap3()
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(5000, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4999, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4998, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4997, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4996, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4995, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4994, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4993, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4992, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4991, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4990, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4989, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4988, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4987, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4986, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4985, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4984, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4983, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4982, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4981, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4980, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4979, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4978, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4977, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4976, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4975, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4974, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4973, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4972, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4971, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4970, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4969, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4968, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4967, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4966, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4965, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4964, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4963, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4962, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4961, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4960, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4959, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4958, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4957, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4956, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4955, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4954, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4953, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4952, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4951, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4950, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4949, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4948, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4947, group2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4946, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4945, group1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(4944, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4987, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4982, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4975, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4968, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4965, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4964, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4960, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4959, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4947, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(5000, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4999, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4998, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4997, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4996, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4995, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4994, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4993, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4991, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4990, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4989, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4985, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4984, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4983, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4981, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4979, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4976, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4973, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4967, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4962, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4961, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4958, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4957, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4956, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4955, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4954, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4953, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4952, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4951, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4950, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4949, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4948, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4946, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4945, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4944, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4963, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4972, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(5000, group2, group0);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4998, group2, group0);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4973, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4947, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4959, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4960, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4964, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4965, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4968, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4975, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4982, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4987, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4999, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4997, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4996, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4995, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4994, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4993, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4991, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4990, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4989, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4985, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4984, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4981, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4979, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4976, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4967, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4962, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4961, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4958, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4957, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4956, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4955, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4954, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4953, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4952, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4951, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4950, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4949, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4948, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4946, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4945, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4944, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4969, group2, group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4973, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4972, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4963, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4987, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4986, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4982, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4977, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4975, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4968, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4965, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4964, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4960, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4959, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4947, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4983, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4972, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4969, group1, group2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4963, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4973, group6, group7);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4966, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4978, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4969, group2, group1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4972, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4983, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4969, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4978, group1, group6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4970, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4972, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4980, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4983, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4974, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4978, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4969, group6, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988, group7, group8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4988, group8, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group7, group8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4971, group8, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992, group7, group8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4992, group8, group7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(4967, group7, group8);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            EGID egid   = new EGID(4985, group7);
            bool exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid);
            
            Assert.IsTrue(exists);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestCreationAndRemovalOfDynamicEntityDescriptors(uint id)
        {
            var ded = new DynamicEntityDescriptor<TestDescriptor>(new IEntityBuilder[] 
                { new EntityBuilder<TestEntityStruct>() });
            
            _entityFactory.BuildEntity(new EGID(id, group0), ded, new[] {new TestIt(2)});
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            var hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(id, group0));
            
            Assert.IsTrue(hasit);
            
            _entityFunctions.SwapEntityGroup<TestDescriptor>(new EGID(id, group0), group3);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(id, group3));
            
            Assert.IsTrue(hasit);
            
            _entityFunctions.RemoveEntity<TestDescriptor>(new EGID(id, group3));
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            hasit = _neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(id, group3));
            
            Assert.IsFalse(hasit);
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroup(uint id)
        {
            bool crashed = false;

            try
            {
                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group0), new[] {new TestIt(2)});
                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group0), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch
            {
                crashed = true;
            }

            Assert.IsTrue(crashed);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(uint id)
        {
            try
            {
                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group0), new[] {new TestIt(2)});


                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, group0), new[] {new TestIt(2)});


                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch (Exception e)
            {
                Assert.That(e.InnerException is TypeSafeDictionaryException);

                return;
            }
            
            Assert.Fail();
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroup(uint id)
        {
            bool crashed = false;

            try
            {
                _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group0), new[] {new TestIt(2)});
                _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, group0), new[] {new TestIt(2)});
                
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch
            {
                crashed = true;
            }

            Assert.IsTrue(crashed);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestTwoEntitiesWithSameIdThrowsIntervaled(uint id)
        {
            try
            {
                _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, group0), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();

                _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, group0), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch (Exception e)
            {
                Assert.That(e.InnerException is TypeSafeDictionaryException);

                return;
            }
            
            Assert.Fail();
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestTwoEntitiesWithSameIDThrows(uint id)
        {
            bool crashed = false;

            try
            {
                _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, group0), new[] {new TestIt(2)});
                _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, group0), new[] {new TestIt(2)});
                _simpleSubmissionEntityViewScheduler.SubmitEntities();
            }
            catch
            {
                crashed = true;
            }

            Assert.IsTrue(crashed);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestTwoEntitiesWithSameIDWorksOnDifferentGroups(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, group0), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestRemoveEntity(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity<TestDescriptor>(new EGID(id, group1));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntity(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityWithImplementor(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));

            var entityView = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<TestEntityViewStruct>(new EGID(id, group1));
            Assert.AreEqual(entityView.TestIt.value, 2);

            uint index;
            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>
                                (new EGID(id, group1), out index)[index].TestIt.value, 2);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityViewStruct(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor4>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntitytruct(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor7>(new EGID(id, group1), null);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(group1));
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityStructWithInitializer(uint id)
        {
            var init = _entityFactory.BuildEntity<TestDescriptor7>(new EGID(id, group1), null);
            init.Init(new TestEntityStruct(3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(group1));
            uint index;
            Assert.IsTrue(
                _neverDoThisIsJustForTheTest.entitiesDB.
                    QueryEntitiesAndIndex<TestEntityStruct>(new EGID(id, group1),
                    out index)[index].value == 3);
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityMixed(uint id)
        {
            TestIt testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptor5>
                (new EGID(id, group1), new[] { testIt });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(group1));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
            uint count;
            Assert.AreSame(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewStruct>(group1, out count)[0].TestIt, testIt);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityWithViewStructWithImplementorAndTestQueryEntitiesAndIndex(uint id)
        {
            var testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptor4>(new EGID(id, group1), 
                new[] {testIt});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.
                HasAnyEntityInGroup<TestEntityViewStruct>(group1));

            uint index;
            var testEntityView2 = _neverDoThisIsJustForTheTest.entitiesDB.
                QueryEntitiesAndIndex<TestEntityViewStruct>
                    (new EGID(id, group1), out index)[index];

            Assert.AreEqual(testEntityView2.TestIt, testIt);
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityToGroupWithDescriptorInfo(uint id)
        {
            _entityFactory.BuildEntity(new EGID(id, group1),
                                       new TestDescriptor(),
                                       new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestBuildEntityInAddFunction(uint id)
        {
            _enginesRoot.AddEngine(new TestEngineAdd(_entityFactory));
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities(); //submit the entities
            _simpleSubmissionEntityViewScheduler.SubmitEntities(); //now submit the entities added by the engines
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(100, group0)));
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestRemoveEntityFromGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));

            _entityFunctions.RemoveEntity<TestDescriptor>(id, group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }
        
        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestRemoveEntityGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
            

            _entityFunctions.RemoveGroupAndEntities(id);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestRemoveAndAddAgainEntity(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity<TestDescriptor>(id, group1);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group1), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group1)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group1));
        }

        [TestCase((uint)0)][TestCase((uint)1)][TestCase((uint)2)]
        public void TestSwapGroup(uint id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, group0), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.SwapEntityGroup<TestDescriptor>(id, group0, group3);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group0)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group0));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group3));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group3)));

            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, group3), out var index)[index].ID.entityID, id);
            Assert.AreEqual((ulong)_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, group3), out index)[index].ID.groupID, (ulong)group3);
            
            _entityFunctions.SwapEntityGroup<TestDescriptor>(id, group3, group0);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group3));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, group3)));

            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, group0), out index)[index].ID.entityID, id);
            Assert.AreEqual((ulong)_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, group0), out index)[index].ID.groupID, (ulong)group0);
        }

        [TestCase((uint)0, (uint)1, (uint)2, (uint)3)]
        [TestCase((uint)4, (uint)5, (uint)6, (uint)7)]
        [TestCase((uint)8, (uint)9, (uint)10, (uint)11)]
        public void TestExecuteOnAllTheEntities(uint id, uint id2, uint id3, uint id4)
        {
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id, groupR4), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id2, groupR4 + 1), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id3, groupR4 + 2), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id4, groupR4 + 3), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities<TestEntityViewStruct>((entity, group, groupCount, db) =>
            {
                for (int i = 0; i < groupCount; i++)
                    entity[i].TestIt.value = entity[i].ID.entityID;
            });

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities<TestEntityStruct>((entity, group, groupCount, db) =>
            {
                for (int i = 0; i < groupCount; i++)
                    entity[i].value = entity[i].ID.entityID;
            });


            uint count;
            uint count2;
            for (uint i = 0; i < 4; i++)
            {
                var buffer1 = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityStruct>(groupR4 + i, out count);
                var buffer2 = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewStruct>(groupR4 + i, out count2);

                Assert.AreEqual(count, 1);
                Assert.AreEqual(count2, 1);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(buffer1[j].value, buffer1[j].ID.entityID);
                    Assert.AreEqual(buffer2[j].TestIt.value, buffer2[j].ID.entityID);
                }
            }

            _entityFunctions.RemoveEntity<TestDescriptor5>(new EGID(id, groupR4));
            _entityFunctions.RemoveEntity<TestDescriptor5>(new EGID(id2, groupR4 + 1));
            _entityFunctions.RemoveEntity<TestDescriptor5>(new EGID(id3, groupR4 + 2));
            _entityFunctions.RemoveEntity<TestDescriptor5>(new EGID(id4, groupR4 + 3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities<TestEntityViewStruct>((entity, group,  groupCount, db) =>
            {
                Assert.Fail();
            });

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities<TestEntityStruct>((entity, group,  groupCount, db) =>
            {
                Assert.Fail();
            });
        }

        [TestCase]
        public void QueryingNotExistingViewsInAnExistingGroupMustNotCrash()
        {
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(group0));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroupArray<TestEntityViewStruct>(group0));
        }

        [TestCase]
        public void TestExtendibleDescriptor()
        {
            _entityFactory.BuildEntity<B>(new EGID(1, group0), null);
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
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(1, group1)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(1, group0)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(1, group1)));
        }

        EnginesRoot                         _enginesRoot;
        IEntityFactory                      _entityFactory;
        IEntityFunctions                    _entityFunctions;
        SimpleSubmissionEntityViewScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                          _neverDoThisIsJustForTheTest;

        struct EVS1 : IEntityStruct
        {
            public EGID ID { get; set; }
        }

        struct EVS2 : IEntityStruct
        {
            public EGID ID { get; set; }
        }

        class A : GenericEntityDescriptor<EVS1>
        {
        }

        class B : ExtendibleEntityDescriptor<A>
        {
            static readonly IEntityBuilder[] _nodesToBuild;

            static B()
            {
                _nodesToBuild = new IEntityBuilder[]
                {
                    new EntityBuilder<EVS2>(),
                };
            }

            public B() : base(_nodesToBuild)
            {
            }
        }

        class A2 : GenericEntityDescriptor<TestEntityViewStruct>
        {
        }

        class B2 : ExtendibleEntityDescriptor<A2>
        {
            static readonly IEntityBuilder[] _nodesToBuild;

            static B2()
            {
                _nodesToBuild = new IEntityBuilder[]
                {
                    new EntityBuilder<TestEntityStruct>(),
                };
            }

            public B2() : base(_nodesToBuild)
            {
            }
        }

        class TestDescriptor : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor2 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor3 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor4 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor7 : GenericEntityDescriptor<TestEntityStruct>
        { }

        class TestDescriptor5 : GenericEntityDescriptor<TestEntityViewStruct, TestEntityStruct>
        { }

        struct TestEntityStruct : IEntityStruct
        {
            public uint value;

            public TestEntityStruct(uint value):this()
            {
                this.value = value;
            }

            public EGID ID { get; set; }
        }

        struct TestEntityViewStruct : IEntityViewStruct
        {
#pragma warning disable 649
            public ITestIt TestIt;
#pragma warning restore 649

            public EGID ID { get; set; }
        }

        interface ITestIt
        {
            float value { get; set; }
        }

        class TestIt : ITestIt
        {
            public TestIt(int i)
            {
                value = i;
            }

            public float value { get; set; }
        }

        class TestEngineAdd : IReactOnAddAndRemove<TestEntityViewStruct>
        {
            public TestEngineAdd(IEntityFactory entityFactory)
            {
                _entityFactory = entityFactory;
            }

            public void Add(ref TestEntityViewStruct entityView)
            {
                _entityFactory.BuildEntity<TestDescriptor7>(new EGID(100, group0), null);
            }

            public void Remove(ref TestEntityViewStruct entityView)
            {
                throw new NotImplementedException();
            }

            IEntityFactory _entityFactory;
        }

        class TestEngine: IQueryingEntitiesEngine
        {
            public IEntitiesDB entitiesDB { get; set; }
            public void Ready() {}

            public bool HasEntity<T>(EGID ID) where T : struct, IEntityStruct
            {
                return entitiesDB.Exists<T>(ID);
            }

            public bool HasAnyEntityInGroup<T>(ExclusiveGroup groupID) where T : struct, IEntityStruct
            {
                uint count;
                entitiesDB.QueryEntities<T>(groupID, out count);
                return count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(ExclusiveGroup groupID) where T: struct, IEntityStruct
            {
                uint count;
                entitiesDB.QueryEntities<T>(groupID, out count);

                return count != 0;
            }
        }
    }
}
