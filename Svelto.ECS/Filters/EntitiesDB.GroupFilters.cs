using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    ///     This feature must be eventually tied to the new ExclusiveGroup that won't allow the use of custom EntitiesID
    ///     The filters could be updated when entities buffer changes during the submission, while now this process
    ///     is completely manual.
    ///     Making this working properly is not in my priorities right now, as I will need to add the new group type
    ///     AND optimize the submission process to be able to add more overhead
    /// </summary>
    public partial class EntitiesDB
    {
        public readonly struct Filters
        {
            public Filters
            (FasterDictionary<RefWrapper<Type>, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> filters)
            {
                _filters    = filters;
            }

            public ref FilterGroup CreateOrGetFilterForGroup<T>(int filterID, ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
                var refWrapper = TypeRefWrapper<T>.wrapper;
                
                return ref CreateOrGetFilterForGroup(filterID, groupID, refWrapper);
            }

            ref FilterGroup CreateOrGetFilterForGroup(int filterID, ExclusiveGroupStruct groupID, RefWrapper<Type> refWrapper)
            {
                var fasterDictionary =
                    _filters.GetOrCreate(refWrapper, () => new FasterDictionary<ExclusiveGroupStruct, GroupFilters>());

                GroupFilters filters =
                    fasterDictionary.GetOrCreate(
                        groupID, () => new GroupFilters(new SharedSveltoDictionaryNative<int, FilterGroup>(0), groupID));

                return ref filters.CreateOrGetFilter(filterID, groupID);
            }

            public bool HasFiltersForGroup<T>(ExclusiveGroupStruct groupID) where T : struct, IEntityComponent
            {
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    return false;
                return _filters[TypeRefWrapper<T>.wrapper].ContainsKey(groupID);
            }

            public bool HasFiltersForGroup<T>(ExclusiveGroupStruct groupID, int filterID)
                where T : struct, IEntityComponent
            {
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    return false;

                if (_filters[TypeRefWrapper<T>.wrapper].TryGetValue(groupID, out var result))
                {
                    return result.HasFilter(filterID);
                }

                return false;
            }

            public ref GroupFilters GetFiltersForGroup<T>(ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)}");
                if (_filters[TypeRefWrapper<T>.wrapper].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif

                return ref _filters[TypeRefWrapper<T>.wrapper].GetValueByRef(groupID);
            }

            public ref FilterGroup GetFilterForGroup<T>(int filterId, ExclusiveGroupStruct groupID)
                where T : struct, IEntityComponent
            {
#if DEBUG && !PROFILE_SVELTO
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)}");
                if (_filters[TypeRefWrapper<T>.wrapper].ContainsKey(groupID) == false)
                    throw new ECSException(
                        $"trying to fetch not existing filters, type {typeof(T)} group {groupID.ToName()}");
#endif
                return ref _filters[TypeRefWrapper<T>.wrapper][groupID].GetFilter(filterId);
            }

            public bool TryGetFilter<T>(int filterId, ExclusiveGroupStruct groupID, out FilterGroup groupFilter)
                where T : struct, IEntityComponent
            {
                groupFilter = default;

                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    return false;

                if (_filters[TypeRefWrapper<T>.wrapper].TryGetValue(groupID, out var groupFilters) == false)
                    return false;

                if (groupFilters.TryGetFilter(filterId, out groupFilter) == false)
                    return false;

                return true;
            }

            public bool TryGetFiltersForGroup<T>(ExclusiveGroupStruct groupID, out GroupFilters groupFilters)
                where T : struct, IEntityComponent
            {
                groupFilters = default;

                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == false)
                    return false;

                return _filters[TypeRefWrapper<T>.wrapper].TryGetValue(groupID, out groupFilters);
            }

            public void ClearFilters<T>(int filterID, ExclusiveGroupStruct exclusiveGroupStruct)
            {
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == true)
                {
                    FasterDictionary<ExclusiveGroupStruct, GroupFilters> fasterDictionary =
                        _filters[TypeRefWrapper<T>.wrapper];

                    fasterDictionary[exclusiveGroupStruct].ClearFilter(filterID);
                }
            }

            public void ClearFilters<T>(int filterID)
            {
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == true)
                {
                    var fasterDictionary = _filters[TypeRefWrapper<T>.wrapper];

                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.Value.ClearFilter(filterID);
                }
            }

            public void ClearFilters<T>()
            {
                if (_filters.ContainsKey(TypeRefWrapper<T>.wrapper) == true)
                {
                    var fasterDictionary = _filters[TypeRefWrapper<T>.wrapper];

                    foreach (var filtersPerGroup in fasterDictionary)
                        filtersPerGroup.Value.ClearFilters();
                }
            }

            public bool TryRemoveEntity<T>(int filtersID, EGID egid) where T : unmanaged, IEntityComponent
            {
                if (TryGetFilter<T>(filtersID, egid.groupID, out var filter))
                {
                    return filter.TryRemove(egid.entityID);
                }

                return false;
            }

            public void RemoveEntity<T>(int filtersID, EGID egid) where T : unmanaged, IEntityComponent
            {
                ref var filter = ref GetFilterForGroup<T>(filtersID, egid.groupID);

                filter.Remove(egid.entityID);
            }

            public void AddEntity<N>(int filtersID, EGID egid, N mapper) where N:IEGIDMapper
            {
                ref var filter = ref CreateOrGetFilterForGroup(filtersID, egid.groupID, new RefWrapper<Type>(mapper.entityType));

                filter.Add(egid.entityID, mapper);
            }

            readonly FasterDictionary<RefWrapper<Type>, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> _filters;
        }

        public Filters GetFilters()
        {
            return new Filters(_filters);
        }

        FasterDictionary<RefWrapper<Type>, FasterDictionary<ExclusiveGroupStruct, GroupFilters>> _filters
            => _enginesRoot._groupFilters;
    }
}