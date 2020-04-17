using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Svelto.Utilities
{
    public static class FastInvoke<T> where T:struct
    {
        public static int GetFieldOffset(RuntimeFieldHandle h) => 
            Marshal.ReadInt32(h.Value + (4 + IntPtr.Size)) & 0xFFFFFF;
        
        public static FastInvokeActionCast<T> MakeSetter(FieldInfo field)
        {
            if (field.FieldType.IsInterfaceEx() == true && field.FieldType.IsValueTypeEx() == false)
            {
                int offset = GetFieldOffset(field.FieldHandle);
                return (ref T target, object o) =>
                {
                    unsafe
                    {
                        ref var pointer = ref Unsafe.AddByteOffset(ref target, (IntPtr) offset);
                        Unsafe.Write(Unsafe.AsPointer(ref pointer), o);
                    }
                };
            }

            throw new ArgumentException("<color=teal>Svelto.ECS</color> unsupported field (must be an interface and a class)");
        }
    }
    
    public delegate void FastInvokeActionCast<T>(ref T target, object value);
}
