using NUnit.Framework;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS.Filters
{
    [TestFixture]
    public class QuerySubsetEntitiesTests
    {
        [SetUp]
        public void Init()
        {
            _scheduler = new SimpleEntitiesSubmissionScheduler();
            _root      = new EnginesRoot(_scheduler);
            _factory   = _root.GenerateEntityFactory();
            _engine    = new TestEngine();
            _root.AddEngine(_engine);
        }

        [TearDown]
        public void TearDown() { _root.Dispose(); }

        [Test]
        public void TestQuery()
        {
            var egid0 = _factory.BuildEntity<TestEntityDescriptor>(0, Tag.BuildGroup).EGID;
            var egid1 = _factory.BuildEntity<TestEntityDescriptor>(1, Tag.BuildGroup).EGID;
            var egid2 = _factory.BuildEntity<TestEntityDescriptor>(2, Tag.BuildGroup).EGID;
            var egid3 = _factory.BuildEntity<TestEntityDescriptor>(3, Tag.BuildGroup).EGID;

            _scheduler.SubmitEntities();

         //   _engine.entitiesDB.QueryEntities(Tag.Groups, new ZeroTest());
        }

        SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot                       _root;
        IEntityFactory                    _factory;
        TestEngine                        _engine;

        class TestEntityDescriptor : GenericEntityDescriptor<TestEntityComponent> { }

        class Tag : GroupTag<Tag> { }

        const int FilterIdA = 0;
    }

    // public class ZeroTest : EntitiesDB.QueryPredicate<TestEntityComponent>
    // {
    //     public override bool Predicate(in EntityCollection<TestEntityComponent> components)
    //     {
    //         var (buffer, count) = components;
    //
    //         return true;
    //     }
    // }
}