namespace Svelto.ECS.Serialization
{
    public interface ISerializableEntityDescriptor : IEntityDescriptor
    {
        uint                         hash                { get; }
        ISerializableEntityBuilder[] entitiesToSerialize { get; }
        /// <summary>
        /// Todo: this method will eventually be removed, as it will be merged with DeserializeEntityStructs
        /// </summary>
        /// <param name="initializer"></param>
        void FillInitializer(ref EntityStructInitializer initializer);

        /// <summary>
        /// Todo: This must be removed!
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Get<T>() where T : unmanaged, IEntityStruct;
    }
}