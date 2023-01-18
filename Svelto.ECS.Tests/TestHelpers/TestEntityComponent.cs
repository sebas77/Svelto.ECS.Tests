namespace Svelto.ECS.Tests
{
    public struct TestEntityComponent : IEntityComponent
#if SLOW_SVELTO_SUBMISSION            
          , INeedEGID
#endif            
    {
        public float floatValue;
        public int   intValue;

        public TestEntityComponent(float floatValue, int intValue) : this()
        {
            this.floatValue = floatValue;
            this.intValue   = intValue;
        }
#if SLOW_SVELTO_SUBMISSION   
        public EGID ID { get; set; }
#endif
    }
    
    public struct TestEntityComponentInt : IEntityComponent
    {
        public int   intValue;
    }
    
    struct TestEntityComponentWithProperties : IEntityComponent
    {
        public float floatValue { get; set; }
        public int   intValue   { get; set; }

        public TestEntityComponentWithProperties(float floatValue, int intValue) : this()
        {
            this.floatValue = floatValue;
            this.intValue   = intValue;
        }
    }
}