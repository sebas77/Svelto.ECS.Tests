using System;
using Svelto.ECS.Serialization;

namespace Svelto.ECS
{
    /// <summary>
    /// Inherit from an ExtendibleEntityDescriptor to extend a base entity descriptor that can be used
    /// to swap and remove specialized entities from abstract engines
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public abstract class ExtendibleEntityDescriptor<TType>:IExtendibleEntityDescriptor where TType : IEntityDescriptor, new()
    {
        static ExtendibleEntityDescriptor()
        {
            if (typeof(ISerializableEntityDescriptor).IsAssignableFrom(typeof(TType)))
                throw new Exception($"SerializableEntityDescriptors cannot be used as base entity descriptor: {typeof(TType)}");
        }

        protected ExtendibleEntityDescriptor(IEntityBuilder[] extraEntities)
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(extraEntities);
        }

        protected ExtendibleEntityDescriptor()
        {
            _dynamicDescriptor = new DynamicEntityDescriptor<TType>(true);
        }

        public IEntityBuilder[] entitiesToBuild => _dynamicDescriptor.entitiesToBuild;

        readonly DynamicEntityDescriptor<TType> _dynamicDescriptor;
    }

    public interface IExtendibleEntityDescriptor:IEntityDescriptor
    {}
}