#if UNITY_5_3_OR_NEWER || UNITY_5
using UnityEngine;

namespace Svelto.ObjectPool
{
    public class MonoBehaviourPool<T> : ThreadSafeObjectPool<T> where T:MonoBehaviour
    {
#if POOL_DEBUGGER
    public MonoBehaviourPool()
    {
        GameObject poolDebugger = new GameObject("MonoBehaviourPoolDebugger");

        poolDebugger.AddComponent<PoolDebugger>().SetPool(this);
    }
#endif
        protected override void OnDispose()
        {
            for (var enumerator = _recycledPools.GetEnumerator(); enumerator.MoveNext();)
                foreach (var obj in enumerator.Current.Value)
                    GameObject.Destroy(obj);
        }
    }
}
#endif