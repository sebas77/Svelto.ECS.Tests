using System;
using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public interface IEntitySerialization
    {
        /// <summary>
        /// Fill the data byte list with a byte[] of the entitiesToSerialize of this descriptor
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="serializedData"></param>
        /// <param name="serializationType"></param>
        /// <returns>Size in bytes of the newly instantiated entity</returns>
        void SerializeEntity(EGID egid, FasterList<byte> serializedData, in SerializationType serializationType);

        /// <summary>
        /// Deserialize a byte[] and copy directly onto the appropriate entities
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(in FasterReadOnlyList<byte> data, ref uint dataPos,
            in SerializationType serializationType);

        /// <summary>
        /// Deserialize a byte[] and copy directly onto the appropriate entities
        /// </summary>
        /// <param name="egid"></param>
        /// <param name="data"></param>
        /// <param name="dataPos"></param>
        /// <param name="serializationType"></param>
        void DeserializeEntity(EGID egid, in FasterReadOnlyList<byte> data, ref uint dataPos,
            in SerializationType serializationType);

        EntityStructInitializer DeserializeNewEntity(EGID egid, in FasterReadOnlyList<byte> data, ref uint dataPos,
            in SerializationType serializationType);

        void DeserializeEntityToSwap(EGID localEgid, EGID toEgid);

        void DeserializeEntityToDelete(EGID egid);

        void DeserializeEntityStructs(FasterReadOnlyList<byte> data, ref uint dataPos,
            SerializationType serializationType, ISerializableEntityDescriptor entityDescriptor, 
            in EntityStructInitializer initializer);

        void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
            where T : ISerializableEntityDescriptor, new();
    }
}
