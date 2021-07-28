namespace Svelto.ECS.Serialization
{
    struct SerializationBlock
    {
        public SerializationType type;
        public int offset;
        public uint size;
    }

    enum SerializationType
    {
        ByteCopy,
        EntityReference,
    }
}