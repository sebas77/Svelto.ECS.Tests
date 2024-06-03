using System;
using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Serialization;
using Assert = NUnit.Framework.Assert;

namespace Svelto.ECS.Tests.ECS
{
    public static class EntityCollectionGroups 
    {
        public static ushort numberOfGroups = 10;
        public static readonly ExclusiveGroup _group = new ExclusiveGroup(numberOfGroups);
    }
    [TestFixture(0u, 100)]
    [TestFixture(20u, 100)]
    [TestFixture(123u, 100)]
    public class EntityCollectionTests
    {
        public EntityCollectionTests(uint idStart, int entityCountPerGroup)
        {
            _idStart             = idStart;
            _entityCountPerGroup = entityCountPerGroup;
        }

        readonly uint _idStart;
        readonly int  _entityCountPerGroup;

        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _entityFactory;
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
        IUnitTestingInterface             _entitiesDB;
        IEntitySerialization              _entitySerializer;

        readonly ushort _groupCount = EntityCollectionGroups.numberOfGroups;
        static ExclusiveGroup _rangedGroup = EntityCollectionGroups._group;

        [SetUp]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot                         = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _entitiesDB                          = _enginesRoot;

            _entityFactory = _enginesRoot.GenerateEntityFactory();
            _entitySerializer = _enginesRoot.GenerateEntitySerializer();
            _enginesRoot.GenerateEntityFunctions();

            var id = _idStart;
            for (uint i = 0; i < _groupCount; i++)
            {
                for (int j = 0; j < _entityCountPerGroup; j++)
                {
                    _entityFactory.BuildEntity<EntityDescriptorWithComponentAndViewComponent>(
                        new EGID(id++, _rangedGroup + i), new object[] { new TestFloatValue(1f), new TestIntValue(1) });

                    _entityFactory.BuildEntity<EntityDescriptorViewComponentWithString>(
                        new EGID(id++, _rangedGroup + i), new object[] { new TestStringValue("test") });
                }
            }

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            foreach (var ((buffer, count), _) in
                     _entitiesDB.entitiesForTesting.QueryEntities<TestEntityViewComponent>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].TestFloatValue.Value += buffer[i].ID.entityID;
                    buffer[i].TestIntValue.Value   += (int)buffer[i].ID.entityID;
                }
            }
#if SLOW_SVELTO_SUBMISSION
            foreach (var ((buffer, count), _) in _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].floatValue = 1 + buffer[i].ID.entityID;
                    buffer[i].intValue   = 1 + (int)buffer[i].ID.entityID;
                }
            }
#endif            
            foreach (var ((buffer, ids, count), _) in _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].floatValue = 1 + ids[i];
                    buffer[i].intValue = 1 + (int)ids[i];
                }
            }

            foreach (var ((buffer, count), _) in _entitiesDB.entitiesForTesting
                                                            .QueryEntities<TestEntityViewComponentString>())
            {
                for (int i = 0; i < count; i++)
                {
                    buffer[i].TestStringValue.Value = (1 + buffer[i].ID.entityID).ToString();
                }
            }
        }

        [TestCase(Description = "Test EntityCollection<T> QueryEntities")]
        public void TestEntityCollection1()
        {
            void TestNotAcceptedEntityComponent()
            {
                _entityFactory.BuildEntity<EntityDescriptorViewComponentWithCustomStruct>(
                    new EGID(0, _rangedGroup), new object[] { new TestCustomStructWithString("test") });
            }

            Assert.Throws<TypeInitializationException>(TestNotAcceptedEntityComponent);
        }

        [TestCase(Description = "Test EntityCollection<T> ToBuffer ToManagedArray")]
        public void TestEntityCollection1ToBufferToManagedArray()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                (MB<TestEntityViewComponent> entityViewsManagedArray, int count) =
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityViewComponent>(_rangedGroup + i);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(entityViewsManagedArray[j].ID.entityID + 1
                                  , entityViewsManagedArray[j].TestFloatValue.Value);
                    Assert.AreEqual(entityViewsManagedArray[j].ID.entityID + 1
                                  , entityViewsManagedArray[j].TestIntValue.Value);
                }
            }
        }
        

        [TestCase(Description = "Test EntityCollection<T> ToBuffer ToNativeArray")]
        public void TestEntityCollection1ToBufferToNativeArray()
        {
#if SLOW_SVELTO_SUBMISSION            
            for (uint i = 0; i < _groupCount; i++)
            {
                var (entityComponents, count) =
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(_rangedGroup + i);

                for (int j = 0; j < count; j++)
                {
                    ref var entity = ref entityComponents[j];
                    Assert.AreEqual(entity.ID.entityID + 1, entity.floatValue);
                    Assert.AreEqual(entity.ID.entityID + 1, entity.intValue);
                }
            }
#endif            
            for (uint i = 0; i < _groupCount; i++)
            {
                var (entityComponents, ids, count) =
                        _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(_rangedGroup + i);

                for (int j = 0; j < count; j++)
                {
                    ref var entity = ref entityComponents[j];
                    Assert.AreEqual(ids[j] + 1, entity.floatValue);
                    Assert.AreEqual(ids[j] + 1, entity.intValue);
                }
            }
        }

#if SLOW_SVELTO_SUBMISSION
        [TestCase(Description = "Test EntityCollection<T> ToBuffer ToNativeArray")]
        public void TestEntityCollection1ToBufferToNativeArrayWithEntitiesID()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                var (entityComponents, entityIDs, count) =
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(_rangedGroup + i);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual(entityComponents[j].ID.entityID, entityIDs[j]);
                   
                }
            }
        }
#endif
        
        [Test]
        public void TestEmptyEntityCollectionDeconstructs()
        {
            void QueryEmptyGroupAndDeconstruct()
            {
                var dummyGroup = new ExclusiveGroup();
                var (components, nativeEntityIDs, count) =
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityComponent>(dummyGroup);
            }

            Assert.DoesNotThrow(QueryEmptyGroupAndDeconstruct);
        }

        [TestCase(Description = "Test EntityCollection<T> String")]
        public void TestEntityCollection1WithString()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                var (entityViews, count) =
                    _entitiesDB.entitiesForTesting.QueryEntities<TestEntityViewComponentString>(_rangedGroup + i);

                for (int j = 0; j < count; j++)
                {
                    Assert.AreEqual((entityViews[j].ID.entityID + 1).ToString(), entityViews[j].TestStringValue.Value);
                }
            }
        }

        [TestCase(Description = "Test GroupHashMap Registration for first group")]
        public void TestGroupHashMap0Registration()
        {
            ExclusiveGroupStruct group = _rangedGroup;
            var hash = _entitySerializer.GetHashFromGroup(group);
            var deserialized = _entitySerializer.GetGroupFromHash(hash);
            Assert.AreEqual(group, deserialized);
        }

        [TestCase(Description = "Test GroupHashMap Registration for all groups")]
        public void TestGroupHashMapRegistration()
        {
            for (uint i = 0; i < _groupCount; i++)
            {
                ExclusiveGroupStruct group = _rangedGroup + i;
                var hash = _entitySerializer.GetHashFromGroup(group);
                var deserialized = _entitySerializer.GetGroupFromHash(hash);
                Assert.AreEqual(group, deserialized);
            }
        }
    }
}
