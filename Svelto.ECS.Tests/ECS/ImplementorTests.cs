using NUnit.Framework;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schedulers;
using System.Threading;

namespace Svelto.ECS.Tests.ECS
{
    internal class ImplementorTests
    {
        [SetUp]
        public void Init()
        {
            ctx1 = new Context();
            ctx2 = new Context();   
            ctx3 = new Context();
        }

        private void Make(int n, Context ctx)
        {
            for (uint i = 0; i < n; i++)
            {
                ctx.EntityFactory.BuildEntity<ImplementorTestDescriptor>(new EGID(i, group0), new[] { new TestItA(i) });
                ctx.Scheduler.SubmitEntities();
            }
        }

        [TestCase]
        public void ImplementorsInterferenceSinglethreaded()
        {
            Make(100000, ctx1);
            Make(100000, ctx2);
            Make(100000, ctx3);
        }
        [TestCase]
        public void ImplementorsInterferenceMultithreaded()
        {
            var t1 = new Thread(() => Make(100000, ctx1));
            var t2 = new Thread(() => Make(100000, ctx2));
            var t3 = new Thread(() => Make(100000, ctx3));
            t1.Start();
            t2.Start();
            t3.Start();

            t1.Join();
            t2.Join();
            t3.Join();
        }
        [TearDown]
        public void Dispose()
        {
            ctx1.EnginesRoot.Dispose();
            ctx2.EnginesRoot.Dispose();
            ctx3.EnginesRoot.Dispose();
        }

        Context ctx1;
        Context ctx2;
        Context ctx3;
        ExclusiveGroup group0 = new ExclusiveGroup();
        class Context
        {
            public Context()
            {
                Scheduler = new SimpleEntitiesSubmissionScheduler();
                EnginesRoot = new EnginesRoot(Scheduler);
                EntityFactory = EnginesRoot.GenerateEntityFactory();
            }

            public EnginesRoot EnginesRoot { get; }
            public SimpleEntitiesSubmissionScheduler Scheduler { get; }
            public IEntityFactory EntityFactory { get; }
        }

        class ImplementorTestDescriptor : GenericEntityDescriptor<ViewComponentForImplementorTest> { }
        struct ViewComponentForImplementorTest : IEntityViewComponent
        {
            public ITestIt TestIt;

            public EGID ID { get; set; }
        }

        interface ITestIt
        {
            uint Value { get; }
        }
        class TestItA : ITestIt, IImplementor
        {
            public uint Value { get; }
            public TestItA(uint v)
            {
                Value = v;
            }
        }
    }
}
