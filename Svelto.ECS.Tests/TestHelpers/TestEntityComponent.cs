namespace Svelto.ECS.Tests
{
    public struct TestEntityComponent : IEntityComponent, INeedEGID
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
    
    public struct TestEntityComponentInt : IEntityComponent, INeedEGID
    {
        public int   intValue;

        public EGID ID { get; set; }
    }
    
    struct TestEntityComponentWithProperties : IEntityComponent, INeedEGID
    {
        public float floatValue { get; set; }
        public int   intValue   { get; set; }

        public TestEntityComponentWithProperties(float floatValue, int intValue) : this()
        {
            this.floatValue = floatValue;
            this.intValue   = intValue;
        }

        public EGID ID { get; set; }
    }
}