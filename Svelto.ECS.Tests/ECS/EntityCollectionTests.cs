using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Svelto.ECS.Tests.ECS
{
    [TestFixture(0u, (ushort) 4, 100)]
    [TestFixture(20u, (ushort) 6, 100)]
    [TestFixture(123u, (ushort) 8, 100)]
    class EntityCollectionTests
    {
        public EntityCollectionTests(uint idStart, ushort groupCount, int entityCountPerGroup)
        {
            this._idStart             = idStart;
            this._groupCount          = groupCount;
            this._entityCountPerGroup = entityCountPerGroup;
        }

        readonly uint   _idStart;
        readonly ushort _groupCount;
        readonly int    _entityCountPerGroup;

        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _entityFactory;
        IEntityFunctions                  _entityFunctions;
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                        _testEngine;
        ExclusiveGroup                    _group;

        [SetUp]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot                         = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _testEngine                          = new TestEngine();

            _enginesRoot.AddEngine(_testEngine);

            _entityFactory   = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();

            _group = new ExclusiveGroup(_groupCount);

            var id = _idStart;
            for (uint i = 0; i < _groupCount; i++)
            {
                for (int j = 0; j < _entityCountPerGroup; j++)
                {
                    _entityFactory.BuildEntity<TestEntityWithComponentViewAndComponentStruct>(
                        new EGID(id++, _group + i), new object[] {new TestFloatValue(1f), new TestIntValue(1)});
                }
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            foreach (var ((buffer, count), exclusiveGroupStruct) in _testEngine
                                                                   .entitiesDB.QueryEntities<TestEntityViewStruct>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].TestFloatValue.Value += buffer[i].ID.entityID;
                    buffer[i].TestIntValue.Value   += (int) buffer[i].ID.entityID;
                }
            }

            foreach (var ((buffer, count), exclusiveGroupStruct) in _testEngine
                                                                   .entitiesDB.QueryEntities<TestEntityStruct>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].floatValue = 1 + buffer[i].ID.entityID;
                    buffer[i].intValue   = 1 + (int) buffer[i].ID.entityID;
                }
            }
        }

        [TestCase(Description = "Test EntityCollection<T> QueryEntities")]
        public void TestEntityCollection1()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                EntityCollection<TestEntityStruct> entities =
                    _testEngine.entitiesDB.QueryEntities<TestEntityStruct>(_group + i);
                EntityCollection<TestEntityViewStruct> entityViews =
                    _testEngine.entitiesDB.QueryEntities<TestEntityViewStruct>(_group + i);

                Assert.AreEqual(_entityCountPerGroup, entities.count);
                Assert.AreEqual(_entityCountPerGroup, entityViews.count);

                foreach (var entity in entities)
                {
                    Assert.AreEqual(entity.ID.entityID + 1, entity.floatValue);
                    Assert.AreEqual(entity.ID.entityID + 1, entity.intValue);
                }

                foreach (var entityView in entityViews)
                {
                    Assert.AreEqual(entityView.ID.entityID + 1, entityView.TestFloatValue.Value);
                    Assert.AreEqual(entityView.ID.entityID + 1, entityView.TestIntValue.Value);
                }
            }
        }

        [TestCase(Description = "Test EntityCollection<T> ToBuffer ToManagedArray")]
        public void TestEntityCollection1ToBufferToManagedArray()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                EntityCollection<TestEntityStruct> entities =
                    _testEngine.entitiesDB.QueryEntities<TestEntityStruct>(_group + i);
                EntityCollection<TestEntityViewStruct> entityViews =
                    _testEngine.entitiesDB.QueryEntities<TestEntityViewStruct>(_group + i);

                var entitiesBuffer = entities.ToBuffer();
                // can't get a managed array from a native buffer
                Assert.Throws<NotImplementedException>(() => entitiesBuffer.buffer.ToManagedArray());

                var entityViewsBuffer       = entityViews.ToBuffer();
                var entityViewsManagedArray = entityViewsBuffer.buffer.ToManagedArray();

                for (int j = 0; j < entityViews.count; j++)
                {
                    Assert.AreEqual(entityViews[j].ID.entityID + 1, entityViewsManagedArray[j].TestFloatValue.Value);
                    Assert.AreEqual(entityViews[j].ID.entityID + 1, entityViewsManagedArray[j].TestIntValue.Value);
                }
            }
        }

        [TestCase(Description = "Test EntityCollection<T> ToBuffer ToNativeArray")]
        public void TestEntityCollection1ToBufferToNativeArray()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                EntityCollection<TestEntityStruct> entities =
                    _testEngine.entitiesDB.QueryEntities<TestEntityStruct>(_group + i);
                EntityCollection<TestEntityViewStruct> entityViews =
                    _testEngine.entitiesDB.QueryEntities<TestEntityViewStruct>(_group + i);

                var entityViewsBuffer = entityViews.ToBuffer();
                // can't get a native array from a managed buffer
                Assert.Throws<NotImplementedException>(() => entityViewsBuffer.buffer.ToNativeArray(out _));

                var entitiesBuffer      = entities.ToBuffer();
                var entitiesNativeArray = entitiesBuffer.buffer.ToNativeArray(out _);
                var sizeOfStruct        = Marshal.SizeOf<TestEntityStruct>();

                for (int j = 0; j < entities.count; j++)
                {
                    var entity = Marshal.PtrToStructure<TestEntityStruct>(entitiesNativeArray + j * sizeOfStruct);
                    Assert.AreEqual(entities[j].ID.entityID + 1, entity.floatValue);
                    Assert.AreEqual(entities[j].ID.entityID + 1, entity.intValue);
                }
            }
        }

        [TestCase(Description = "Test EntityCollection<T> ToNativeBuffer")]
        public void TestEntityCollection1ToNativeBuffer()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                EntityCollection<TestEntityStruct> entities =
                    _testEngine.entitiesDB.QueryEntities<TestEntityStruct>(_group + i);

                var entitiesBuffer = entities.ToBuffer();
                Assert.AreEqual(entities.count, entitiesBuffer.count);

                var entitiesNativeArray = entitiesBuffer.buffer.ToNativeArray(out _);
                var sizeOfStruct        = Marshal.SizeOf<TestEntityStruct>();

                for (int j = 0; j < entities.count; j++)
                {
                    var entity = Marshal.PtrToStructure<TestEntityStruct>(entitiesNativeArray + j * sizeOfStruct);
                    Assert.AreEqual(entities[j].ID.entityID + 1, entity.floatValue);
                    Assert.AreEqual(entities[j].ID.entityID + 1, entity.intValue);
                }
            }
        }
    }
}