using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public EntityFilterCollection GetFilter(EntityFilterID filterId)
        {
            return entityFilters[filterId];
        }

        public EntityFilterIterator FilterEntities(EntityFilterID filterId)
        {
            return entityFilters[filterId].iterator;
        }

        FasterDictionary<EntityFilterID, EntityFilterCollection> entityFilters => _enginesRoot._entityFilters;
    }
}