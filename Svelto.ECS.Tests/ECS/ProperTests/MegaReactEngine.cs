namespace Svelto.ECS.Tests.ECS;

public class MegaReactEngine : IReactOnAddAndRemove<TestEntityComponent>, IReactOnDispose<TestEntityComponent>, IReactOnSubmission,
        IReactOnSwap<TestEntityComponent>, IReactOnAddEx<TestEntityComponent>, IReactOnRemoveEx<TestEntityComponent>,
        IReactOnSwapEx<TestEntityComponent>, IReactOnDisposeEx<TestEntityComponent>, IDisposableEngine
{
    public int  addCounter;
    public int  removeCounter;
        
    public int  legacyAddCounter;
    public int  legacyRemoveCounter;
        
    public int legacySwapCounter;
    public int swapCounter;
        
    public bool entitySubmittedIsCalled;
    public int  legacyRemoveCounterOnDispose;
    public int  removeCounterOnDispose;
        

    public void Add(ref TestEntityComponent entityComponent, EGID egid)
    {
        legacyAddCounter += entityComponent.intValue;
    }

    public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            var entityComponent = buffer[i];
            addCounter += entityComponent.intValue;
        }
    }

    void IReactOnDispose<TestEntityComponent>.Remove(ref TestEntityComponent entityComponent, EGID egid)
    {
        legacyRemoveCounterOnDispose += entityComponent.intValue;
    }

    void IReactOnRemove<TestEntityComponent>.Remove(ref TestEntityComponent entityComponent, EGID egid)
    {
        legacyRemoveCounter += entityComponent.intValue;
    }

    public void Remove((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
        ExclusiveGroupStruct groupID)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            var entityComponent = buffer[i];
            if (isDisposing)
                removeCounterOnDispose += entityComponent.intValue;
            else
                removeCounter += entityComponent.intValue;
        }
    }

    public void EntitiesSubmitted()
    {
        entitySubmittedIsCalled = true;
    }

    public void MovedTo(ref TestEntityComponent entityComponent, ExclusiveGroupStruct previousGroup, EGID egid)
    {
        legacySwapCounter += entityComponent.intValue;
    }

    public void MovedTo((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection,
        ExclusiveGroupStruct fromGroup, ExclusiveGroupStruct toGroup)
    {
        var (buffer, _) = collection;

        for (var i = rangeOfEntities.start; i < rangeOfEntities.end; i++)
        {
            var entityComponent = buffer[i];
            swapCounter += entityComponent.intValue;
        }
    }

    public void Dispose()
    {
            
    }

    public bool isDisposing { get; set; }
}