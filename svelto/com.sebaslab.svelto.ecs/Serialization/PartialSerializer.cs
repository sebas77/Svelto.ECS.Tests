using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Serialization;
using Svelto.Common;
using Svelto.DataStructures;
using Attribute = System.Attribute;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PartialSerializerFieldAttribute : Attribute
    {}

    public class PartialSerializer<T> : IComponentSerializer<T>, IEntityReferenceSerializer
        where T : unmanaged, IEntityComponent
    {
        static PartialSerializer()
        {
            Type type = typeof(T);
            FieldInfo[] fields = type.GetFields().OrderBy(MemoryUtilities.GetFieldOffset).ToArray();

            var blocks = new FasterList<SerializationBlock>();
            foreach (var field in fields)
            {
                var attributes = field.GetCustomAttributes(true);
                foreach (var attribute in attributes)
                {
                    if (attribute is PartialSerializerFieldAttribute)
                    {
                        var fieldType = field.FieldType;
                        if (fieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) &&
                            field.IsPrivate == false)
                            throw new ECSException($"field cannot be serialised {fieldType} in {type.FullName}");

                        var block = DefaultSerializerUtils.GetSerializationBlock(field);
                        _totalSize += block.size;
                        blocks.Add(block);
                    }
                }
            }

            if (type.GetProperties().Length > (ComponentBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(type.FullName));

            _blocks = blocks.ToArrayFast(out _);
        }

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            serializationData.dataPos +=
                DefaultSerializerUtils.SerializeBlocksToByteArray(value, serializationData.data.ToArrayFast(out _),
                    serializationData.dataPos, _blocks, referenceSerializer);
            return true;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            serializationData.dataPos +=
                DefaultSerializerUtils.DeserializeBlocksFromByteArray(serializationData.data.ToArrayFast(out _),
                    serializationData.dataPos, ref value, _blocks, referenceSerializer);

            return true;
        }

        public uint size => _totalSize;
        public EntityReferenceSerializer referenceSerializer { get; set; }

        static readonly SerializationBlock[] _blocks;
        static readonly uint _totalSize;
    }
}