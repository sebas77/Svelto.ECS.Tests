namespace Svelto.ECS.Tests.ECS;

public partial class DisableEnableEntitiesFilterTests
{
    class ChangeActiveEntityValueEngine: IStepEngine, IQueryingEntitiesEngine
    {
        public string name => nameof(ChangeActiveEntityValueEngine);

        public EntitiesDB entitiesDB { get; set; }

        public void Ready() { }

        public void Step()
        {
            var query = entitiesDB.QueryEntities<TestEntityComponent>(TestGroups.TestGroupTag.Groups);

            foreach (var ((buffer, count), _) in query)
            {
                for (var i = 0; i < count; i++)
                {
                    //This engine queries all enabled entities and increases their value by 1
                    ref var entity = ref buffer[i];
                    entity.SomeValue++;
                }
            }
        }
    }
}