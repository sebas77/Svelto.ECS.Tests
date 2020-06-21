#if UNITY_2019_2_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Note unsorted jobs run in parallel
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    public abstract class JobifedEnginesGroup<Interface> : IJobifiedGroupEngine where Interface : class, IJobifiedEngine
    {
        protected JobifedEnginesGroup(FasterReadOnlyList<Interface> engines)
        {
            _engines         = engines;
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var fasterReadOnlyList = _engines;
            JobHandle combinedHandles = inputHandles;
            for (var index = 0; index < fasterReadOnlyList.count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(inputHandles));
            }

            return combinedHandles;
        }

        readonly FasterReadOnlyList<Interface> _engines;
        readonly bool                          _completeEachJob;
    }
    
    public abstract class JobifedEnginesGroup<Interface, Param>: IJobifiedGroupEngine<Param> where Interface : class, IJobifiedEngine<Param>
    {
        protected JobifedEnginesGroup(FasterReadOnlyList<Interface> engines)
        {
            _engines         = engines;
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Param _param)
        {
            var fasterReadOnlyList = _engines;
            for (var index = 0; index < fasterReadOnlyList.count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles, ref _param));
            }

            return combinedHandles;
        }

        readonly FasterReadOnlyList<Interface> _engines;
    }
}
#endif