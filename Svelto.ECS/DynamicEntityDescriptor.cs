using System;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    /// <summary>
    /// DynamicEntityDescriptor can be used to add entity views to an existing EntityDescriptor that act as flags,
    /// at building time.
    /// This method allocates, so it shouldn't be abused
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public struct DynamicEntityDescriptor<TType> : IEntityDescriptor where TType : IEntityDescriptor, new()
    {
        internal DynamicEntityDescriptor(bool isExtendible) : this()
        {
            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length = defaultEntities.Length;
            
            _entitiesToBuild = new IEntityBuilder[length + 1];

            Array.Copy(defaultEntities, 0, _entitiesToBuild, 0, length);

            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            _entitiesToBuild[length] = new EntityBuilder<EntityStructInfoView>
            (
                new EntityStructInfoView
                {
                    entitiesToBuild = _entitiesToBuild
                }
            );
        }
        
        public DynamicEntityDescriptor(IEntityBuilder[] extraEntities) : this()
        {
            var extraEntitiesLength = extraEntities.Length;

            Setup(extraEntitiesLength, extraEntities);
        }

        public DynamicEntityDescriptor(FasterList<IEntityBuilder> extraEntitiesList) : this()
        {
            var extraEntities = extraEntitiesList.ToArrayFast();

            var extraEntitiesLength = extraEntitiesList.Count;

            Setup(extraEntitiesLength, extraEntities);
        }

        void Setup(int extraEntitiesLength, IEntityBuilder[] extraEntities)
        {
            if (extraEntitiesLength == 0)
            {
                _entitiesToBuild = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
                return;
            }

            var defaultEntities = EntityDescriptorTemplate<TType>.descriptor.entitiesToBuild;
            var length = defaultEntities.Length;

            var index = SetupSpecialEntityStruct(defaultEntities, out _entitiesToBuild, extraEntitiesLength);

            Array.Copy(extraEntities, 0, _entitiesToBuild, length, extraEntitiesLength);
            
            //assign it after otherwise the previous copy will overwrite the value in case the item
            //is already present
            _entitiesToBuild[index] = new EntityBuilder<EntityStructInfoView>
            (
                new EntityStructInfoView
                {
                    entitiesToBuild = _entitiesToBuild
                }
            );
        }

        static int SetupSpecialEntityStruct(IEntityBuilder[] defaultEntities, out IEntityBuilder[] entitiesToBuild,
            int extraLenght)
        {
            int length = defaultEntities.Length;
            int index = -1;

            for (var i = 0; i < length; i++)
            {
                if (defaultEntities[i].GetEntityType() == EntityBuilderUtilities.ENTITY_STRUCT_INFO_VIEW)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                index = length + extraLenght;
                entitiesToBuild = new IEntityBuilder[index + 1];
            }
            else
            {
                entitiesToBuild = new IEntityBuilder[length + extraLenght];
            }
            
            Array.Copy(defaultEntities, 0, entitiesToBuild, 0, length);

            return index;
        }


        public IEntityBuilder[] entitiesToBuild
        {
            get => _entitiesToBuild;
        }
        
        IEntityBuilder[] _entitiesToBuild;
    }
}