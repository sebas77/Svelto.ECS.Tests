namespace Svelto.ECS.Tests.ECS;

public class MegaReactEngineCallback : IReactOnAddEx<TestEntityViewComponent>,
        IReactOnRemoveEx<TestEntityViewComponent>, IReactOnSwapEx<TestEntityViewComponent>
{
    readonly IEntityFunctions _generateEntityFunctions;
    readonly IEntityFactory _generateEntityFactory;

    public MegaReactEngineCallback(IEntityFunctions generateEntityFunctions, IEntityFactory generateEntityFactory)
    {
        _generateEntityFunctions = generateEntityFunctions;
        _generateEntityFactory = generateEntityFactory;
    }

    public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (_, ids, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            _generateEntityFunctions.SwapEntityGroup<EntityDescriptorWithComponents>(ids[i], groupID, Groups.GroupB);
        }
    }

    public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (_, ids, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            _generateEntityFactory.BuildEntity<EntityDescriptorWithComponents>(ids[i], groupID);
        }
    }

    public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
    {
        var (_, ids, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            _generateEntityFunctions.RemoveEntity<EntityDescriptorWithComponents>(ids[i], toGroup);
        }
    }
}