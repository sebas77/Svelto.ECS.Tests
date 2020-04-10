using System;

namespace Svelto.Common
{
    public class TypeCache<T>
    {
        public static readonly Type type = typeof(T);
    }

    public class TypeHash<T>
    {
#if !ENABLE_BURST_AOT        
        public static readonly int hash = TypeCache<T>.type.GetHashCode();
#else
        public static readonly int hash = Unity.Burst.BurstRuntime.GetHashCode32<T>();
#endif        
    }
}