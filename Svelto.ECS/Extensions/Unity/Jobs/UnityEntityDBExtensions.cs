#if UNITY_2019_2_OR_NEWER
using System;
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public static class UnityEntityDBExtensions
    {
        public static JobHandle CombineDispose
            <T1>(this T1 disposable, JobHandle combinedDependencies, JobHandle inputDeps) where T1 : struct, IDisposable
        {
            return JobHandle.CombineDependencies(combinedDependencies,
                                                 new DisposeJob<T1>(disposable).Schedule(inputDeps));
        }
        
        public static JobHandle CombineDispose
            <T1>(this T1 disposable, JobHandle inputDeps) where T1 : struct, IDisposable
        {
            return new DisposeJob<T1>(disposable).Schedule(inputDeps);
        }

        
        public static JobHandle CombineDispose
            <T1, T2>(this T1 disposable1, T2 disposable2, JobHandle combinedDependencies, JobHandle inputDeps) 
                where T1 : struct, IDisposable where T2 : struct, IDisposable
        {
            return JobHandle.CombineDependencies(combinedDependencies, new DisposeJob<T2>(disposable2).Schedule(inputDeps),
                                                 new DisposeJob<T1>(disposable1).Schedule(inputDeps));
        }
    }
}
#endif