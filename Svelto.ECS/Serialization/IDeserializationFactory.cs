using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public interface IDeserializationFactory
    {
        IEntitySerialization entitySerialization { set; }

        EntityStructInitializer Create(EGID egid, FasterReadOnlyList<byte> data, ref uint dataPos,
            SerializationType serializationType, ISerializableEntityDescriptor entityDescriptor);
    }
}
