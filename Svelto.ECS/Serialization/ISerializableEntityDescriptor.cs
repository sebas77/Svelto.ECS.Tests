using Svelto.DataStructures;

namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                                           hash                { get; }
        FasterReadOnlyList<ISerializableEntityBuilder> entitiesToSerialize { get; }
        
        T Get<T>() where T : unmanaged, IEntityStruct;
        void FillInitializer(ref EntityStructInitializer initializer);
    }
}