using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EntitiesDB
    {
        public EntityFilterCollection GetPersistentFilter(int filterId)
        {
            return _enginesRoot._persistentEntityFilters[filterId];
        }

        public EntityFilterCollection GetTransientFilter(int filterId)
        {
            return _enginesRoot._transientEntityFilters[filterId];
        }
    }
}