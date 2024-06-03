using NUnit.Framework;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Schedulers;
using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Svelto.ECS.Tests.ECS
{
    class ImplementorTests
    {
        [SetUp]
        public void Init()
        {
            ctxs = new List<Context>();
            for (int i = 0; i < 10; i++)
                ctxs.Add(new Context());
        }

        void Make(int n, Context ctx)
        {
            for (uint i = 0; i < n; i++)
            {
                try
                {
                    ctx.EntityFactory.BuildEntity<ImplementorTestDescriptor>(new EGID(i, group0), new[] { new TestItA(i) });
                    ctx.Scheduler.SubmitEntities();
                } catch (Exception e)
                {
                    // In the event of an exception we want to save it instead of throwing
                    // because NUnit doesn't recognize exceptions or asserts on different threads as "failed"
                    ctx.Exception = ExceptionDispatchInfo.Capture(e);
                    break;
                }
            }
        }

        [TestCase]
        public void ImplementorsInterferenceSinglethreaded()
        {
            foreach(var ctx in ctxs)
                Make(10, ctx);

            // Use the same logic for re-throwing exception as for multithreaded
            // to keep the Make function more generic
            foreach (var ctx in ctxs)
                ctx.Exception?.Throw();
        }
        [TestCase]
        public void ImplementorsInterferenceMultithreaded()
        {
            var threads = new List<Thread>();
            foreach (var ctx in ctxs) {
                var _ctx = ctx;
                threads.Add(new Thread(() => Make(100000, _ctx)));
            }
            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();

            // We need to rethrow any exceptions that might have occurred during the test
            foreach (var ctx in ctxs)
                ctx.Exception?.Throw();
        }
        [TearDown]
        public void Dispose()
        {
            foreach (var ctx in ctxs)
                ctx.EnginesRoot.Dispose();
        }

        List<Context> ctxs;
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
            public ExceptionDispatchInfo Exception { get; set; }
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
