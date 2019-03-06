using System;
using System.Threading.Tasks;
using Svelto.ECS;

namespace ECSAllocationTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CheckAllocations().Wait();
        }

        private static async Task CheckAllocations()
        {
            var exclusiveGroup = new ExclusiveGroup();
            var enginesRoot = new EnginesRoot(new SimpleSubmissionEntityViewScheduler());
            var factory = enginesRoot.GenerateEntityFactory();
            int i = 0;

            while (true)
            {
                factory.BuildEntity<TestAllocation>(new EGID(i++, exclusiveGroup), null);

                await Task.Delay(100);
            }
        }
    }

    internal class TestAllocation:GenericEntityDescriptor<EntityStruct>
    {
    }

    internal struct EntityStruct : IEntityStruct
    {
        public EGID ID { get; set; }
    }
}
