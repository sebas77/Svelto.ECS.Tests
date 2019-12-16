using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.ECS
{
    public struct UnsafeStructRef<T>: IDisposable
    {
        readonly IntPtr pointer;
        GCHandle _handle;

        public UnsafeStructRef(ref T entityStruct, GCHandle handle)
        {
            unsafe
            {
                _handle = handle;
                pointer = (IntPtr) Unsafe.AsPointer(ref entityStruct);
            }
        }

        public unsafe ref T refvalue => ref Unsafe.AsRef<T>((void*) pointer);

        public void Dispose()
        {
            _handle.Free();
        }
    }
}