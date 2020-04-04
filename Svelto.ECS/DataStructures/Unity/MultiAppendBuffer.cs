#if UNITY_ECS
using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace Svelto.ECS.DataStructures.Unity
{
    /// <summary>
    /// A collection of <see cref="SimpleNativeBag"/> intended to allow one buffer per thread.
    /// from: https://github.com/jeffvella/UnityEcsEvents/blob/develop/Runtime/MultiAppendBuffer.cs
    /// </summary>
    public unsafe struct MultiAppendBuffer
    {
        public const int DefaultThreadIndex = -1;
        public const int MaxThreadIndex = JobsUtility.MaxJobThreadCount - 1;
        public const int MinThreadIndex = DefaultThreadIndex;

        [NativeDisableUnsafePtrRestriction] SimpleNativeBag* _data;
        public readonly Allocator Allocator;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInvalidThreadIndex(int index) => index < MinThreadIndex || index > MaxThreadIndex;

        public MultiAppendBuffer(Allocator allocator)
        {
            Allocator = allocator;

            var bufferSize = UnsafeUtility.SizeOf<SimpleNativeBag>();
            var bufferCount = JobsUtility.MaxJobThreadCount + 1;
            var allocationSize = bufferSize * bufferCount;

            var ptr = (byte*)UnsafeUtility.Malloc(allocationSize, UnsafeUtility.AlignOf<int>(), allocator);
            UnsafeUtility.MemClear(ptr, allocationSize);

            for (int i = 0; i < bufferCount; i++)
            {
                var bufferPtr = (SimpleNativeBag*)(ptr + bufferSize * i);
                var buffer = new SimpleNativeBag(allocator);
                UnsafeUtility.CopyStructureToPtr(ref buffer, bufferPtr);
            }

            _data = (SimpleNativeBag*)ptr;
        }

        /// <summary>
        /// Retrieve buffer for a specific thread index.
        /// </summary>
        /// <param name="threadIndex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref SimpleNativeBag GetBuffer(int threadIndex)
        {
            // All indexes are offset by +1; Unspecified ThreadIndex 
            // (main thread without explicitly checking for ThreadId) 
            // should use first index by providing threadIndex of -1;

            return ref UnsafeUtilityEx.ArrayElementAsRef<SimpleNativeBag>(_data, threadIndex + 1);
        }

        public uint Count()
        {
            return JobsUtility.MaxJobThreadCount + 1;
        }

        public void Dispose()
        {
            for (int i = -1; i < JobsUtility.MaxJobThreadCount; i++)
            {
                GetBuffer(i).Dispose();
            }
            UnsafeUtility.Free(_data, Allocator);
        }

        public void Clear()
        {
            for (int i = -1; i < JobsUtility.MaxJobThreadCount; i++)
            {
                GetBuffer(i).Clear();
            }
        }
    }
}
#endif