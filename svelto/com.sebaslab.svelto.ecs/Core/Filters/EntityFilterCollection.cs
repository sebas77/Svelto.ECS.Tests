using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public struct EntityFilterCollection
    {
        internal EntityFilterCollection(RefWrapperType refType, EnginesRoot enginesRoot)
        {
            _enginesRoot = enginesRoot;
            _refType = refType;
            _filtersPerGroup = new FasterDictionary<ExclusiveGroupStruct, GroupFilters>();
        }

        public EntityFilterIterator iterator => new EntityFilterIterator(this);

        public void AddEntity(EGID egid)
        {
            // Get dictionary to find the index of the target entity.
            var dictionary = _enginesRoot._groupsPerEntity[_refType][egid.groupID];
            AddEntity(egid, dictionary);
        }

        public void RemoveEntity(EGID egid)
        {
            _filtersPerGroup[egid.groupID].Remove(egid.entityID);
        }

        public void Clear()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Clear();
            }
        }

        internal RefWrapperType refType => _refType;

        internal void UpdateOnRemove(EGID egid, ITypeSafeDictionary dictionary)
        {
            // If the way the submission code calls this function changes we want to be aware of it as it would break the code.
            DBC.ECS.Check.Require(dictionary.ContainsKey(egid.entityID)
                , "We expected the entity to still be part of the original dictionary, update the last index from next call from dictionary.count - 1 to dictionary.count");

            if (_filtersPerGroup.TryGetValue(egid.groupID, out var groupFilter))
            {
                // We must perform the swap back regardless of having the entity in the group filter or not.
                // Since this might affect the last entity as well.
                groupFilter.RemoveWithSwapBack(egid.entityID, dictionary.GetIndex(egid.entityID), (uint)dictionary.count - 1);
            }
        }

        internal void UpdateOnSwap(EGID from, EGID to, ITypeSafeDictionary fromDictionary, ITypeSafeDictionary toDictionary)
        {
            // If the way the submission code calls this function changes we want to be aware of it in order to fix the
            // index passed to the swap back and the index passed to the addition.
            DBC.ECS.Check.Require(fromDictionary.ContainsKey(from.entityID)
                , "We expected the entity to still be part of the original dictionary, update the last index from next call from dictionary.count - 1 to dictionary.count");
            DBC.ECS.Check.Require(toDictionary.ContainsKey(to.entityID)
                , "Entity must be added to the target dictionary before calling this method.");

            if (_filtersPerGroup.TryGetValue(from.groupID, out var groupFilter))
            {
                if (groupFilter.HasEntity(from.entityID))
                {
                    AddEntity(to, toDictionary);
                }

                // We must perform the swap back regardless of having the entity in the group filter or not.
                // Since this might affect the last entity as well.
                groupFilter.RemoveWithSwapBack(from.entityID, fromDictionary.GetIndex(from.entityID), (uint)fromDictionary.count - 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void AddEntity(EGID egid, ITypeSafeDictionary dictionary)
        {
            if (_filtersPerGroup.TryGetValue(egid.groupID, out var groupFilter) == false)
            {
                groupFilter = new GroupFilters(32, egid.groupID);
                _filtersPerGroup[egid.groupID] = groupFilter;
            }

            groupFilter.Add(egid.entityID, dictionary.GetIndex(egid.entityID));
        }

        internal int groupCount => _filtersPerGroup.count;

        internal GroupFilters GetGroup(int indexGroup)
        {
            DBC.ECS.Check.Require(indexGroup < _filtersPerGroup.count);
            return _filtersPerGroup.GetValues(out _)[indexGroup];
        }

        internal void Dispose()
        {
            var filterSets = _filtersPerGroup.GetValues(out var count);
            for (var i = 0; i < count; i++)
            {
                filterSets[i].Dispose();
            }
        }

        readonly EnginesRoot _enginesRoot;
        readonly RefWrapperType _refType;
        readonly FasterDictionary<ExclusiveGroupStruct, GroupFilters> _filtersPerGroup;

        internal struct GroupFilters
        {
            internal GroupFilters(uint size, ExclusiveGroupStruct group)
            {
                _entityIDToDenseIndex = new SharedSveltoDictionaryNative<uint, uint>(size);
                _indexToEntityId = new SharedSveltoDictionaryNative<uint, uint>(size);
                _group = group;
            }

            internal void Add(uint entityId, uint entityIndex)
            {
                _entityIDToDenseIndex.Add(entityId, entityIndex);
                _indexToEntityId.Add(entityIndex, entityId);
            }

            internal void Remove(uint entityId)
            {
                _indexToEntityId.Remove(_entityIDToDenseIndex[entityId]);
                _entityIDToDenseIndex.Remove(entityId);
            }

            public void RemoveWithSwapBack(uint entityId, uint entityIndex, uint lastIndex)
            {
                // Check if we need to do the swap back.
                if (entityIndex != lastIndex && _indexToEntityId.ContainsKey(lastIndex))
                {
                    var lastEntityID = _indexToEntityId[lastIndex];

                    _entityIDToDenseIndex[lastEntityID] = entityIndex;
                    _indexToEntityId[entityIndex] = lastEntityID;

                    _indexToEntityId.Remove(lastIndex);
                    // We don't need to check if the entityID and the entityIndex are part of the dictionary.
                    // The Remove function will check for us.
                    _entityIDToDenseIndex.Remove(entityId);
                }
                else
                {
                    // We don't need to check if the entityID and the entityIndex are part of the dictionary.
                    // The Remove function will check for us.
                    _entityIDToDenseIndex.Remove(entityId);
                    _indexToEntityId.Remove(entityIndex);
                }
            }

            internal void Clear()
            {
                _indexToEntityId.Clear();
                _entityIDToDenseIndex.FastClear();
            }

            internal bool HasEntity(uint entityId) => _entityIDToDenseIndex.ContainsKey(entityId);

            internal void Dispose()
            {
                _entityIDToDenseIndex.Dispose();
                _indexToEntityId.Dispose();
            }

            internal EntityFilterIndices indices
            {
                get
                {
                    var values = _entityIDToDenseIndex.GetValues(out var count);
                    return new EntityFilterIndices(values, count);
                }
            }

            internal uint count => (uint)_entityIDToDenseIndex.count;

            internal ExclusiveGroupStruct group => _group;

            SharedSveltoDictionaryNative<uint, uint> _indexToEntityId;
            SharedSveltoDictionaryNative<uint, uint> _entityIDToDenseIndex;
            ExclusiveGroupStruct                     _group;
        }
    }
}