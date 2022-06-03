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
            
            groups = _entitiesDB.entitiesForTesting
                                    .FindGroups<TestEntityComponent, TestEntityComponentWithProperties>();
            
            Assert.That(groups.count == 2);
            Assert.That(groups[0] == Groups.GroupA);
            Assert.That(groups[1] == Groups.GroupB);
            
            groups = _entitiesDB.entitiesForTesting
                                .FindGroups<TestEntityViewComponentString>();
            
            Assert.That(groups.count == 0);
            
             
            groups = _entitiesDB.entitiesForTesting
                                .FindGroups<TestEntityViewComponent,
                                     TestEntityComponent, TestEntityComponentWithProperties, TestEntityComponentInt>();
            
            Assert.That(groups.count == 1);
            Assert.That(groups[0] == Groups.GroupA);
        }
    }
}