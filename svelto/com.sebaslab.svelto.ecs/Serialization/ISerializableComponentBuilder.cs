using Serialization;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableComponentBuilder : IComponentBuilder
    {
        void Serialize(uint id, ITypeSafeDictionary dictionary, EntityReferenceSerializer referenceSerializer
            , ISerializationData serializationData, int serializationType);

        void Deserialize(uint id, ITypeSafeDictionary dictionary, EntityReferenceSerializer referenceSerializer
            , ISerializationData serializationData, int serializationType);

        void Deserialize(ISerializationData serializationData, EntityReferenceSerializer referenceSerializer
            , in EntityInitializer initializer, int serializationType);

        uint Size(int serializationType);
    }
}