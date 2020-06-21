#if UNITY_2019_1_OR_NEWER
using Svelto.DataStructures;
using Unity.Jobs;
using Svelto.Common;

namespace Svelto.ECS.Extensions.Unity
{
    /// <summary>
    /// Note sorted jobs run in serial
    /// </summary>
    /// <typeparam name="Interface"></typeparam>
    /// <typeparam name="SequenceOrder"></typeparam>
    public abstract class SortedJobifedEnginesGroup<Interface, SequenceOrder> : IJobifiedGroupEngine
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine
    {
        protected SortedJobifedEnginesGroup(FasterReadOnlyList<Interface> engines)
        {
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle inputHandles)
        {
            var fasterReadOnlyList = _instancedSequence.items;
            JobHandle combinedHandles = inputHandles;
            for (var index = 0; index < fasterReadOnlyList.count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles));
            }

            return combinedHandles; 
        }

        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    } 
    
    public abstract class SortedJobifedEnginesGroup<Interface, Parameter, SequenceOrder>: IJobifiedGroupEngine<Parameter>
        where SequenceOrder : struct, ISequenceOrder where Interface : class, IJobifiedEngine<Parameter>
    {
        protected SortedJobifedEnginesGroup(FasterReadOnlyList<Interface> engines)
        {
            _instancedSequence = new Sequence<Interface, SequenceOrder>(engines);
        }

        public JobHandle Execute(JobHandle combinedHandles, ref Parameter param)
        {
            var fasterReadOnlyList = _instancedSequence.items;
            for (var index = 0; index < fasterReadOnlyList.count; index++)
            {
                var engine = fasterReadOnlyList[index];
                combinedHandles = JobHandle.CombineDependencies(combinedHandles, engine.Execute(combinedHandles, ref param));
            }

            return combinedHandles;
        }

        readonly Sequence<Interface, SequenceOrder> _instancedSequence;
    }
}
#endif