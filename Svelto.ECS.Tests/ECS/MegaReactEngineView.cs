namespace Svelto.ECS.Tests.ECS;

public class MegaReactEngineView : IReactOnAddAndRemove<TestEntityViewComponent>, IReactOnDispose<TestEntityViewComponent>, IReactOnDisposeEx<TestEntityViewComponent>,
        IReactOnSwap<TestEntityViewComponent>, IReactOnAddEx<TestEntityViewComponent>,
        IReactOnRemoveEx<TestEntityViewComponent>, IReactOnSwapEx<TestEntityViewComponent>
{
    public int  addCounter;
    public int  removeCounter;
        
    public int  legacyAddCounter;
    public int  legacyRemoveCounter;
        
    public int legacySwapCounter;
    public int swapCounter;
        
    public int  removeCounterOnDispose;
    public int  legacyRemoveCounterOnDispose;
       
    public void Add(ref TestEntityViewComponent entityComponent, EGID egid)
    {
        legacyAddCounter += entityComponent.TestIntValue.Value;
    }

    public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            var entityComponent = buffer[i];
            addCounter += entityComponent.TestIntValue.Value;
        }
    }

    void IReactOnRemove<TestEntityViewComponent>.Remove(ref TestEntityViewComponent entityComponent, EGID egid)
    {
        legacyRemoveCounter += entityComponent.TestIntValue.Value;
    }

    public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            var entityComponent = buffer[i];
            removeCounter += entityComponent.TestIntValue.Value;
        }
    }

    public void MovedTo(ref TestEntityViewComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
    {
        legacySwapCounter += entityComponent.TestIntValue.Value;
    }

    public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            ref var entityComponent = ref buffer[i];
            swapCounter += entityComponent.TestIntValue.Value;
        }
    }
        
    void IReactOnDispose<TestEntityViewComponent>.Remove(ref TestEntityViewComponent entityComponent, EGID egid)
    {
        legacyRemoveCounterOnDispose += entityComponent.TestIntValue.Value;
    }
        
    void IReactOnDisposeEx<TestEntityViewComponent>.Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityViewComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            ref var entityComponent = ref buffer[i];
            removeCounterOnDispose += entityComponent.TestIntValue.Value;
        }
    }
}