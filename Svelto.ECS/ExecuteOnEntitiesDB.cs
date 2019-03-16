using System;

namespace Svelto.ECS.Internal
{
    partial class EntitiesDB
    {
        public void ExecuteOnEntity<T>(uint                                 id,
                                       ExclusiveGroup.ExclusiveGroupStruct groupid,
                                       EntityAction<T>                     action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (uint)groupid, action);
        }

        public void ExecuteOnEntity<T, W>(EGID entityGID, ref W value, EntityAction<T, W> action) where T: IEntityStruct
        {
            if (QueryEntitySafeDictionary(entityGID.groupID, out TypeSafeDictionary<T> casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, ref value, action))
                        return;
            }

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        public void ExecuteOnEntity<T>(EGID entityGID, EntityAction<T> action) where T : IEntityStruct
        {
            if (QueryEntitySafeDictionary(entityGID.groupID, out TypeSafeDictionary<T> casted))
            {
                if (casted != null)
                    if (casted.ExecuteOnEntityView(entityGID.entityID, action))
                        return;
            }

            throw new EntityNotFoundException(entityGID, typeof(T));
        }

        public void ExecuteOnEntity<T>(uint id, uint groupid, EntityAction<T> action) where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), action);
        }

        public void ExecuteOnEntity<T, W>(uint id, uint groupid, ref W value, EntityAction<T, W> action)
            where T : IEntityStruct
        {
            ExecuteOnEntity(new EGID(id, groupid), ref value, action);
        }

        public void ExecuteOnEntity<T, W>(uint                                 id,
                                          ExclusiveGroup.ExclusiveGroupStruct groupid,
                                          ref W                               value,
                                          EntityAction<T, W>                  action) where T : IEntityStruct
        {
            ExecuteOnEntity(id, (uint)groupid, ref value, action);
        }

        //----------------------------------------------------------------------------------------------------------

        public void ExecuteOnEntities<T>(uint groupID, EntitiesAction<T> action) where T : IEntityStruct
        {
            if (QueryEntitySafeDictionary(groupID, out TypeSafeDictionary<T> typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out var count);

            for (uint i = 0; i < count; i++)
                action(ref entities[i], new EntityActionData(this, i));
        }

        public void ExecuteOnEntities<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                         EntitiesAction<T>                   action) where T : IEntityStruct
        {
            ExecuteOnEntities((uint)groupStructId, action);
        }

        public void ExecuteOnEntities<T, W>(uint groupID, ref W value, EntitiesAction<T, W> action) where T:IEntityStruct
        {
            if (QueryEntitySafeDictionary(groupID, out TypeSafeDictionary<T> typeSafeDictionary) == false) return;

            var entities = typeSafeDictionary.GetValuesArray(out var count);

            for (uint i = 0; i < count; i++)
                action(ref entities[i], ref value, new EntityActionData(this, i));
        }

        public void ExecuteOnEntities<T, W>(ExclusiveGroup.ExclusiveGroupStruct groupStructId,
                                            ref W value, EntitiesAction<T, W> action) where T : IEntityStruct
        {
            ExecuteOnEntities((uint)groupStructId, ref value, action);
        }

        //-----------------------------------------------------------------------------------------------------------
        
        public void ExecuteOnAllEntities<T>(Action<T[], uint, IEntitiesDB> action) where T : IEntityStruct
        {
            var type = typeof(T);

            if (_groupedGroups.TryGetValue(type, out var dic))
            {
                var typeSafeDictionaries = dic.GetValuesArray(out var count);

                for (uint j = 0; j < count; j++)
                {
                    var entities = (typeSafeDictionaries[j] as TypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, innerCount, this);
                }
            }
        }

        public void ExecuteOnAllEntities<T, W>(ref W value, Action<T[], uint, IEntitiesDB, W> action) where T : IEntityStruct
        {
            var type = typeof(T);

            if (_groupedGroups.TryGetValue(type, out var dic))
            {
                var typeSafeDictionaries = dic.GetValuesArray(out var count);

                for (uint j = 0; j < count; j++)
                {
                    var entities = (typeSafeDictionaries[j] as TypeSafeDictionary<T>).GetValuesArray(out var innerCount);

                    if (innerCount > 0)
                        action(entities, innerCount, this, value);
                }
            }
        }

        public void ExecuteOnAllEntities<T>(ExclusiveGroup[] groups, EntitiesAction<T> action) where T : IEntityStruct
        {
            foreach (var group in groups)
            {
                ExecuteOnEntities(group, action);
            }
        }

        public void ExecuteOnAllEntities<T, W>(ExclusiveGroup[]     groups,
                                               ref W                value,
                                               EntitiesAction<T, W> action) where T : IEntityStruct
        {
            foreach (var group in groups)
            {
                ExecuteOnEntities(group, ref value, action);
            }
        }
    }
}