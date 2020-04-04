#if UNITY_ECS
using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.DataStructures.Unity;
using Unity.Collections;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        NativeEntityOperations ProvideNativeEntityOperationsQueue<T>
            (Allocator allocator) where T : IEntityDescriptor, new()
        {
            NativeQueue<EGID> egidsToRemove = new NativeQueue<EGID>(allocator);

            _nativeRemoveOperations.Add(new NativeOperationRemove(egidsToRemove
                                                                , EntityDescriptorTemplate<T>
                                                                 .descriptor.entityComponentsToBuild));
            NativeQueue<DoubleEGID> egidsToSwap = new NativeQueue<DoubleEGID>(allocator);
            _nativeSwapOperations.Add(
                new NativeOperationSwap(egidsToSwap, EntityDescriptorTemplate<T>.descriptor.entityComponentsToBuild));

            return new NativeEntityOperations(egidsToRemove, egidsToSwap);
        }

        NativeEntityFactory ProvideNativeEntityFactoryQueue<T>(Allocator allocator) where T : IEntityDescriptor, new()
        {
            MultiAppendBuffer addOperationQueue = new MultiAppendBuffer(allocator);

            _nativeAddOperations.Add(new NativeOperationBuild(addOperationQueue, EntityDescriptorTemplate<T>.descriptor.entityComponentsToBuild));

            return new NativeEntityFactory(addOperationQueue);
        }
        
        void NativeOperationSubmission(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Remove Operations"))
            {
                for (int i = 0; i < _nativeRemoveOperations.count; i++)
                {
                    var simpleNativeArray = _nativeRemoveOperations[i].queue;

                    while (simpleNativeArray.TryDequeue(out EGID entityEGID))
                    {
                        CheckRemoveEntityID(entityEGID);
                        QueueEntitySubmitOperation(new EntitySubmitOperation(
                                                       EntitySubmitOperationType.Remove, entityEGID, entityEGID
                                                     , _nativeRemoveOperations[i].entityComponents));
                    }
                }
            }
            using (profiler.Sample("Native Add Operations"))
            {
                for (int i = 0; i < _nativeAddOperations.count; i++)
                {
                    var simpleNativeArray = _nativeAddOperations[i].addOperationQueue;

                    for (int j = 0; j < simpleNativeArray.Count(); j++)
                    {
                        var buffer = simpleNativeArray.GetBuffer(j);

                        while (buffer.IsEmpty() == false)
                        {
                            var egid = buffer.Dequeue<EGID>();
                            var componentCounts = buffer.Dequeue<uint>();
                            EntityComponentInitializer init = BuildEntity(egid, _nativeAddOperations[i].components);
                            
                            DBC.ECS.Check.Assert(componentCounts == 3, "mo" + componentCounts);
                            
                            while (componentCounts > 0)
                            {
                                componentCounts--;
                                
                                var typeID = buffer.Dequeue<uint>();

                                IFiller entityBuilder = EntityComponentIDMap.GetTypeFromID(typeID);
                               
                                //after the typeID, I expect the serialized component
                                entityBuilder.FillFromByteArray(init, buffer);
                            }
                        }
                    }
                }
            }
        }

        void AllocateNativeOperations()
        {
            _nativeRemoveOperations = new FasterList<NativeOperationRemove>();
            _nativeSwapOperations   = new FasterList<NativeOperationSwap>();
            _nativeAddOperations    = new FasterList<NativeOperationBuild>();
        }

        void DisposeNativeOperations(in PlatformProfiler profiler)
        {
            using (profiler.Sample("Native Dispose Operations"))
            {
                for (int i = 0; i < _nativeRemoveOperations.count; i++)
                    _nativeRemoveOperations[i].queue.Dispose();

                for (int i = 0; i < _nativeSwapOperations.count; i++)
                    _nativeSwapOperations[i].queue.Dispose();

                for (int i = 0; i < _nativeAddOperations.count; i++)
                    _nativeAddOperations[i].addOperationQueue.Dispose();

                _nativeRemoveOperations.FastClear();
                _nativeSwapOperations.FastClear();
                _nativeAddOperations.FastClear();
            }
        }

        FasterList<NativeOperationRemove> _nativeRemoveOperations;
        FasterList<NativeOperationSwap>   _nativeSwapOperations;
        FasterList<NativeOperationBuild> _nativeAddOperations;
    }

    readonly struct DoubleEGID
    {
        internal readonly EGID from;
        internal readonly EGID to;

        public DoubleEGID(EGID from1, EGID to1)
        {
            from = from1;
            to   = to1;
        }
    }

    public struct NativeEntityOperations
    {
        NativeQueue<EGID>.ParallelWriter       EGIDsToRemove;
        NativeQueue<DoubleEGID>.ParallelWriter EGIDsToSwap;

        internal NativeEntityOperations(NativeQueue<EGID> EGIDsToRemove, NativeQueue<DoubleEGID> EGIDsToSwap)
        {
            this.EGIDsToRemove = EGIDsToRemove.AsParallelWriter();
            this.EGIDsToSwap   = EGIDsToSwap.AsParallelWriter();
        }

        public void RemoveEntity(EGID egid)        { EGIDsToRemove.Enqueue(egid); }
        public void SwapEntity(EGID from, EGID to) { EGIDsToSwap.Enqueue(new DoubleEGID(from, to)); }
    }

    public readonly struct NativeEntityFactory
    {
        readonly MultiAppendBuffer _addOperationQueue;
        
        public NativeEntityFactory(MultiAppendBuffer addOperationQueue)
        {
            _addOperationQueue = addOperationQueue;
        }

        public NativeEntityComponentInitializer BuildEntity(uint eindex, ExclusiveGroupStruct buildGroup, int threadIndex)
        {
            SimpleNativeBag unsafeBuffer = _addOperationQueue.GetBuffer(threadIndex);
            
            unsafeBuffer.Enqueue(new EGID(eindex, buildGroup));
            unsafeBuffer.ReserveEnqueue<uint>(out var index) = 0;
            
            return new NativeEntityComponentInitializer(unsafeBuffer, index);
        }
    }
    
    public readonly ref struct NativeEntityComponentInitializer
    {
        readonly SimpleNativeBag _unsafeBuffer;
        readonly UnsafeArrayIndex _index;

        public NativeEntityComponentInitializer(in SimpleNativeBag unsafeBuffer, UnsafeArrayIndex index)
        {
            _unsafeBuffer = unsafeBuffer;
            _index = index;
        }

        public void Init<T>(in T component) where T : unmanaged, IEntityComponent
        {
            uint id = EntityComponentIDMap.GetIDFromType<T>();

            _unsafeBuffer.AccessReserved<uint>(_index)++;
            
            _unsafeBuffer.Enqueue(id);
            _unsafeBuffer.Enqueue(component);
        }
    }

    struct NativeOperationBuild
    {
        internal MultiAppendBuffer addOperationQueue;
        internal readonly IEntityBuilder[] components;

        public NativeOperationBuild
            (in MultiAppendBuffer addOperationQueue, IEntityBuilder[] descriptorEntityComponentsToBuild)
        {
            this.addOperationQueue = addOperationQueue;
            components = descriptorEntityComponentsToBuild;
        }
    }

    readonly struct NativeOperationRemove
    {
        internal readonly IEntityBuilder[]  entityComponents;
        internal readonly NativeQueue<EGID> queue;

        public NativeOperationRemove(in NativeQueue<EGID> simpleQueue, IEntityBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
            queue            = simpleQueue;
        }
    }

    readonly struct NativeOperationSwap
    {
        internal readonly IEntityBuilder[]        entityComponents;
        internal readonly NativeQueue<DoubleEGID> queue;

        public NativeOperationSwap(in NativeQueue<DoubleEGID> simpleQueue, IEntityBuilder[] descriptorEntitiesToBuild)
        {
            entityComponents = descriptorEntitiesToBuild;
            queue            = simpleQueue;
        }
    }
}
#endif