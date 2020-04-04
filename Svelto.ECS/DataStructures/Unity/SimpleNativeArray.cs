#if UNITY_ECS
using System;
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Svelto.ECS.DataStructures.Unity
{
    public struct SimpleNativeArray : IDisposable
    {
        [NativeDisableUnsafePtrRestriction] unsafe UnsafeArray* _list;
#if DEBUG && !PROFILE_SVELTO
        int hashType;
#endif

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint Count<T>() where T:unmanaged
        {
            unsafe
            {
                return (uint) (_list->size / sizeof(T));
            }
        }

        public static SimpleNativeArray Alloc<T>(Allocator allocator, uint newLength = 0) where T : unmanaged
        {
            unsafe
            {
                var rtnStruc = new SimpleNativeArray();
#if DEBUG && !PROFILE_SVELTO
                rtnStruc.hashType = typeof(T).GetHashCode();
#endif
                var sizeOf  = UnsafeUtility.SizeOf<T>();
                var alignOf = UnsafeUtility.AlignOf<T>();

                UnsafeArray* listData =
                    (UnsafeArray*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<UnsafeArray>()
                                                     , UnsafeUtility.AlignOf<UnsafeArray>(), allocator);
                UnsafeUtility.MemClear(listData, UnsafeUtility.SizeOf<UnsafeArray>());

                listData->allocator = allocator;

                if (newLength > 0)
                    listData->Realloc(alignOf, (uint) (newLength * sizeOf));

                UnsafeUtility.MemClear(listData->ptr, listData->capacity * sizeOf);

                rtnStruc._list = listData;

                return rtnStruc;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Get<T>(uint index) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (hashType != typeof(T).GetHashCode())
                    throw new Exception("SimpleNativeArray: not except type used");
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
                if (index >= Count<T>())
                    throw new Exception($"SimpleNativeArray: out of bound access, index {index} count {Count<T>()}");
#endif
                T* buffer = (T*) _list->ptr;
                return ref buffer[index];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(uint index, in T value) where T : unmanaged => Get<T>(index) = value;

        public unsafe void Dispose()
        {
            if (_list != null)
            {
                _list->Dispose();
                _list = null;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(in T item) where T : unmanaged
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (hashType != typeof(T).GetHashCode())
                    throw new Exception("SimpleNativeArray: not except type used");
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                if (_list->space == 0)
                    _list->Realloc(UnsafeUtility.AlignOf<T>(), (uint) ((_list->capacity + 1) * 1.5f));

                _list->Write(item);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            unsafe
            {
#if DEBUG && !PROFILE_SVELTO
                if (_list == null)
                    throw new Exception("SimpleNativeArray: null-access");
#endif
                _list->Clear();
            }
        }
    }
}
#endif