using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS 
{
    [TestFixture]
    class SentinelTests: GenericTestsBaseClass
    {
       
        [TestCase(Description = "Test entities queries through sentinels cannot be used in read and write mode in 2 different threads")]
        public void TestReadAndWriteFromTheSameDatabase()
        {
           
        }

       
    }
}