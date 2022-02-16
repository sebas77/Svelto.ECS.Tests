﻿using System.Runtime.CompilerServices;
using Svelto.DataStructures;
using Svelto.DataStructures.Native;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public readonly struct EntityFilterCollection
    {
        internal EntityFilterCollection(RefWrapperType refType, EnginesRoot enginesRoot)
        {
            _enginesRoot     = enginesRoot;
            _refType         = refType;
            _filtersPerGroup = new FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters>();
        }

        public EntityFilterIterator iterator => new EntityFilterIterator(this);

        public void AddEntity(EGID egid)
        {
            ITypeSafeDictionary dictionary = _enginesRoot._groupsPerEntity[_refType][egid.groupID];
            
            AddEntity(egid, dictionary.GetIndex(egid.entityID));
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

        internal void UpdateOnRemove(EGID egid, ITypeSafeDictionary dictionary)
        {
            // If the way the submission code calls this function changes we want to be aware of it as it would break the code.
            DBC.ECS.Check.Require(dictionary.ContainsKey(egid.entityID),
                "We expected the entity to still be part of the original dictionary, update the last index from next call from dictionary.count - 1 to dictionary.count");

            if (_filtersPerGroup.TryGetValue(egid.groupID, out var groupFilter))
            {
                // We must perform the swap back regardless of having the entity in the group filter or not.
                // Since this might affect the last entity as well.
                groupFilter.RemoveWithSwapBack(egid.entityID, dictionary.GetIndex(egid.entityID),
                    (uint)dictionary.count - 1);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void AddEntity(EGID egid, uint toIndex)
        {
            if (_filtersPerGroup.TryGetValue(egid.groupID, out var groupFilter) == false)
            {
                groupFilter                    = new LegacyGroupFilters(32, egid.groupID);
                _filtersPerGroup[egid.groupID] = groupFilter;
            }

            groupFilter.Add(egid.entityID, toIndex);
        }

        internal int groupCount => _filtersPerGroup.count;

        internal LegacyGroupFilters GetGroup(int indexGroup)
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

        readonly          EnginesRoot                                          _enginesRoot;
        readonly          RefWrapperType                                       _refType;
        internal readonly FasterDictionary<ExclusiveGroupStruct, LegacyGroupFilters> _filtersPerGroup;

        internal struct LegacyGroupFilters
        {
            internal LegacyGroupFilters(uint size, ExclusiveGroupStruct group)
            {
                _entityIDToDenseIndex = new SharedSveltoDictionaryNative<uint, uint>(size);
                _indexToEntityId      = new SharedSveltoDictionaryNative<uint, uint>(size);
                _group                = group;
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

            internal void RemoveWithSwapBack(uint entityId, uint entityIndex, uint lastIndex)
            {
                // Check if the last index is part of the filter as an entity, in that case
                //we need to update the filter
                if (entityIndex != lastIndex && _indexToEntityId.ContainsKey(lastIndex))
                {
                    uint lastEntityID = _indexToEntityId[lastIndex];

                    _entityIDToDenseIndex[lastEntityID] = entityIndex;
                    _indexToEntityId[entityIndex]       = lastEntityID;
                }

                // We don't need to check if the entityID and the entityIndex are part of the dictionary.
                // The Remove function will check for us.
                _entityIDToDenseIndex.Remove(entityId);
                _indexToEntityId.Remove(entityIndex);
            }

            internal void Clear()
            {
                _indexToEntityId.FastClear();
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
            readonly ExclusiveGroupStruct            _group;
        }
    }
}