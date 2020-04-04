using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.Common
{
#if !UNITY_ECS    
    public struct Allocator
    {
        
    }
#endif    
    
    public static class MemoryUtilities
    {
        public static void Free(IntPtr ptr)
        {
#if UNITY_ECS
                UnsafeUtility.Free(ptr, allocator);
#else
            Marshal.FreeHGlobal((IntPtr) ptr);
#endif
        }

        public static unsafe void MemCpy(void* newPointer, void* readerHead, uint currentSize)
        {
#if UNITY_ECS
            UnsafeUtility.MemCpy(newPointer, ptr + readerHead, currentSize);
#else
            Unsafe.CopyBlock(newPointer, readerHead, currentSize);
#endif
        }

        public static unsafe void* Alloc(uint newCapacity, uint alignOf, Allocator allocator)
        {
#if UNITY_ECS
            var newPointer = (void*) UnsafeUtility.Malloc(newCapacity, alignOf, allocator);
#else
            var newPointer = (void*) Marshal.AllocHGlobal((int) newCapacity);
#endif
            return newPointer;
        }

        public static unsafe void MemClear(void* listData, uint sizeOf)
        {
#if UNITY_ECS
            var newPointer = (void*) UnsafeUtility.Malloc(newCapacity, alignOf, allocator);
#else
            Unsafe.InitBlock(listData, 0, sizeOf);
#endif            
        }

        public static uint SizeOf<T>()
        {
#if UNITY_ECS            
            
#else
        return (uint) Unsafe.SizeOf<T>();
#endif
        }

        public static uint AlignOf<T>()
        {
            return 4;
        }
    }
}