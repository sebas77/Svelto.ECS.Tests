using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        /// <summary>
        /// todo: EnginesRoot was a weakreference to give the change to inject
        /// entityfunctions from other engines root. It probably should be reverted
        /// </summary>
        sealed class GenericEntityFunctions : IEntityFunctions
        {
            readonly EnginesRoot _weakReference;

            internal GenericEntityFunctions(EnginesRoot weakReference)
            {
                _weakReference = weakReference;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) where T :
                IEntityDescriptor, new()
            {
                RemoveEntity<T>(new EGID(entityID, groupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveEntity<T>(EGID entityEGID) where T : IEntityDescriptor, new()
            {
                _weakReference.CheckRemoveEntityID(entityEGID);

                _weakReference.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Remove, entityEGID, entityEGID,
                        EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void RemoveGroupAndEntities(ExclusiveGroup.ExclusiveGroupStruct groupID)
            {
                _weakReference.RemoveGroupID(groupID);

                _weakReference.QueueEntitySubmitOperation(
                    new EntitySubmitOperation(EntitySubmitOperationType.RemoveGroup, new EGID(0, groupID), new EGID()));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(uint entityID, ExclusiveGroup.ExclusiveGroupStruct fromGroupID,
                ExclusiveGroup.ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(new EGID(entityID, fromGroupID), toGroupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID)
                where T : IEntityDescriptor, new()
            {
                SwapEntityGroup<T>(fromID, new EGID(fromID.entityID, (uint) toGroupID));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, ExclusiveGroup.ExclusiveGroupStruct toGroupID
                , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(fromID, toGroupID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID
                , ExclusiveGroup.ExclusiveGroupStruct mustBeFromGroup)
                where T : IEntityDescriptor, new()
            {
                if (fromID.groupID != mustBeFromGroup)
                    throw new ECSException("Entity is not coming from the expected group");

                SwapEntityGroup<T>(fromID, toID);
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void SwapEntityGroup<T>(EGID fromID, EGID toID)
                where T : IEntityDescriptor, new()
            {
                _weakReference.CheckRemoveEntityID(fromID);
                _weakReference.CheckAddEntityID(toID);

                _weakReference.QueueEntitySubmitOperation<T>(
                    new EntitySubmitOperation(EntitySubmitOperationType.Swap,
                        fromID, toID, EntityDescriptorTemplate<T>.descriptor.entitiesToBuild));
            }

        }

        void QueueEntitySubmitOperation(EntitySubmitOperation entitySubmitOperation)
        {
#if DEBUG && !PROFILER
            entitySubmitOperation.trace = new StackFrame(1, true);
#endif
            _entitiesOperations.Add((ulong) entitySubmitOperation.fromID, ref entitySubmitOperation);
        }

        void QueueEntitySubmitOperation<T>(EntitySubmitOperation entitySubmitOperation) where T : IEntityDescriptor
        {
#if DEBUG && !PROFILER
            entitySubmitOperation.trace = new StackFrame(1, true);

            if (_entitiesOperations.TryGetValue((ulong) entitySubmitOperation.fromID, out var entitySubmitedOperation))
            {
                if (entitySubmitedOperation != entitySubmitOperation)
                    throw new ECSException("Only one entity operation per submission is allowed"
                        .FastConcat(" entityViewType: ")
                        .FastConcat(typeof(T).Name)
                        .FastConcat(" submission type ", entitySubmitOperation.type.ToString(),
                            " from ID: ", entitySubmitOperation.fromID.entityID.ToString())
                        .FastConcat(" previous operation type: ",
                            _entitiesOperations[(ulong) entitySubmitOperation.fromID].type
                                .ToString()));
            }
            else
#endif
                _entitiesOperations.Set((ulong) entitySubmitOperation.fromID, ref entitySubmitOperation);
        }
    }
}