namespace Svelto.ECS.Tests
{
    struct TestEntityComponent : IEntityComponent, INeedEGID
    {
        public float floatValue;
        public int   intValue;

        public TestEntityComponent(float floatValue, int intValue) : this()
        {
            this.floatValue = floatValue;
            this.intValue   = intValue;
        }

        public EGID ID { get; set; }
    }
    
    struct TestEntityComponentWithProperties : IEntityComponent, INeedEGID
    {
        public float floatValue;
        public int   intValue;

        public TestEntityComponentWithProperties(float floatValue, int intValue) : this()
        {
            this.floatValue = floatValue;
            this.intValue   = intValue;
        }

        public EGID ID { get; set; }
    }
}