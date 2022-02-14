using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        public EntityFilterID CreateTransientFilter<T>() where T : IEntityComponent
        {
            var typeRef = TypeRefWrapper<T>.wrapper;
            var filter = new EntityFilterID(_nextEntityFilter++, typeRef);
            var filterCollection = new EntityFilterCollection(typeRef, this);

            _entityFilters[filter] = filterCollection;
            _transientFilters.Add(filterCollection);

            return filter;
        }

        public EntityFilterID CreatePersistentFilter<T>() where T : IEntityComponent
        {
            var typeRef = TypeRefWrapper<T>.wrapper;
            var filter = new EntityFilterID(_nextEntityFilter++, typeRef);
            var filterCollection = new EntityFilterCollection(typeRef, this);

            _entityFilters[filter] = filterCollection;
            _persistentFilters.Add(filterCollection);

            return filter;
        }

        void InitFilters()
        {
            _entityFilters = new FasterDictionary<EntityFilterID, EntityFilterCollection>();
            _transientFilters = new FasterList<EntityFilterCollection>();
            _persistentFilters = new FasterList<EntityFilterCollection>();
        }

        void DisposeFilters()
        {
            foreach (var filter in _transientFilters)
            {
                filter.Dispose();
            }

            foreach (var filter in _persistentFilters)
            {
                filter.Dispose();
            }
        }

        internal void ClearTransientFilters()
        {
            foreach (var filter in _transientFilters)
            {
                filter.Clear();
            }
        }

        void RemovePersistentFilters(EGID egid)
        {
            for (var i = 0; i < _persistentFilters.count; i++)
            {
                var refType = _persistentFilters[i].refType;
                if (_groupsPerEntity.ContainsKey(refType) && _groupsPerEntity[refType].ContainsKey(egid.groupID))
                {
                    _persistentFilters[i].UpdateOnRemove(egid, _groupsPerEntity[refType][egid.groupID]);
                }
            }
        }

        void SwapPersistentFilters(EGID from, EGID to)
        {
            for (var i = 0; i < _persistentFilters.count; i++)
            {
                var refType = _persistentFilters[i].refType;
                if (_groupsPerEntity.ContainsKey(refType) && _groupsPerEntity[refType].ContainsKey(from.groupID))
                {
                    _persistentFilters[i].UpdateOnSwap(from, to, _groupsPerEntity[refType][from.groupID], _groupsPerEntity[refType][to.groupID]);
                }
            }
        }

        internal FasterDictionary<EntityFilterID, EntityFilterCollection> _entityFilters;

        FasterList<EntityFilterCollection> _transientFilters;
        FasterList<EntityFilterCollection> _persistentFilters;
        uint                               _nextEntityFilter;
    }
}