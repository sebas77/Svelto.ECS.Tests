using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    public struct EGIDMapper<T> where T : struct, IEntityStruct
    {
        internal FasterDictionaryStruct<uint, T> map;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILER
                if (map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
                map.TryFindIndex(entityID, out var findIndex);
#endif
                return ref map.unsafeValues[(int) findIndex];
        }
        
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                value = map.GetDirectValue(index);
                return true;
            }

            value = default;
            return false;
        }
    }
    
    public struct NativeEGIDMapper<T> where T : unmanaged, IEntityStruct
    {
        internal FasterDictionaryStruct<uint, T> map;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T Entity(uint entityID)
        {
#if DEBUG && !PROFILER
                if (map.TryFindIndex(entityID, out var findIndex) == false)
                    throw new Exception("Entity not found in this group ".FastConcat(typeof(T).ToString()));
#else
            map.TryFindIndex(entityID, out var findIndex);
#endif
            return ref map.unsafeValues[(int) findIndex];
        }
        
        public bool TryGetEntity(uint entityID, out T value)
        {
            if (map.TryFindIndex(entityID, out var index))
            {
                value = map.GetDirectValue(index);
                return true;
            }

            value = default;
            return false;
        }
    }
}

