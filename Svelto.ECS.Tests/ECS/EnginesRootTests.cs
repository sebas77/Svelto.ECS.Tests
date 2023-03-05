using System;
using System.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Svelto.ECS.Schedulers;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture]
    class EnginesRootTests
    {
        [TestCase(Description = "Test that engines are disposed when the EngineRoot is disposed")]
        public void TestDisposeEnginesRoot()
        {
            _scheduler   = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);

            Assert.DoesNotThrow(_enginesRoot.Dispose);
        }
        
        [TestCase(Description = "Test that engines are disposed when the EngineRoot is disposed")]
        public void TestDisposeEngines()
        {
            _scheduler   = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            var engines = new DisposableEngineBase[]
            {
                new DisposableEngine1(), new DisposableEngine2(), new DisposableEngine3()
            };

            for (var i = 0; i < engines.Length; i++)
            {
                _enginesRoot.AddEngine(engines[i]);
            }

            // all engines start out not disposed
            Assert.IsTrue(engines.All(sut => sut.disposed == false));

            _enginesRoot.Dispose();

            // all engines should be disposed after root is disposed
            Assert.IsTrue(engines.All(sut => sut.disposed == true));
        }

        [TestCase(Description =
            "Test that engines are disposed when the EngineRoot is disposed, using engines of the same type")]
        public void TestDisposeEngineMultiples()
        {
            _scheduler   = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot = new EnginesRoot(_scheduler);
            
            var engines = new[]
            {
                new DisposableEngineMultiple(), new DisposableEngineMultiple(), new DisposableEngineMultiple()
            };

            for (var i = 0; i < engines.Length; i++)
            {
                _enginesRoot.AddEngine(engines[i]);
            }

            // all engines start out not disposed
            Assert.IsTrue(engines.All(sut => sut.disposed == false));

            _enginesRoot.Dispose();

            // all engines should be disposed after root is disposed
            Assert.IsTrue(engines.All(sut => sut.disposed == true));
        }

        class DisposableEngineBase : IEngine, IDisposable
        {
            public bool disposed = false;

            public void Dispose() { disposed = true; }
        }

        class DisposableEngine1 : DisposableEngineBase { }

        class DisposableEngine2 : DisposableEngineBase { }

        class DisposableEngine3 : DisposableEngineBase { }

        [AllowMultiple]
        class DisposableEngineMultiple : DisposableEngineBase { }

        SimpleEntitiesSubmissionScheduler _scheduler;
        EnginesRoot                  _enginesRoot;
    }
}