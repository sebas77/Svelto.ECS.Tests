using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.DataStructures.Experimental;

namespace UnitTests
{
    //test the exception no implementors passed    
    
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
        
        struct Test
        {
            public  int i;

            public Test(int i) : this()
            {
                this.i = i;
            }
        }

        [TestMethod]
        public void TestFasterDictionary()
        {
            FasterDictionary<int, Test> test = new FasterDictionary<int, Test>();

            int dictionarysize = 10000;

            int[] numbers = new int[dictionarysize];

            for (int i = 1; i < dictionarysize; i++)
                numbers[i] = numbers[i - 1] + i * HashHelpers.ExpandPrime(dictionarysize);

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

            for (int i = dictionarysize - 1; i >= 0; i -= 3)
                if (test.Remove(i) == false)
                    throw new Exception();

            test.Trim();

            for (int i = dictionarysize - 1; i >= 0; i -= 3)
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

        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }

        [TestMethod]
        public void TestMegaEntitySwap()
        {
            for (int i = 0; i < 29; i++)
            {
                EGID egid = new EGID(i, 1);
                _entityFactory.BuildEntity<TestDescriptor>(egid, new[] { new TestIt(2) });
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            SwapMinNeededForException(_entityFunctions);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            bool allFound = true;
            bool mustNotBeFound = false;

            for (int i = 0; i < 29; i++)
                allFound &= _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(i, 2));

            for (int i = 0; i < 29; i++)
                mustNotBeFound |= _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(i, 1));

            
            Assert.IsTrue(allFound);
            Assert.IsTrue(mustNotBeFound == false);
        }

        void SwapMinNeededForException(IEntityFunctions entityFunctions)
        {
            entityFunctions.SwapEntityGroup<TestDescriptor>(18, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(19, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(20, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(21, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(22, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(17, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(16, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(15, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(14, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(13, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(11, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(9, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(6, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(5, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(4, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(3, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(2, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(0, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(24, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(25, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(26, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(27, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(28, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(23, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(8, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(7, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(1, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(12, 1, 2);
            entityFunctions.SwapEntityGroup<TestDescriptor>(10, 1, 2);
        }
        
        
        [TestMethod]
        public void TestMegaEntitySwap2()
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-5000, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4999, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4998, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4997, 1),  new[] {new TestIt(2)});
        
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4996, 2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4995, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4994, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4993, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4992, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4991, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4990, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4988, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4987, 2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4986, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4985, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4984, 2),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4980, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4977, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4976, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4974, 2),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4971, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4970, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4967, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4966, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4965, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4964, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4963, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4962, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4961, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4960, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4959, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4958, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4957, 1),  new[] {new TestIt(2)});

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4955, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4954, 1),  new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4953, 1),  new[] {new TestIt(2)});
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4996    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4995    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4991    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4984    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974    , 2    , 1);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-5000    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4999    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4998    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4997    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4994    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4993    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4990    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4985    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4976    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4967    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4965    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4964    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4963    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4962    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4961    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4960    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4959    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4958    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4957    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4955    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4954    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4953    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4984    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4991    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4995    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4996    , 1    , 6);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-5000    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4999    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4998    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4997    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4994    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4993    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4990    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4985    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4976    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4967    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4965    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4964    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4963    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4962    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4961    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4960    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4959    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4958    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4957    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4955    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4954    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4953    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4996    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4995    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4991    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4984    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977    , 6    , 7);         
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974    , 6    , 7);         
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            EGID egid = new EGID(-4958, 7);
            bool exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid);
            Assert.IsTrue(exists); 
        
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4961    , 7    , 8);         
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid); 
            Assert.IsTrue(exists);
        }

        [TestMethod]
        public void TestMegaEntitySwap3()
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-5000, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4999, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4998, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4997, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4996, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4995, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4994, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4993, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4992, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4991, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4990, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4989, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4988, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4987, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4986, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4985, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4984, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4983, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4982, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4981, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4980, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4979, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4978, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4977, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4976, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4975, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4974, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4973, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4972, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4971, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4970, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4969, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4968, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4967, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4966, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4965, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4964, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4963, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4962, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4961, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4960, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4959, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4958, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4957, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4956, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4955, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4954, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4953, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4952, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4951, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4950, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4949, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4948, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4947, 2), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4946, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4945, 1), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(-4944, 1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4987, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4982, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4975, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4968, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4965, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4964, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4960, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4959, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4947, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-5000, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4999, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4998, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4997, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4996, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4995, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4994, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4993, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4991, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4990, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4989, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4985, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4984, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4983, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4981, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4979, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4976, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4973, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4967, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4962, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4961, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4958, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4957, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4956, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4955, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4954, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4953, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4952, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4951, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4950, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4949, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4948, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4946, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4945, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4944, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4963, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4972, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-5000, 2, 0);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4998, 2, 0);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4973, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4947, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4959, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4960, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4964, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4965, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4968, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4975, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4982, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4987, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4999, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4997, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4996, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4995, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4994, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4993, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4991, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4990, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4989, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4985, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4984, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4981, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4979, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4976, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4967, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4962, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4961, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4958, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4957, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4956, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4955, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4954, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4953, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4952, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4951, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4950, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4949, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4948, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4946, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4945, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4944, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4969, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4973, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4972, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4963, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4987, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4986, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4982, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4977, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4975, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4968, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4965, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4964, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4960, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4959, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4947, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4983, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4972, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4969, 1, 2);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4963, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4973, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4966, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4978, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4969, 2, 1);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4972, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4983, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4969, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4978, 1, 6);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4970, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4972, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4980, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4983, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4974, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4978, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4969, 6, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988, 7, 8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4988, 8, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 7, 8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4971, 8, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992, 7, 8);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4992, 8, 7);
            _entityFunctions.SwapEntityGroup<TestDescriptor>(-4967, 7, 8);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            EGID egid   = new EGID(-4985, 7);
            bool exists = _neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(egid);
            
            Assert.IsTrue(exists);
        }

        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionTwoEntitiesCannotHaveTheSameIDInTheSameGroup(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroupInterleaved(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestExceptionTwoDifferentEntitiesCannotHaveTheSameIDInTheSameGroup(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(TypeSafeDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIdThrowsIntervaled(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [ExpectedException(typeof(FasterDictionaryException))]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDThrows(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, id), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestTwoEntitiesWithSameIDWorksOnDifferentGroups(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor2>(new EGID(id, id), new[] {new TestIt(2)});
            _entityFactory.BuildEntity<TestDescriptor3>(new EGID(id, id+1), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id+1)));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity<TestDescriptor>(new EGID(id, id));

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityWithImplementor(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor5>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));

            var entityView = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntityView
                <TestEntityView>(new EGID(id, id));
            Assert.AreEqual(entityView.TestIt.value, 2);

            uint index;
            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>
                                (new EGID(id, id), out index)[index].TestIt.value, 2);
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityViewStruct(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor4>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntitytruct(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor7>(new EGID(id, id), null);
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
                _entityFactory.BuildEntity<TestDescriptor6>(new EGID(id, id), null);
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
        public void TestBuildEntityStructWithInitializer(int id)
        {
            var init = _entityFactory.BuildEntity<TestDescriptor7>(new EGID(id, id), null);
            init.Init(new TestEntityStruct(3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityStruct>(id));
            uint index;
            Assert.IsTrue(
                _neverDoThisIsJustForTheTest.entitiesDB.
                    QueryEntitiesAndIndex<TestEntityStruct>(new EGID(id, id),
                    out index)[index].value == 3);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityMixed(int id)
        {
            TestIt testIt = new TestIt(2);
            _entityFactory.BuildEntity<TestDescriptor5>
                (new EGID(id, id), new[] { testIt });
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
            _entityFactory.BuildEntity<TestDescriptor4>(new EGID(id, id), 
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
            _entityFactory.BuildEntity(new EGID(id, id),
                                       EntityDescriptorTemplate<TestDescriptor>.descriptor,
                                       new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestBuildEntityInAddFunction(int id)
        {
            _enginesRoot.AddEngine(new TestEngineAdd(_entityFactory));
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityStruct>(new EGID(100, 0)));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntityFromGroup(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), 
                new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity<TestDescriptor>(id, id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>
                (new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup
                <TestEntityView>(id));
        }
        
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveEntityGroup(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveGroupAndEntities(id);

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityView>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityView>(id));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestRemoveAndAddAgainEntity(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.RemoveEntity<TestDescriptor>(id, id);

            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void TestSwapGroup(int id)
        {
            _entityFactory.BuildEntity<TestDescriptor>(new EGID(id, id), new[] {new TestIt(2)});
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _entityFunctions.SwapEntityGroup<TestDescriptor>(id, id, 3);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(3));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, 3)));

            uint index;
            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, 3), out index)[index].ID.entityID, id);
            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, 3), out index)[index].ID.groupID, 3);
            
            _entityFunctions.SwapEntityGroup<TestDescriptor>(id, 3, id);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, id)));
            Assert.IsTrue(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(id));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasAnyEntityInGroup<TestEntityViewStruct>(3));
            Assert.IsFalse(_neverDoThisIsJustForTheTest.HasEntity<TestEntityViewStruct>(new EGID(id, 3)));

            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, id), out index)[index].ID.entityID, id);
            Assert.AreEqual(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntitiesAndIndex<TestEntityViewStruct>(new EGID(id, id), out index)[index].ID.groupID, id);
        }

        [TestMethod]
        [DataRow(0, 1, 2, 3)]
        [DataRow(4, 5, 6, 7)]
        [DataRow(8, 9, 10, 11)]
        public void TestExecuteOnAllTheEntities(int id, int id2, int id3, int id4)
        {
            _entityFactory.BuildEntity<TestDescriptor8>(new EGID(id, 0), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor8>(new EGID(id2, 1), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor8>(new EGID(id3, 2), new[] { new TestIt(2) });
            _entityFactory.BuildEntity<TestDescriptor8>(new EGID(id4, 3), new[] { new TestIt(2) });
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities((ref TestEntityViewStruct entity, IEntitiesDB db)
                => entity.TestIt.value = entity.ID.entityID);

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities((ref TestEntityStruct entity, IEntitiesDB db)
                => entity.value = entity.ID.entityID);

            int count;
            int count2;
            for (int i = 0; i < 4; i++)
            {
                var buffer1 = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityStruct>(i, out count);
                var buffer2 = _neverDoThisIsJustForTheTest.entitiesDB.QueryEntities<TestEntityViewStruct>(i, out count2);

                Assert.AreEqual(count, 1);
                Assert.AreEqual(count2, 1);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(buffer1[j].value, buffer1[j].ID.entityID);
                    Assert.AreEqual(buffer2[j].TestIt.value, buffer2[j].ID.entityID);
                }
            }

            _entityFunctions.RemoveEntity<TestDescriptor8>(new EGID(id, 0));
            _entityFunctions.RemoveEntity<TestDescriptor8>(new EGID(id2, 1));
            _entityFunctions.RemoveEntity<TestDescriptor8>(new EGID(id3, 2));
            _entityFunctions.RemoveEntity<TestDescriptor8>(new EGID(id4, 3));
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities((ref TestEntityViewStruct entity, IEntitiesDB db)
                => Assert.Fail());

            _neverDoThisIsJustForTheTest.entitiesDB.ExecuteOnAllEntities((ref TestEntityStruct entity, IEntitiesDB db)
                => Assert.Fail());
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

        class TestDescriptor : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor2 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor3 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor4 : GenericEntityDescriptor<TestEntityViewStruct>
        {}
        
        class TestDescriptor5 : 
            GenericEntityDescriptor<TestEntityView, TestEntityViewStruct>
        {}
        
        class TestDescriptor6 : GenericEntityDescriptor<TestInvalidEntityStruct>
        {}

        class TestDescriptor7 : GenericEntityDescriptor<TestEntityStruct>
        { }

        class TestDescriptor8 : GenericEntityDescriptor<TestEntityViewStruct, TestEntityStruct>
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
            public int value;

            public TestEntityStruct(int value):this()
            {
                this.value = value;
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

        class TestEngineAdd : SingleEntityEngine<TestEntityViewStruct>
        {
            public TestEngineAdd(IEntityFactory entityFactory)
            {
                _entityFactory = entityFactory;
            }

            protected override void Add(ref TestEntityViewStruct entityView)
            {
                _entityFactory.BuildEntity<TestDescriptor7>(new EGID(100,0), null);
            }

            protected override void Remove(ref TestEntityViewStruct entityView)
            {
                throw new NotImplementedException();
            }

            IEntityFactory _entityFactory;
        }

        class TestEngine: IQueryingEntitiesEngine
        {
            public IEntitiesDB entitiesDB { get; set; }
            public void Ready() {}

            public bool HasEntity<T>(EGID ID) where T : IEntityStruct
            {
                return entitiesDB.Exists<T>(ID);
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
