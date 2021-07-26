using System.Linq;
using System.Runtime.InteropServices;
using Serialization;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public class DefaultSerializer<T> : IComponentSerializer<T>, IEntityReferenceSerializer
        where T : unmanaged, IEntityComponent
    {
        static DefaultSerializer()
        {
            var _type = typeof(T);
            var fields = _type.GetFields().OrderBy(MemoryUtilities.GetFieldOffset).ToArray();

            var blocks = new FasterList<SerializationBlock>();

            foreach (var field in fields)
            {
                var fieldType = field.FieldType;
                if (fieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) &&
                    field.IsPrivate == false)
                    throw new ECSException($"field cannot be serialised {fieldType} in {_type.FullName}");

                var block = DefaultSerializerUtils.GetSerializationBlock(field);

                _totalSize += block.size;
                blocks.Add(block);
            }

            _blocks = blocks.ToArrayFast(out _);

            if (_type.GetProperties().Length > (ComponentBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(_type.FullName));
        }

        public uint size => _totalSize;

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            serializationData.dataPos += DefaultSerializerUtils.SerializeBlocksToByteArray(value,
                serializationData.data.ToArrayFast(out _), serializationData.dataPos, _blocks, referenceSerializer);
            return true;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            serializationData.dataPos +=
                DefaultSerializerUtils.DeserializeBlocksFromByteArray(serializationData.data.ToArrayFast(out _),
                    serializationData.dataPos, ref value, _blocks, referenceSerializer);

            return true;
        }

        public EntityReferenceSerializer referenceSerializer { get; set; }

        static readonly uint _totalSize = 0;
        static readonly SerializationBlock[] _blocks;
    }
}