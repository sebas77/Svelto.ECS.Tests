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
    }
}