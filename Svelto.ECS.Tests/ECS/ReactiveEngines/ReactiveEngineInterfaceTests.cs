using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using NUnit.Framework;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    public class ReactiveEngineInterfaceTests : GenericTestsBaseClass
    {
        [TestCase]
        public void Test_ReactiveEngine_WithConcreteInterface()
        {
            var engine = new ReactConcreteEngine();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                _factory.BuildEntity<EntityDescriptorWithComponents>(i, GroupA);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; i++)
                _functions.RemoveEntity<EntityDescriptorWithComponents>(i, GroupA);

            _scheduler.SubmitEntities();

            Assert.AreEqual(10 + 1 + 5, engine.calledCount);
        }

        [TestCase]
        public void Test_ReactiveEngine_WithGenericInterface()
        {
            var engine = new ReactGenericEngine<TestEntityComponent>();

            _enginesRoot.AddEngine(engine);

            for (uint i = 0; i < 10; i++)
                _factory.BuildEntity<EntityDescriptorWithComponents>(i, GroupA);

            _scheduler.SubmitEntities();

            for (uint i = 0; i < 5; i++)
                _functions.RemoveEntity<EntityDescriptorWithComponents>(i, GroupA);

            _scheduler.SubmitEntities();

            Assert.AreEqual(10 + 1 + 5, engine.calledCount);
        }

        public interface IReactOnConcreteType :
            IReactOnAdd<TestEntityComponent>,
            IReactOnAddAndRemove<TestEntityComponent>,
            IReactOnAddEx<TestEntityComponent>
        { }

        internal class ReactConcreteEngine : IReactOnConcreteType
        {
            public int calledCount = 0;

            public void Add(ref TestEntityComponent entityComponent, EGID egid)
            {
                calledCount++;
            }

            public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponent> collection, ExclusiveGroupStruct groupID)
            {
                calledCount++;
            }

            public void Remove(ref TestEntityComponent entityComponent, EGID egid)
            {
                calledCount++;
            }
        }

        public interface IReactGenericType<TComponent1, TComponent2> :
                IReactOnAddAndRemove<TComponent1>,
                IReactOnAddEx<TComponent2>
            where TComponent1 : struct, IEntityComponent
            where TComponent2 : struct, IEntityComponent
        { }

        internal class ReactGenericEngine<TComponent> : IReactGenericType<TComponent, TestEntityComponentWithProperties>
            where TComponent : struct, IEntityComponent
        {
            public int calledCount = 0;

            public void Add(ref TComponent entityComponent, EGID egid)
            {
                calledCount++;
            }

            public void Add((uint start, uint end) rangeOfEntities, in EntityCollection<TestEntityComponentWithProperties> collection, ExclusiveGroupStruct groupID)
            {
                calledCount++;
            }

            public void Remove(ref TComponent entityComponent, EGID egid)
            {
                calledCount++;
            }
        }

    }
}