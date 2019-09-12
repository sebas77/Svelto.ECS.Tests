using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class DefaultSerializer<T> : ISerializer<T> where T : unmanaged, IEntityStruct
    {
        public static readonly uint SIZEOFT = SerializableEntityBuilder<T>.SIZE;

        public uint size => SIZEOFT;

        static DefaultSerializer()
        {
            var _type = typeof(T);

            foreach (var field in _type.GetFields())
            {
                if (field.FieldType.ContainsCustomAttribute(typeof(DoNotSerializeAttribute)) && field.IsPrivate == false)
                    throw new ECSException("field cannot be serialised ".FastConcat(_type.FullName));
            }
            
            if (_type.GetProperties().Length > (EntityBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(_type.FullName));
        }

        public void Serialize(in T value, byte[] data, ref uint dataPos)
        {
            DefaultSerializerUtils.CopyToByteArray(value, data, dataPos);

            dataPos += SIZEOFT;
        }

        public void Deserialize(ref T value, byte[] data, ref uint dataPos)
        {
            value = DefaultSerializerUtils.CopyFromByteArray<T>(data, dataPos);

            dataPos += SIZEOFT;
        }
    }
}
