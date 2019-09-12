using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityBuilder : IEntityBuilder
    {
        void Serialize(uint id, ITypeSafeDictionary dictionary, FasterList<byte> data,
            in SerializationType serializationType);
        
        void Deserialize(uint id, ITypeSafeDictionary dictionary, in FasterReadOnlyList<byte> data, ref uint dataPos,
            in SerializationType serializationType);

        void Deserialize(in FasterReadOnlyList<byte> data, ref uint dataPos, in SerializationType serializationType,
            in EntityStructInitializer initializer);

        void Set(ref EntityStructInitializer initializer);
    }
}
