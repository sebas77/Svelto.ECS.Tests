using System.Collections.Generic;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        /// <summary>
        /// todo: EnginesRoot was a weakreference to give the change to inject
        /// IEntityFactory from other engines root. It probably should be reverted
        /// </summary>
        class GenericEntityFactory : IEntityFactory
        {
            public GenericEntityFactory(EnginesRoot weakReference)
            {
                _enginesRoot = weakReference;
            }

            public EntityStructInitializer BuildEntity<T>(uint entityID,
                ExclusiveGroup.ExclusiveGroupStruct groupStructId, IEnumerable<object> implementors  = null) 
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.BuildEntity(new EGID(entityID, groupStructId), 
                    EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, IEnumerable<object> implementors = null) 
                where T : IEntityDescriptor, new()
            {
                return _enginesRoot.BuildEntity(egid,
                    EntityDescriptorTemplate<T>.descriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(EGID egid, T entityDescriptor, IEnumerable<object> implementors) 
                where T : IEntityDescriptor
            {
                return _enginesRoot.BuildEntity(egid, entityDescriptor.entitiesToBuild, implementors);
            }

            public EntityStructInitializer BuildEntity<T>(uint entityID,
                ExclusiveGroup.ExclusiveGroupStruct groupStructId, T descriptorEntity, IEnumerable<object> implementors) 
                where T : IEntityDescriptor
            {
                return _enginesRoot.BuildEntity(new EGID(entityID, groupStructId), descriptorEntity.entitiesToBuild,
                    implementors);
            }

            public void PreallocateEntitySpace<T>(ExclusiveGroup.ExclusiveGroupStruct groupStructId, uint size)
                where T : IEntityDescriptor, new()
            {
                _enginesRoot.Preallocate<T>(groupStructId, size);
            }

            readonly EnginesRoot _enginesRoot;
        }
    }
}