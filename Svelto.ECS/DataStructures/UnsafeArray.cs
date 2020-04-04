using System;
using System.Runtime.CompilerServices;
using Svelto.Common;

namespace Svelto.ECS.DataStructures
{
    public struct UnsafeArrayIndex
    {
        internal uint writerPointer;
        internal uint capacity;
    }

    /// <summary>
    /// Note: this must work inside burst, so it must follow burst restrictions
    /// </summary>
    unsafe struct UnsafeArray : IDisposable 
    {    
        /// <summary>
        /// </summary>
#if UNITY_ECS        
        [NativeDisableUnsafePtrRestriction]
#endif
        internal byte* ptr;

        /// <summary>
        /// </summary>
        uint writePointer, readPointer;

        /// <summary>
        /// </summary>
        internal uint capacity;

        internal uint size => writePointer - readPointer;

        internal uint space
        {
            get
            {
                if (capacity == 0)
                    return 0;
                
                return capacity - size;
            }
        }

        /// <summary>
        /// </summary>
        internal Allocator allocator;

        internal void Write<T>(in T item) where T : struct
        {
            var sizeOf = (uint)Unsafe.SizeOf<T>();
            
            //the idea is, considering the wrap, a read pointer must always be behind a writer pointer
            if (space - sizeOf < 0)
                throw new Exception("no writing authorized");
            
            Unsafe.Write(ptr + writePointer % capacity, item);

            writePointer += sizeOf;
        }
        
        internal T Read<T>() where T : struct
        {
            uint structSize = (uint) Unsafe.SizeOf<T>();
            
            if (size < structSize)
                throw new Exception("dequeuing empty queue or unexpected type dequeued");
            
            byte* addr    = ptr + readPointer % capacity;

            var data = Unsafe.Read<T>(addr);

            readPointer += structSize;
            
            if (readPointer > writePointer)
                throw new Exception("unexpected read");

            return data;
        }
        
        internal ref T Reserve<T>(out UnsafeArrayIndex index) where T : unmanaged
        {
            var head = writePointer % capacity;
            T* buffer = (T*) (ptr + head);
            
            if (writePointer >= capacity)
                throw new Exception("SimpleNativeArray: Reserve wrong index");

            index = new UnsafeArrayIndex()
            {
                capacity      = capacity
              , writerPointer = writePointer
            };

            writePointer += (uint)Unsafe.SizeOf<T>();
            
            return ref buffer[0];
        }
        
        internal ref T AccessReserve<T>(UnsafeArrayIndex index) where T : unmanaged
        {
            if (index.writerPointer >= capacity) new Exception($"SimpleNativeArray: out of bound access, index {index} capacity {capacity}");
            if (index.writerPointer < readPointer) new Exception($"SimpleNativeArray: out of bound access, index {index} count {readPointer % capacity}");

            T* buffer = (T*) (ptr + index.writerPointer);
            
            return ref buffer[0];
        }

        internal void Realloc(uint alignOf, uint newCapacity)
        {
            byte* newPointer = null;
            
            if (newCapacity <= capacity)
                throw new Exception("new capacity must be bigger than current");

            if (newCapacity > 0)
            {
                newPointer = (byte*) MemoryUtilities.Alloc(newCapacity, alignOf, allocator);
                if (size > 0)
                {
                    var readerHead = readPointer % capacity;
                    var writerHead = writePointer % capacity;

                    uint currentSize;
                    if (readerHead <= writerHead)
                    {
                        currentSize = writePointer - readPointer;
                        //copy to the new pointer, from th reader position

                        MemoryUtilities.MemCpy(newPointer, ptr + readPointer, currentSize);
                    }
                    else
                    {
                        currentSize = (capacity - readerHead) + writerHead;
                        //copy from  
                        MemoryUtilities.MemCpy(newPointer, ptr + readerHead, currentSize);
                        MemoryUtilities.MemCpy(newPointer + currentSize, ptr, writerHead);
                    }
                }
            }

            if (ptr != null)
                MemoryUtilities.Free((IntPtr) ptr);

            writePointer = size;
            readPointer = 0;
            
            ptr      = newPointer;
            capacity = newCapacity;
        }

        public void Dispose()
        {
            MemoryUtilities.Free((IntPtr) ptr);

            ptr      = null;
            writePointer   = 0;
            capacity = 0;
        }
        
        public void Clear()
        {
            writePointer = 0;
            readPointer = 0;
        }
    }
}
