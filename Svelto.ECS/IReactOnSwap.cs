using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public interface IReactOnSwap<T> : IReactOnSwap where T : IEntityStruct
    {
        void MovedTo(ref T entityView, ExclusiveGroup.ExclusiveGroupStruct previousGroup, EGID egid);
        void MovedFrom(ref T entityView, EGID egid);
    }
}