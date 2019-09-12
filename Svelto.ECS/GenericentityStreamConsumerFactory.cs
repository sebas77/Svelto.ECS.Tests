namespace Svelto.ECS
{
    class GenericEntityStreamConsumerFactory : IEntityStreamConsumerFactory
    {
        public GenericEntityStreamConsumerFactory(EnginesRoot weakReference)
        {
            _enginesRoot = weakReference;
        }

        public Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.GenerateConsumer<T>(name, capacity);
        }

        public Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, int capacity) where T : unmanaged, IEntityStruct
        {
            return _enginesRoot.GenerateConsumer<T>(group, name, capacity);
        }

        readonly EnginesRoot _enginesRoot;
    }
    
    public interface IEntityStreamConsumerFactory
    {
        Consumer<T> GenerateConsumer<T>(string name, int capacity) where T : unmanaged, IEntityStruct;
        Consumer<T> GenerateConsumer<T>(ExclusiveGroup group, string name, int capacity) 
            where T : unmanaged, IEntityStruct;
    }
}