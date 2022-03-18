using NUnit.Framework;
using Svelto.DataStructures;
using Assert = NUnit.Framework.Assert;

namespace Svelto.Common.Tests.Datastructures
{
    [TestFixture]
    class FasterDictionaryTests
    {
        [TestCase(Description = "Test adding an existing key throws")]
        public void TestUniqueKey()
        {
            FasterDictionary<int, string> test = new FasterDictionary<int, string>();

            test.Add(1, "one.a");
            void TestAddDup() { test.Add(1, "one.b"); }

            Assert.Throws(typeof(SveltoDictionaryException), TestAddDup);
        }

        [TestCase]
        public void TestIntersect()
        {
            FasterDictionary<int, int> test1 = new FasterDictionary<int, int>();
            FasterDictionary<int, int> test2 = new FasterDictionary<int, int>();

            for (int i = 0; i < 100; ++i)
                test1.Add(i, i);

            for (int i = 0; i < 200; i += 2)
                test2.Add(i, i);

            test1.Intersect(test2);

            for (int i = 0; i < test1.count; ++i)
                Console.LogDebug(test1.unsafeKeys[i].key.ToString());

            Assert.AreEqual(50, test1.count);

            for (int i = 0; i < 100; i += 2)
                Assert.IsTrue(test1.ContainsKey(i));
        }

        [TestCase]
        public void TestExclude()
        {
            FasterDictionary<int, int> test1 = new FasterDictionary<int, int>();
            FasterDictionary<int, int> test2 = new FasterDictionary<int, int>();

            for (int i = 0; i < 100; ++i)
                test1.Add(i, i);

            for (int i = 0; i < 200; i += 2)
                test2.Add(i, i);

            test1.Intersect(test2);

            for (int i = 0; i < test1.count; ++i)
                Console.LogDebug(test1.unsafeKeys[i].key.ToString());

            Assert.AreEqual(50, test1.count);

            for (int i = 1; i < 100; i += 2)
                Assert.IsTrue(test1.ContainsKey(i));
        }

        [TestCase]
        public void TestUnion()
        {
            FasterDictionary<int, int> test1 = new FasterDictionary<int, int>();
            FasterDictionary<int, int> test2 = new FasterDictionary<int, int>();

            for (int i = 0; i < 100; ++i)
                test1.Add(i, i);

            for (int i = 0; i < 200; i += 2)
                test2.Add(i, i);

            test1.Union(test2);

            for (int i = 0; i < test1.count; ++i)
                Console.LogDebug(test1.unsafeKeys[i].key.ToString());

            Assert.AreEqual(150, test1.count);

            for (int i = 0; i < 100; i++)
                Assert.IsTrue(test1.ContainsKey(i));

            for (int i = 100; i < 200; i += 2)
                Assert.IsTrue(test1.ContainsKey(i));
        }
    }
}