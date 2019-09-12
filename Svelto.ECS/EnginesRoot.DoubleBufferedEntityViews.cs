using System;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        internal class DoubleBufferedEntitiesToAdd
        {
            internal void Swap()
            {
                Swap(ref current, ref other);
                Swap(ref currentDense, ref otherDense);
            }

            static void Swap<T>(ref T item1, ref T item2)
            {
                var toSwap = item2;
                item2 = item1;
                item1 = toSwap;
            }

            internal void ClearOther()
            {
                foreach (var groups in otherDense)
                    foreach (var entitiesPerType in groups.Value)
                        entitiesPerType.Value.FastClear();

                other.FastClear();
                otherDense.FastClear();
            }

            FasterSparseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> current;
            FasterSparseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> other;
            
            FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> currentDense;
            FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> otherDense;

            readonly FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
              _entityViewsToAddBufferA = new FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            readonly FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>
              _entityViewsToAddBufferB = new FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>>();

            internal DoubleBufferedEntitiesToAdd()
            {
                currentDense = _entityViewsToAddBufferA;
                otherDense = _entityViewsToAddBufferB;
                
                current = currentDense.SparseSet();
                other = otherDense.SparseSet();
            }

            internal FasterSparseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> entitiesMapToSubmit =>
                current;
            internal FasterDenseList<FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary>> entitiesListToSubmit =>
                otherDense;
        }
    }
}