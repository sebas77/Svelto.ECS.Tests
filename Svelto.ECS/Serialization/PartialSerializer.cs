using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Svelto.DataStructures;
using Svelto.ECS.Internal;
using Attribute = System.Attribute;

namespace Svelto.ECS.Serialization
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PartialSerializerFieldAttribute : Attribute
    {}

    public class PartialSerializer<T> : ISerializer<T>
        where T : unmanaged, IEntityStruct
    {
        static PartialSerializer()
        {
            Type myType = typeof(T);
            FieldInfo[] myMembers = myType.GetFields();

            for (int i = 0; i < myMembers.Length; i++)
            {
                Object[] myAttributes = myMembers[i].GetCustomAttributes(true);
                for (int j = 0; j < myAttributes.Length; j++)
                {
                    if (myAttributes[j] is PartialSerializerFieldAttribute)
                    {
                        if (myMembers[i].FieldType == typeof(EGID))
                            throw new ECSException("EGID fields cannot be serialised ".FastConcat(myType.FullName));

                        var offset = Marshal.OffsetOf<T>(myMembers[i].Name);
                        var sizeOf = (uint)Marshal.SizeOf(myMembers[i].FieldType);
                        offsets.Add(((uint) offset.ToInt32(), sizeOf));
                        totalSize += sizeOf;
                    }
                }
            }

            if (myType.GetProperties().Length > (EntityBuilder<T>.HAS_EGID ? 1 : 0))
                throw new ECSException("serializable entity struct must be property less ".FastConcat(myType.FullName));
        }

        public void Serialize(in T value, byte[] data, ref uint dataPos)
        {
            unsafe
            {
                fixed (byte* dataptr = data)
                {
                    var entityStruct = value;
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* srcPtr = (byte*) &entityStruct + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(srcPtr, dataptr + dataPos, data.Length - dataPos, offset.size);
                        dataPos += offset.size;
                    }
                }
            }
        }
        
        public void Deserialize(ref T value, byte[] data, ref uint dataPos)
        {
            unsafe
            {
                T tempValue = value; //todo: temporary solution I want to get rid of this copy
                fixed (byte* dataptr = data)
                    foreach ((uint offset, uint size) offset in offsets)
                    {
                        byte* dstPtr = (byte*) &tempValue + offset.offset;
                        //todo move to Unsafe Copy when available as it is faster
                        Buffer.MemoryCopy(dataptr + dataPos, dstPtr, offset.size, offset.size);
                        dataPos += offset.size;
                    }
                
                value = tempValue; //todo: temporary solution I want to get rid of this copy
            }
        }

        public uint size => totalSize;

        static readonly FasterList<(uint, uint)> offsets = new FasterList<(uint, uint)>();
        static readonly uint totalSize;
    }
}