using Svelto.ECS.Hybrid;

namespace Svelto.ECS.Tests
{
    public interface ITestFloatValue
    {
        float Value { get; set; }
    }

    public interface ITestIntValue
    {
        int Value { get; set; }
    }

    interface ITestStringValue
    {
        string Value { get; set; }
    }

    class TestFloatValue : ITestFloatValue
    {
        public TestFloatValue(float i) { Value = i; }

        public float Value { get; set; }
    }

    class TestIntValue : ITestIntValue
    {
        public TestIntValue(int i) { Value = i; }

        public int Value { get; set; }
    }

    class TestStringValue : ITestStringValue
    {
        public TestStringValue(string i) { Value = i; }
        
        public string Value { get; set; }
    }

    struct TestCustomStructWithString
    {
        public readonly string Value;
        
        public TestCustomStructWithString(string i)
        {
            Value = i;
        }
    }

    public struct TestEntityViewComponent : IEntityViewComponent
    {
#pragma warning disable 649
        public ITestFloatValue TestFloatValue;
        public ITestIntValue   TestIntValue;
#pragma warning restore 649

        public EGID ID { get; set; }
    }
}