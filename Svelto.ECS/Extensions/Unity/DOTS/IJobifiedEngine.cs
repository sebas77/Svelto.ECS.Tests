#if UNITY_2019_2_OR_NEWER
using Unity.Jobs;

namespace Svelto.ECS.Extensions.Unity
{
    public interface IJobifiedEngine : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle);
    }
    
    public interface IJobifiedGroupEngine : IJobifiedEngine
    { }
    
    public interface IJobifiedEngine<T> : IEngine
    {
        JobHandle Execute(JobHandle _jobHandle, ref T _param);
    }
    
    public interface IJobifiedGroupEngine<T> : IJobifiedEngine<T>
    {
    }
}
#endif