using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Tests
{
    struct TestEntityViewComponentString : IEntityViewComponent
    {
#pragma warning disable 649
        public ITestStringValue TestStringValue;
#pragma warning restore 649
        
        public EGID ID { get; set; }
    }
}