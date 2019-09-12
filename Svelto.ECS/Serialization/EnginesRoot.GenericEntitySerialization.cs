using System;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    public enum SerializationType
    {
        Network,
        Storage,

        Length
    }

    public partial class EnginesRoot
    {
        sealed class EntitySerialization : IEntitySerialization
        {
            public void SerializeEntity(EGID egid, FasterList<byte> serializedData,
                in SerializationType serializationType)
            {
                var entitiesDb = _enginesRoot._entitiesDB;

                //needs to retrieve the meta data associated with the entity
                ref var serializableEntityStruct = ref entitiesDb.QueryEntity<SerializableEntityStruct>(egid);
                uint descriptorHash = serializableEntityStruct.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);
                var entityStructsToSerialise = entityDescriptor.entitiesToSerialize;

                var header = new SerializableEntityHeader(descriptorHash, egid, (byte) entityStructsToSerialise.Count);
                header.Copy(serializedData);

                for (int index = 0; index < entityStructsToSerialise.Count; index++)
                {
                    var entityBuilder = entityStructsToSerialise[index];

                    SerializeEntityStruct(egid, entityBuilder, serializedData, serializationType);
                }
            }

            public EntityStructInitializer DeserializeNewEntity(EGID egid, in FasterReadOnlyList<byte> data,
                ref uint dataPos, in SerializationType serializationType)
            {  //todo: SerializableEntityHeader may be needed to be customizable
                var serializableEntityHeader = new SerializableEntityHeader(data, ref dataPos);

                uint descriptorHash = serializableEntityHeader.descriptorHash;
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var factory = serializationDescriptorMap.GetSerializationFactory(descriptorHash);
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                if (factory == null)
                {
                    var initializer = _enginesRoot.BuildEntity(egid, entityDescriptor.entitiesToBuild);

                    DeserializeEntityStructs(data, ref dataPos, serializationType, entityDescriptor, initializer);

                    entityDescriptor.FillInitializer(ref initializer);

                    return initializer;
                }

                return factory.Create(egid, data, ref dataPos, serializationType, entityDescriptor);
            }

            public void DeserializeEntity(in FasterReadOnlyList<byte> data, ref uint dataPos,
                in SerializationType serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(data, ref dataPos);

                EGID egid = serializableEntityHeader.egid;

                DeserializeEntityInternal(data, ref dataPos, serializationType, egid, serializableEntityHeader);
            }

            public void DeserializeEntity(EGID egid, in FasterReadOnlyList<byte> data, ref uint dataPos,
                in SerializationType serializationType)
            {
                var serializableEntityHeader = new SerializableEntityHeader(data, ref dataPos);

                DeserializeEntityInternal(data, ref dataPos, serializationType, egid, serializableEntityHeader);
            }

            public void DeserializeEntityStructs(FasterReadOnlyList<byte> data, ref uint dataPos,
                SerializationType serializationType, ISerializableEntityDescriptor entityDescriptor,
                in EntityStructInitializer initializer)
            {
                for (int index = 0; index < entityDescriptor.entitiesToSerialize.Count; ++index)
                {
                    var serializableEntityBuilder = entityDescriptor.entitiesToSerialize[index];

                    serializableEntityBuilder.Deserialize(data, ref dataPos, serializationType, initializer);
                }
            }

            public void DeserializeEntityToSwap(EGID localEgid, EGID toEgid)
            {
                EntitiesDB entitiesDb = _enginesRoot._entitiesDB;
                ref var serializableEntityStruct = ref entitiesDb.QueryEntity<SerializableEntityStruct>(localEgid);

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                uint descriptorHash = serializableEntityStruct.descriptorHash;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                var entitySubmitOperation = new EntitySubmitOperation(
                    EntitySubmitOperationType.Swap,
                    localEgid,
                    toEgid,
                    entityDescriptor.entitiesToBuild);

                _enginesRoot.CheckRemoveEntityID(localEgid);
                _enginesRoot.CheckAddEntityID(toEgid);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
            }

            public void DeserializeEntityToDelete(EGID egid)
            {
                EntitiesDB entitiesDB = _enginesRoot._entitiesDB;
                ref var serializableEntityStruct = ref entitiesDB.QueryEntity<SerializableEntityStruct>(egid);
                uint descriptorHash = serializableEntityStruct.descriptorHash;

                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = serializationDescriptorMap.GetDescriptorFromHash(descriptorHash);

                _enginesRoot.CheckRemoveEntityID(egid);

                var entitySubmitOperation = new EntitySubmitOperation(
                    EntitySubmitOperationType.Remove,
                    egid,
                    egid,
                    entityDescriptor.entitiesToBuild);

                _enginesRoot.QueueEntitySubmitOperation(entitySubmitOperation);
            }

            public void RegisterSerializationFactory<T>(IDeserializationFactory deserializationFactory)
                where T : ISerializableEntityDescriptor, new()
            {
                SerializationDescriptorMap serializationDescriptorMap = _enginesRoot.serializationDescriptorMap;
                serializationDescriptorMap.RegisterSerializationFactory<T>(deserializationFactory);
                deserializationFactory.entitySerialization = this;
            }

            internal EntitySerialization(EnginesRoot enginesRoot)
            {
                _enginesRoot = enginesRoot;
            }

            void SerializeEntityStruct(EGID entityGID, ISerializableEntityBuilder entityBuilder, FasterList<byte> data,
                in SerializationType serializationType)
            {
                uint groupId = entityGID.groupID;
                Type entityType = entityBuilder.GetEntityType();
                if (!_enginesRoot._entitiesDB.UnsafeQueryEntityDictionary(groupId, entityType, out var safeDictionary))
                {
                    throw new Exception("Entity Serialization failed");
                }

                entityBuilder.Serialize(entityGID.entityID, safeDictionary, data, serializationType);
            }

            void DeserializeEntityInternal(FasterReadOnlyList<byte> data, ref uint dataPos,
                SerializationType serializationType, EGID egid, SerializableEntityHeader serializableEntityHeader)
            {
                SerializationDescriptorMap descriptorMap = _enginesRoot.serializationDescriptorMap;
                var entityDescriptor = descriptorMap.GetDescriptorFromHash(serializableEntityHeader.descriptorHash);

                foreach (var serializableEntityBuilder in entityDescriptor.entitiesToSerialize)
                {
                    _enginesRoot._entitiesDB.UnsafeQueryEntityDictionary(egid.groupID,
                        serializableEntityBuilder.GetEntityType(), out var safeDictionary);

                    serializableEntityBuilder.Deserialize(egid.entityID, safeDictionary, data, ref dataPos,
                        serializationType);
                }
            }

            readonly EnginesRoot _enginesRoot;
        }

        public IEntitySerialization GenerateEntitySerializer()
        {
            return new EntitySerialization(this);
        }
    }
}