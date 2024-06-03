using System.Collections.Generic;
using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class SpecialEnumeratorsTests : GenericTestsBaseClass
    {
        [Test(Description =
            "Test DoubleEntitiesEnumerator to iterate a group of entities against themselves with complexity n*(n+1)/2 (skips already tested couples)")]
        public void DoubleForEnumeratorWithOneComponentAndEmptyGroupsTest()
        {
            var dynamicEntities =
                new DoubleEntitiesEnumerator<TestEntityComponent>(
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(GroupAB));

            foreach (var none in dynamicEntities)
            {
                Assert.Fail();
            }
        }

        [Test]
        public void DoubleForEnumeratorWithOneComponentAndOneEmptyGroupTest()
        {
            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, Groups.GroupA, (int) i);

            _scheduler.SubmitEntities();

            List<int> results = new List<int>();

            for (uint i = 0; i < 10; i++)
            for (uint j = i + 1; j < 10; j++)
            {
                results.Add((int) ((i << 16) | j));
            }

            var dynamicEntities =
                new DoubleEntitiesEnumerator<TestEntityComponent>(
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(GroupAB));

            var iteration = 0;

            foreach (var ((bufferA, countA), indexA, groupA, (bufferB, countB), indexB, groupB) in dynamicEntities)
            {
                Assert.That(indexA < countA, "invalid outer index");
                Assert.That(indexB < countB, "invalid inner index");

                Assert.That(results[iteration]
                          , Is.EqualTo((bufferA[indexA].intValue << 16) | bufferB[indexB].intValue));
                Assert.That(results[iteration++], Is.EqualTo((indexA << 16) | indexB));

                Assert.That(groupA == Groups.GroupA);
            }

            Assert.That(results.Count == iteration);
        }

        [Test]
        public void DoubleForEnumeratorWithOneComponentTest()
        {
            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, Groups.GroupA, (int) i);

            for (uint i = 0; i < 5; i++)
                CreateTestEntity(i, Groups.GroupB, (int) i);

            _scheduler.SubmitEntities();

            List<int> results = new List<int>();

            //store the permutation of each group against all the groups without dups
            for (uint i = 0; i < 10; i++)
            {
                for (uint j = i + 1; j < 10; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }

                for (uint j = 0; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            for (uint i = 0; i < 5; i++)
            {
                for (uint j = i + 1; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            var dynamicEntities =
                new DoubleEntitiesEnumerator<TestEntityComponent>(
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(GroupAB));

            var iteration = 0;

            foreach (var ((bufferA, countA), indexA, (bufferB, countB), indexB) in dynamicEntities)
            {
                Assert.That(indexA < countA, "invalid outer index");
                Assert.That(indexB < countB, "invalid inner index");

                Console.Log($"{indexA} {indexB} {countA} {countB}");

                Assert.That(results[iteration]
                          , Is.EqualTo((bufferA[indexA].intValue << 16) | bufferB[indexB].intValue));
                Assert.That(results[iteration++], Is.EqualTo((indexA << 16) | indexB));
            }

            Assert.That(results.Count == iteration);
        }

        [Test]
        public void DoubleForEnumeratorWithTwoComponentTest()
        {
            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, Groups.GroupA, (int) i);

            for (uint i = 0; i < 5; i++)
                CreateTestEntity(i, Groups.GroupB, (int) i);

            _scheduler.SubmitEntities();

            List<int> results = new List<int>();

            //store the permutation of each group against all the groups without dups
            for (uint i = 0; i < 10; i++)
            {
                for (uint j = i + 1; j < 10; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }

                for (uint j = 0; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            for (uint i = 0; i < 5; i++)
            {
                for (uint j = i + 1; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            var dynamicEntities = new DoubleIterationEnumerator<TestEntityComponent, TestEntityViewComponent>(
                _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent, TestEntityViewComponent>(GroupAB));

            var iteration = 0;

            foreach (var ((bufferA, _, countA), indexA, (bufferB, _, countB), indexB) in dynamicEntities)
            {
                Assert.That(indexA < countA, "invalid outer index");
                Assert.That(indexB < countB, "invalid inner index");

                Assert.That(results[iteration]
                          , Is.EqualTo((bufferA[indexA].intValue << 16) | bufferB[indexB].intValue));
                Assert.That(results[iteration++], Is.EqualTo((indexA << 16) | indexB));
            }

            Assert.That(results.Count == iteration);
        }

        [Test]
        public void DoubleForEnumeratorWith3ComponentTest()
        {
            for (uint i = 0; i < 10; i++)
                CreateTestEntity(i, Groups.GroupA, (int) i);

            for (uint i = 0; i < 5; i++)
                CreateTestEntity(i, Groups.GroupB, (int) i);

            _scheduler.SubmitEntities();

            List<int> results = new List<int>();

            //store the permutation of each group against all the groups without dups
            for (uint i = 0; i < 10; i++)
            {
                for (uint j = i + 1; j < 10; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }

                for (uint j = 0; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            for (uint i = 0; i < 5; i++)
            {
                for (uint j = i + 1; j < 5; j++)
                {
                    results.Add((int) ((i << 16) | j));
                }
            }

            var dynamicEntities =
                new DoubleEntitiesEnumerator<TestEntityComponent, TestEntityComponentWithProperties,
                    TestEntityViewComponent>(_entitiesDB.entitiesForTesting
                                                               .QueryEntities<TestEntityComponent,
                                                                    TestEntityComponentWithProperties,
                                                                    TestEntityViewComponent>(
                                                                    GroupAB));

            var iteration = 0;

            foreach (var ((bufferA, _, _, countA), indexA, (bufferB, _, _, countB), indexB) in dynamicEntities)
            {
                Assert.That(indexA < countA, "invalid outer index");
                Assert.That(indexB < countB, "invalid inner index");

                Assert.That(results[iteration]
                          , Is.EqualTo((bufferA[indexA].intValue << 16) | bufferB[indexB].intValue));
                Assert.That(results[iteration++], Is.EqualTo((indexA << 16) | indexB));
            }

            Assert.That(results.Count == iteration);
        }
    }
}