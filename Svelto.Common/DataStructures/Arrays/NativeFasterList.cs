#if later
using System.Collections.Generic;

namespace Svelto.DataStructures
{
    public class 
ativeFasterList<T> : FasterList<T, NativeBuffer<T>> where T : unmanaged
    {
        internal new static readonly NativeFasterList<T> DefaultEmptyList = new NativeFasterList<T>();
        
        public NativeFasterList() { }

        public NativeFasterList(uint initialSize) : base(initialSize) { }

        public NativeFasterList(int initialSize) : base(initialSize) { }

        public NativeFasterList(T[] collection) : base(collection) { }

        public NativeFasterList(T[] collection, uint actualSize) : base(collection, actualSize) { }

        public NativeFasterList(ICollection<T> collection) : base(collection) { }

        public NativeFasterList(ICollection<T> collection, int extraSize) : base(collection, extraSize) { }

        public NativeFasterList(FasterList<T, NativeBuffer<T>> listCopy) : base(listCopy) { }

        public NativeFasterList(in FasterListStruct<T, NativeBuffer<T>> list) : base(list) { }
    }
}
#endif