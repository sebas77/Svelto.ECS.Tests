using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class QueryGroupsTests : GenericTestsBaseClass
    {
        [Test]
        public void TestFindGroup()
        {
            _factory.BuildEntity<EntityDescriptorWithComponentAndViewComponent>
                (0, Groups.GroupA, new object[] {new TestFloatValue(1), new TestIntValue(1)});
            _factory.BuildEntity<EntityDescriptorWithComponents>
                (0, Groups.GroupB);
            _factory.BuildEntity<EntityDescriptorWith4Components>(1, Groups.GroupA, new object[] {new TestFloatValue(1), new TestIntValue(1)});
            
            _scheduler.SubmitEntities();

            var groups = _entitiesDB.entitiesForTesting
                       .FindGroups<TestEntityViewComponent, TestEntityComponent, TestEntityComponentWithProperties>();
            
            Assert.That(groups.count == 1);
            Assert.That(groups[0] == Groups.GroupA);
            
            groups = _entitiesDB.entitiesForTesting.FindGroups<TestEntityComponent, TestEntityComponentWithProperties>();
            
            Assert.That(groups.count == 2);
            Assert.That(groups[0] == Groups.GroupA);
            Assert.That(groups[1] == Groups.GroupB);
            
            groups = _entitiesDB.entitiesForTesting.FindGroups<TestEntityViewComponentString>();
            
            Assert.That(groups.count == 0);
            
            groups = _entitiesDB.entitiesForTesting.FindGroups<TestEntityViewComponent,
                                     TestEntityComponent, TestEntityComponentWithProperties, TestEntityComponentInt>();
            
            Assert.That(groups.count == 1);
            Assert.That(groups[0] == Groups.GroupA);
        }
        
        [Test]
        public void TestCombinedQuery()
        {
            _factory.BuildEntity<EntityDescriptorWithComponentAndViewComponent>(0, Groups.GroupA, new object[] {new TestFloatValue(1), new TestIntValue(1)});
            _factory.BuildEntity<EntityDescriptorWithComponents>(0, Groups.GroupB);
            _factory.BuildEntity<EntityDescriptorWith4Components>(1, Groups.GroupA, new object[] {new TestFloatValue(1), new TestIntValue(1)});
            
            _scheduler.SubmitEntities();
            
            var groups = _entitiesDB.entitiesForTesting.FindGroups<TestEntityViewComponent,
                TestEntityComponent, TestEntityComponentWithProperties, TestEntityComponentInt>();

            foreach (var ((entityComponents, entityComponentsWithProperties, viewComponents, count), _) in
                _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent, TestEntityComponentWithProperties, TestEntityViewComponent>(groups))
            {
                for (int i = 0; i < count; i++)
                {
                    Assert.That(entityComponents[i].intValue == 0);
                    Assert.That(entityComponentsWithProperties[i].intValue == 0);
                    Assert.That(viewComponents[i].TestIntValue.Value == 1);
                }
            }
        }
    }
}