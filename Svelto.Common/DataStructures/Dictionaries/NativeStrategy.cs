using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DBC.Common;
using Svelto.Common;

namespace Svelto.DataStructures
{
    public struct NativeStrategy<T> : IBufferStrategy<T> where T : struct
    {
        Allocator _nativeAllocator;
        NB<T>     _realBuffer;
#if UNITY_COLLECTIONS
        [Unity.Collections.LowLevel.Unsafe.NativeDisableUnsafePtrRestriction]
#endif
        IntPtr _buffer;

#if DEBUG && !PROFILE_SVELTO            
        static NativeStrategy()
        {
            if (TypeCache<T>.IsUnmanaged == false)
                throw new PreconditionException("Only unmanaged data can be stored natively");
        }
#endif        

        public void Alloc(uint newCapacity, Allocator nativeAllocator)
        {
#if DEBUG && !PROFILE_SVELTO            
            if (!(this._realBuffer.ToNativeArray(out _) == IntPtr.Zero))
                throw new PreconditionException("can't alloc an already allocated buffer");
#endif            
            _nativeAllocator = nativeAllocator;

            var realBuffer =
                MemoryUtilities.Alloc((uint) (newCapacity * MemoryUtilities.SizeOf<T>()), _nativeAllocator);
            NB<T> b = new NB<T>(realBuffer, newCapacity);
            _buffer          = IntPtr.Zero;
            _realBuffer = b;
        }

        public bool isValid => _realBuffer.isValid;

        public NativeStrategy(uint size, Allocator nativeAllocator) : this()
        {
            Alloc(size, nativeAllocator);
        }

        public void Resize(uint newCapacity)
        {
#if DEBUG && !PROFILE_SVELTO            
            if (!(newCapacity > 0))
                throw new PreconditionException("Resize requires a size greater than 0");
            if (!(newCapacity > capacity))
                throw new PreconditionException("can't resize to a smaller size");
#endif            
            var pointer = _realBuffer.ToNativeArray(out _);
            var sizeOf  = MemoryUtilities.SizeOf<T>();
            pointer = MemoryUtilities.Realloc(pointer, (uint) (capacity * sizeOf), (uint) (newCapacity * sizeOf)
                                            , Allocator.Persistent);
            NB<T> b = new NB<T>(pointer, newCapacity);
            _buffer     = IntPtr.Zero;
            _realBuffer = b;
        }

        public void Clear()     => _realBuffer.Clear();

        public void FastClear() => _realBuffer.FastClear();

        public ref T this[uint index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref _realBuffer[index];
        }

        public IBuffer<T> ToBuffer()
        {
            if (_buffer == IntPtr.Zero)
                _buffer = GCHandle.ToIntPtr(GCHandle.Alloc(_realBuffer, GCHandleType.Normal));
            
            return (IBuffer<T>) GCHandle.FromIntPtr(_buffer).Target;
        }

        public NB<T> ToRealBuffer() { return _realBuffer; }

        public int       capacity           => _realBuffer.capacity;
        public Allocator allocationStrategy => _nativeAllocator;

        public void Dispose()
        {
            if (_realBuffer.ToNativeArray(out _) != IntPtr.Zero)
            {
                if (_buffer != IntPtr.Zero)
                    GCHandle.FromIntPtr(_buffer).Free();
                MemoryUtilities.Free(_realBuffer.ToNativeArray(out _), Allocator.Persistent);
            }
            else
                throw new Exception("trying to dispose disposed buffer");

            _realBuffer = default;
        }
    }
}