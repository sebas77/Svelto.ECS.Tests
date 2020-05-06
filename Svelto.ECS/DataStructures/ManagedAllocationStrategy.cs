using System;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    class ManagedAllocationStrategy<T> : IAccess<T>
    {
        IBuffer<T> buffer;

        public void Alloc(uint size)
        {
            MB<T> b = default;
            b.Set(new T[size], size);
            buffer = b;
        }

        public IBuffer<T> values => buffer;

        public uint capacity => buffer.capacity;

        public void Resize(uint size)
        {
            DBC.ECS.Check.Require(size > 0, "Resize requires a size greater than 0");
            
            var realBuffer = buffer.ToManagedArray();
            Array.Resize(ref realBuffer, (int) size);
            MB<T> b = default;
            b.Set(realBuffer, size);
            buffer = b;
        }

        public void Dispose() {  }
    }
}