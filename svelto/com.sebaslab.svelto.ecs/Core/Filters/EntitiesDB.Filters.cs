using System;
using System.Threading;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.DataStructures;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public SveltoFilters GetFilters()
        {
            return new SveltoFilters(_enginesRoot);
        }
        

        /// <summary>
        /// this whole structure is usable inside DOTS JOBS and BURST
        /// </summary>
        public readonly struct SveltoFilters
        {
            public struct CombinedFilterID
            {
                public readonly long id;

                public CombinedFilterID(int filterID, ContextID contextID)
                {
                    id = (long)filterID << 32 | (uint)contextID.id << 16;
                }

                public static implicit operator CombinedFilterID((int filterID, ContextID contextID) data)
                {
                    return new CombinedFilterID(data.filterID, data.contextID);
                }
            }

            public struct ContextID
            {
                public readonly uint id;

                internal ContextID(uint id)
                {
                    DBC.ECS.Check.Require(id < ushort.MaxValue, "too many types registered, HOW :)");

                    this.id = id;
                }
            }

            public static ContextID GetNewContextID()
            {
                return new ContextID((uint)Interlocked.Increment(ref uniqueContextID.Data));
            }

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _persistentEntityFilters;

            readonly SharedSveltoDictionaryNative<NativeRefWrapperType, NativeDynamicArrayCast<int>>
                _indicesOfPersistentFiltersUsedByThisComponent;

            readonly SharedSveltoDictionaryNative<long, EntityFilterCollection> _transientEntityFilters;

            public SveltoFilters(EnginesRoot enginesRoot)
            {
                _persistentEntityFilters = enginesRoot._persistentEntityFilters;
                _indicesOfPersistentFiltersUsedByThisComponent =
                    enginesRoot._indicesOfPersistentFiltersUsedByThisComponent;
                _transientEntityFilters = enginesRoot._transientEntityFilters;
            }

            static readonly SharedStatic<int, EntitiesDB> uniqueContextID = new SharedStatic<int, EntitiesDB>(1);
#if UNITY_BURST
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID,
                NativeRefWrapperType typeRef) where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = EnginesRoot.CombineFilterIDs<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                var filterCollection = EntityFilterCollection.Create();

                _persistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = _persistentEntityFilters.count - 1;

                if (_indicesOfPersistentFiltersUsedByThisComponent.TryFindIndex(typeRef, out var getIndex) == false)
                {
                    var newArray = new NativeDynamicArrayCast<int>(1, Allocator.Persistent);
                    newArray.Add(lastIndex);
                    _indicesOfPersistentFiltersUsedByThisComponent.Add(typeRef, newArray);
                }
                else
                {
                    ref var array = ref _indicesOfPersistentFiltersUsedByThisComponent.GetDirectValueByRef(getIndex);
                    array.Add(lastIndex);
                }

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }
#endif

            /// <summary>
            /// Create a persistent filter. Persistent filters are not deleted after each submission,
            /// however they have a maintenance cost that must be taken into account and will affect
            /// entities submission performance.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreatePersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = EnginesRoot.CombineFilterIDs<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                var typeRef          = TypeRefWrapper<T>.wrapper;
                var filterCollection = EntityFilterCollection.Create();

                _persistentEntityFilters.Add(combineFilterIDs, filterCollection);

                var lastIndex = _persistentEntityFilters.count - 1;

                _indicesOfPersistentFiltersUsedByThisComponent.GetOrAdd(new NativeRefWrapperType(typeRef),
                    () => new NativeDynamicArrayCast<int>(1, Svelto.Common.Allocator.Persistent)).Add(lastIndex);

                return ref _persistentEntityFilters.GetDirectValueByRef((uint)lastIndex);
            }

            public ref EntityFilterCollection GetPersistentFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                long combineFilterIDs = EnginesRoot.CombineFilterIDs<T>(filterID);

                if (_persistentEntityFilters.TryFindIndex(combineFilterIDs, out var index) == true)
                    return ref _persistentEntityFilters.GetDirectValueByRef(index);

                throw new Exception("filter not found");
            }

            /// <summary>
            /// Creates a transient filter. Transient filters are deleted after each submission
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <returns></returns>
            public ref EntityFilterCollection GetOrCreateTransientFilter<T>(CombinedFilterID filterID)
                where T : unmanaged, IEntityComponent
            {
                var combineFilterIDs = EnginesRoot.CombineFilterIDs<T>(filterID);

                if (_transientEntityFilters.TryFindIndex(combineFilterIDs, out var index))
                    return ref _transientEntityFilters.GetDirectValueByRef(index);

                var filterCollection = EntityFilterCollection.Create();

                _transientEntityFilters.Add(combineFilterIDs, filterCollection);

                return ref _transientEntityFilters.GetDirectValueByRef((uint)(_transientEntityFilters.count - 1));
            }
        }
    }
}