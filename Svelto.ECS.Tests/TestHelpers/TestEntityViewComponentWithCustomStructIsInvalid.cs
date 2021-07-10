using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Tests
{
    struct TestEntityViewComponentWithCustomStructIsInvalid : IEntityViewComponent
    {
#pragma warning disable 649
        public TestCustomStructWithString TestCustomStructString;
#pragma warning restore 649
        
        public EGID ID { get; set; }
    }
}