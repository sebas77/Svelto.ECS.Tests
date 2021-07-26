using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Serialization;
using Svelto.Common;
using Svelto.ECS;
using Svelto.ECS.Serialization;

public static class DefaultSerializerUtils
{
    static readonly uint REFERENCE_SIZE = (uint)MemoryUtilities.SizeOf<EntityReference>();
    static readonly uint EGID_SIZE = (uint)MemoryUtilities.SizeOf<EGID>();

    public static unsafe void CopyToByteArray<T>(in T src, byte[] data, uint offsetDst) where T : unmanaged, IEntityComponent
    {
#if DEBUG && !PROFILE_SVELTO
        if (data.Length - offsetDst < sizeof(T))
        {
            throw new IndexOutOfRangeException(
                $"Data out of bounds when copying struct {typeof(T).GetType().Name}. data.Length: {data.Length}, offsetDst: {offsetDst}");
        }
#endif

        fixed (void* dstPtr = data)
        {
            void* dstOffsetPtr;
            if (IntPtr.Size == sizeof(int))
            {
                dstOffsetPtr = (void*) (((IntPtr) dstPtr).ToInt32() + ((IntPtr) offsetDst).ToInt32());
            }
            else
            {
                dstOffsetPtr = (void*) (((IntPtr) dstPtr).ToInt64() + ((IntPtr) offsetDst).ToInt64());
            }

            *(T*) dstOffsetPtr = src;
        }
    }

    internal static unsafe uint SerializeBlocksToByteArray<T>(in  T src, byte[] data, uint offsetDst,
        SerializationBlock[] blocks, EntityReferenceSerializer referenceSerializer)
        where T : unmanaged, IEntityComponent
    {
        uint bytesWritten = 0;
        fixed (byte* dataPtr = data)
        {
            byte* destPtr = dataPtr + offsetDst;
            fixed (T* srcPtr = &src)
            {
                foreach (var block in blocks)
                {
                    byte* srcPtrOffset = (byte*)srcPtr + block.offset;
                    byte* destPtrOffset = destPtr + bytesWritten;

                    if (block.type == SerializationType.EntityReference)
                    {
                        EGID egid = referenceSerializer.SerializeReference(*(EntityReference*) srcPtrOffset);
                        srcPtrOffset = (byte*)&egid;
                    }

                    MemoryUtilities.MemcpyUnaligned((IntPtr)srcPtrOffset, (IntPtr)destPtrOffset, block.size);

                    bytesWritten += block.size;
                }
            }
        }

        return bytesWritten;
    }

    public static unsafe T CopyFromByteArray<T>(byte[] data, uint offsetSrc) where T : unmanaged, IEntityComponent
    {
        T dst = default;

#if DEBUG && !PROFILE_SVELTO
        if (data.Length - offsetSrc < sizeof(T))
        {
            throw new IndexOutOfRangeException(
                $"Data out of bounds when copying struct {dst.GetType().Name}. data.Length: {data.Length}, offsetSrc: {offsetSrc}");
        }
#endif

        void* dstPtr = &dst;
        fixed (void* srcPtr = data)
        {
            void* srcOffsetPtr;
            if (IntPtr.Size == sizeof(int))
            {
                srcOffsetPtr = (void*) (((IntPtr) srcPtr).ToInt32() + ((IntPtr) offsetSrc).ToInt32());
            }
            else
            {
                srcOffsetPtr = (void*) (((IntPtr) srcPtr).ToInt64() + ((IntPtr) offsetSrc).ToInt64());
            }

            *(T*) dstPtr = *(T*) srcOffsetPtr;
        }

        return dst;
    }

    internal static unsafe uint DeserializeBlocksFromByteArray<T>(byte[] data, uint offsetSrc, ref T dest,
        SerializationBlock[] blocks, EntityReferenceSerializer referenceSerializer)
        where T : unmanaged, IEntityComponent
    {
        uint bytesRead = 0;
        fixed (byte* dataPtr = data)
        {
            byte* srcPtr = dataPtr + offsetSrc;
            fixed (T* destPtr = &dest)
            {
                foreach (var block in blocks)
                {
                    byte* srcPtrOffset = srcPtr + bytesRead;
                    byte* destPtrOffset = (byte*)destPtr + block.offset;
                    uint size = block.size;

                    if (block.type == SerializationType.EntityReference)
                    {
                        EntityReference reference = referenceSerializer.DeserializeReference(*(EGID*)srcPtrOffset);
                        srcPtrOffset = (byte*)&reference;
                        size = REFERENCE_SIZE;
                    }

                    MemoryUtilities.MemcpyUnaligned(new IntPtr(srcPtrOffset), new IntPtr(destPtrOffset), size);

                    bytesRead += size;
                }
            }
        }

        return bytesRead;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static SerializationBlock GetSerializationBlock(FieldInfo fieldInfo)
    {
        var fieldType = fieldInfo.FieldType;
        var block = new SerializationBlock
        {
            offset = MemoryUtilities.GetFieldOffset(fieldInfo)
        };

        if (fieldType == typeof(EntityReference))
        {
            block.type = SerializationType.EntityReference;
            block.size = EGID_SIZE;
        }
        else
        {
            block.type = SerializationType.ByteCopy;
            block.size = (uint)Marshal.SizeOf(fieldType);
        }

        return block;
    }
}