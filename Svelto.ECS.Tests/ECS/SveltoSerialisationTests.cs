﻿using System;
 using System.Runtime.InteropServices;
 using NUnit.Framework;
using Svelto.DataStructures;
 using Svelto.ECS.Hybrid;
using Svelto.ECS.Schedulers;
using Svelto.ECS.Serialization;

namespace Svelto.ECS.Tests.Serialization
{
    [TestFixture]
    public class TestSveltoECSSerialisation
    {
        class NamedGroup1 : NamedExclusiveGroup<NamedGroup1> { }

        [SetUp]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            _enginesRoot                         = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest         = new TestEngine();

            _enginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            _entityFactory   = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
        }

        [TearDown]
        public void Dispose() { _enginesRoot.Dispose(); }

        [TestCase]
        public void TestSerializingToByteArrayRemoveGroup()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized()
            {
                value = 5
            });
            init.Init(new EntityStructSerialized2()
            {
                value = 4
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 3
            });
            init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(1, NamedGroup1.Group);
            init.Init(new EntityStructSerialized()
            {
                value = 4
            });
            init.Init(new EntityStructSerialized2()
            {
                value = 3
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 2
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes                    = new FasterList<byte>();
            var              generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var              simpleSerializationData  = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);
            generateEntitySerializer.SerializeEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);

            _entityFunctions.RemoveEntitiesFromGroup(NamedGroup1.Group);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            simpleSerializationData.Reset();

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            var queryEGID0 = new EGID(0, NamedGroup1.Group);
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(queryEGID0).value
              , Is.EqualTo(5));
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(queryEGID0).value
              , Is.EqualTo(4));
            Assert.That(
                _neverDoThisIsJustForTheTest
                   .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(queryEGID0).value1
              , Is.EqualTo(3));

            var queryEGID1 = new EGID(1, NamedGroup1.Group);
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(queryEGID1).value
              , Is.EqualTo(4));
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(queryEGID1).value
              , Is.EqualTo(3));
            Assert.That(
                _neverDoThisIsJustForTheTest
                   .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(queryEGID1).value1
              , Is.EqualTo(2));
        }

        [TestCase]
        public void TestSerializingToByteArrayNewEnginesRoot()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized()
            {
                value = 5
            });
            init.Init(new EntityStructSerialized2()
            {
                value = 4
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 3
            });
            init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(1, NamedGroup1.Group);
            init.Init(new EntityStructSerialized()
            {
                value = 4
            });
            init.Init(new EntityStructSerialized2()
            {
                value = 3
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 2
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            var generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var simpleSerializationData  = new SimpleSerializationData(new FasterList<byte>());
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);
            generateEntitySerializer.SerializeEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);

            var serializerSubmissionScheduler = new SimpleEntitiesSubmissionScheduler();

            var newEnginesRoot = new EnginesRoot(serializerSubmissionScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();
            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);
            serializerSubmissionScheduler.SubmitEntities();

            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(0, NamedGroup1.Group).value
              , Is.EqualTo(5));
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value
              , Is.EqualTo(4));
            Assert.That(
                _neverDoThisIsJustForTheTest
                   .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1
              , Is.EqualTo(3));

            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(1, NamedGroup1.Group).value
              , Is.EqualTo(4));
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(1, NamedGroup1.Group).value
              , Is.EqualTo(3));
            Assert.That(
                _neverDoThisIsJustForTheTest
                   .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(1, NamedGroup1.Group).value1
              , Is.EqualTo(2));

            newEnginesRoot.Dispose();
        }

        [TestCase]
        public void TestSerializingWithEntityStructsWithVersioning()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptorV0>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized2()
            {
                value = 4
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 3
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes                    = new FasterList<byte>();
            var              generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var              simpleSerializationData  = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);

            var serializerSubmissionScheduler = new SimpleEntitiesSubmissionScheduler();
            var newEnginesRoot = new EnginesRoot(serializerSubmissionScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();

            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();

            //needed for the versioning to work
            generateEntitySerializer.RegisterSerializationFactory<SerializableEntityDescriptorV0>(
                new DefaultVersioningFactory<SerializableEntityDescriptorV1>());

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);

            serializerSubmissionScheduler.SubmitEntities();

            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value
              , Is.EqualTo(4));
            Assert.That(
                _neverDoThisIsJustForTheTest
                   .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1
              , Is.EqualTo(3));

            newEnginesRoot.Dispose();
        }

        [TestCase]
        public void TestSerializingWithEntityViewStructsAndFactories()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptorWithViews>(
                0, NamedGroup1.Group, new[] {new Implementor(1)});
            init.Init(new EntityStructSerialized()
            {
                value = 5
            });
            init.Init(new EntityStructSerialized2()
            {
                value = 4
            });
            init.Init(new EntityStructPartiallySerialized()
            {
                value1 = 3
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes                    = new FasterList<byte>();
            var              generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var              simpleSerializationData  = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                   , (int) SerializationType.Storage);

            SimpleEntitiesSubmissionScheduler simpleSubmissionEntityViewScheduler = new SimpleEntitiesSubmissionScheduler();
            var                         newEnginesRoot = new EnginesRoot(simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();

            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();
            DeserializationFactory factory = new DeserializationFactory();
            generateEntitySerializer.RegisterSerializationFactory<SerializableEntityDescriptorWithViews>(factory);

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData
                                                        , (int) SerializationType.Storage);

            simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(0, NamedGroup1.Group).value
              , Is.EqualTo(5));
            Assert.That(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value
              , Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest
                       .entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1
                      , Is.EqualTo(3));

            newEnginesRoot.Dispose();
        }

        [TestCase]
        public void TestSerializingWithEntityReferences()
        {
            var egid0 = new EGID(0, NamedGroup1.Group);
            var init0 = _entityFactory.BuildEntity<SerializableEntityDescriptorWithReferences>(egid0);
            var egid1 = new EGID(1, NamedGroup1.Group);
            var init1 = _entityFactory.BuildEntity<SerializableEntityDescriptorWithReferences>(egid1);

            init0.Init(new EntityStructSerialized()
            {
                value = 2
            });
            init0.Init(new EntityStructSerialized2()
            {
                value = 4
            });
            init0.Init(new EntityReferenceParatiallySerialized()
            {
                value = int.MinValue,
                reference = init1.reference,
            });
            init0.Init(new EntityReferenceSerialized
            {
                value1 = int.MinValue,
                reference = init0.reference,
                value2 = long.MinValue
            });

            init1.Init(new EntityStructSerialized()
            {
                value = 1
            });
            init1.Init(new EntityStructSerialized2()
            {
                value = 3
            });
            init1.Init(new EntityReferenceParatiallySerialized()
            {
                value = int.MaxValue,
                reference = init0.reference,
            });
            init1.Init(new EntityReferenceSerialized
            {
                value1 = int.MaxValue,
                reference = init1.reference,
                value2 = long.MaxValue
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes                    = new FasterList<byte>();
            var              generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var              simpleSerializationData  = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(egid0, simpleSerializationData, (int) SerializationType.Storage);
            generateEntitySerializer.SerializeEntity(egid1, simpleSerializationData, (int) SerializationType.Storage);

            _entityFunctions.RemoveEntitiesFromGroup(NamedGroup1.Group);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            simpleSerializationData.Reset();

            generateEntitySerializer.DeserializeNewEntity(egid0, simpleSerializationData, (int) SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(egid1, simpleSerializationData, (int) SerializationType.Storage);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            // Entity 0 assertions.
            Assert.AreEqual(2,
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(egid0).value);
            Assert.AreEqual(4,
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(egid0).value);

            var entityRefPartially =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityReferenceParatiallySerialized>(egid0);
            // Assert.AreEqual(0, entityRefPartially.value);
            Assert.AreEqual(egid1, _neverDoThisIsJustForTheTest.entitiesDB.GetEGID(entityRefPartially.reference));

            var entityRefStruct =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityReferenceSerialized>(egid0);
            Assert.AreEqual(int.MinValue, entityRefStruct.value1);
            Assert.AreEqual(long.MinValue, entityRefStruct.value2);
            Assert.AreEqual(egid0, _neverDoThisIsJustForTheTest.entitiesDB.GetEGID(entityRefStruct.reference));

            // Entity 1 assertions.
            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(egid1).value, 1);
            Assert.AreEqual(
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(egid1).value, 3);

            entityRefPartially =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityReferenceParatiallySerialized>(egid1);
            // Assert.AreEqual(0, entityRefPartially.value);
            Assert.AreEqual(egid0, _neverDoThisIsJustForTheTest.entitiesDB.GetEGID(entityRefPartially.reference));

            entityRefStruct =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityReferenceSerialized>(egid1);
            Assert.AreEqual(int.MaxValue, entityRefStruct.value1);
            Assert.AreEqual(long.MaxValue, entityRefStruct.value2);
            Assert.AreEqual(egid1, _neverDoThisIsJustForTheTest.entitiesDB.GetEGID(entityRefStruct.reference));
        }

        [TestCase]
        public void TestSerializingWithStructLayout()
        {
            var egid0 = new EGID(0, NamedGroup1.Group);
            var init0 = _entityFactory.BuildEntity<SerializableEntityDescriptorWithLayout>(egid0);

            init0.Init(new EntityStructExplicitLayoutSerialized()
            {
                uintValue = 32,
                longValue = 512,
                intValue = -32
            });
            init0.Init(new EntityStructSequentialLayoutSerialized()
            {
                uintValue = 64,
                longValue = 1024,
                intValue = -64
            });

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes                    = new FasterList<byte>();
            var              generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var              simpleSerializationData  = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(egid0, simpleSerializationData, (int) SerializationType.Storage);

            _entityFunctions.RemoveEntitiesFromGroup(NamedGroup1.Group);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            simpleSerializationData.Reset();

            generateEntitySerializer.DeserializeNewEntity(egid0, simpleSerializationData, (int) SerializationType.Storage);

            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            // Entity 0 assertions.
            var explicitLayout =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructExplicitLayoutSerialized>(egid0);
            Assert.AreEqual(32, explicitLayout.uintValue);
            Assert.AreEqual(-32, explicitLayout.intValue);
            Assert.AreEqual(512, explicitLayout.longValue);

            var sequentialLayout =
                _neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSequentialLayoutSerialized>(egid0);
            Assert.AreEqual(64, sequentialLayout.uintValue);
            Assert.AreEqual(-64, sequentialLayout.intValue);
            Assert.AreEqual(1024, sequentialLayout.longValue);
        }

        EnginesRoot                       _enginesRoot;
        IEntityFactory                    _entityFactory;
        IEntityFunctions                  _entityFunctions;
        SimpleEntitiesSubmissionScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                        _neverDoThisIsJustForTheTest;

        public enum SerializationType
        {
            Network
          , Storage
        }

        class SerializableEntityDescriptor : SerializableEntityDescriptor<
            SerializableEntityDescriptor.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptor")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new ComponentBuilder<EntityStructNotSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized2>(
                        ((int) SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>())
                      , ((int) SerializationType.Network, new DefaultSerializer<EntityStructSerialized2>()))
                  , new SerializableComponentBuilder<SerializationType, EntityStructPartiallySerialized>(
                        ((int) SerializationType.Storage, new PartialSerializer<EntityStructPartiallySerialized>()))
                };
            }
        }

        class SerializableEntityDescriptorV0 : SerializableEntityDescriptor<
            SerializableEntityDescriptorV0.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorV0")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new ComponentBuilder<EntityStructNotSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized2>(
                        ((int) SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>()))
                  , new SerializableComponentBuilder<SerializationType, EntityStructPartiallySerialized>(
                        ((int) SerializationType.Storage, new PartialSerializer<EntityStructPartiallySerialized>()))
                };
            }
        }

        class SerializableEntityDescriptorV1 : SerializableEntityDescriptor<
            SerializableEntityDescriptorV1.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorV1")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new ComponentBuilder<EntityStructNotSerialized>()
                  , new SerializableComponentBuilder<EntityStructSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized2>(
                        ((int) SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>()))
                  , new SerializableComponentBuilder<SerializationType, EntityStructPartiallySerialized>(
                        ((int) SerializationType.Storage, new PartialSerializer<EntityStructPartiallySerialized>()))
                };
            }
        }

        class SerializableEntityDescriptorWithViews : SerializableEntityDescriptor<
            SerializableEntityDescriptorWithViews.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorWithView")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new ComponentBuilder<EntityViewStructNotSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized>()
                  , new SerializableComponentBuilder<SerializationType, EntityStructSerialized2>(
                        ((int) SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>())
                      , ((int) SerializationType.Network, new DefaultSerializer<EntityStructSerialized2>()))
                  , new SerializableComponentBuilder<SerializationType, EntityStructPartiallySerialized>(
                        ((int) SerializationType.Storage, new PartialSerializer<EntityStructPartiallySerialized>()))
                };
            }
        }

        class SerializableEntityDescriptorWithReferences : SerializableEntityDescriptor<
            SerializableEntityDescriptorWithReferences.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorWithReferences")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new SerializableComponentBuilder<SerializationType, EntityStructSerialized>()
                    , new SerializableComponentBuilder<SerializationType, EntityReferenceSerialized>()
                    , new SerializableComponentBuilder<SerializationType, EntityStructSerialized2>()
                    , new SerializableComponentBuilder<SerializationType, EntityReferenceParatiallySerialized>()
                };
            }
        }

        class SerializableEntityDescriptorWithLayout : SerializableEntityDescriptor<
            SerializableEntityDescriptorWithLayout.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorWithLayou")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IComponentBuilder[] componentsToBuild => ComponentsToBuild;

                static readonly IComponentBuilder[] ComponentsToBuild =
                {
                    new SerializableComponentBuilder<SerializationType, EntityStructExplicitLayoutSerialized>()
                    , new SerializableComponentBuilder<SerializationType, EntityStructSequentialLayoutSerialized>()
                };
            }
        }

        class TestEngine : IQueryingEntitiesEngine
        {
            public EntitiesDB entitiesDB { get; set; }
            public void       Ready()    { }
        }

        struct EntityStructNotSerialized : IEntityComponent { }

        struct EntityStructSerialized : IEntityComponent
        {
            public int value;
        }

        struct EntityStructSerialized2 : IEntityComponent
        {
            public int value;
        }

        struct EntityStructPartiallySerialized : IEntityComponent
        {
#pragma warning disable 169
            int value;
#pragma warning restore 169
            [PartialSerializerField] public int value1;
        }

        struct EntityViewStructNotSerialized : IEntityViewComponent
        {
#pragma warning disable 649
            public ITestIt TestIt;
#pragma warning restore 649

            public EGID ID { get; set; }
        }

        struct EntityReferenceSerialized : IEntityComponent
        {
            public int value1;
            public EntityReference reference;
            public long value2;
        }

        struct EntityReferenceParatiallySerialized : IEntityComponent
        {
            public long value;
            [PartialSerializerField] public EntityReference reference;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct EntityStructExplicitLayoutSerialized : IEntityComponent
        {
            [FieldOffset(4)] public long longValue;
            [FieldOffset(0)] public uint uintValue;
            [FieldOffset(12)] public int intValue;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        struct EntityStructSequentialLayoutSerialized : IEntityComponent
        {
            public uint uintValue;
            public long longValue;
            public int intValue;
        }

        interface ITestIt
        {
            float value { get; set; }
        }

        class Implementor : ITestIt
        {
            public Implementor(int i) { value = i; }

            public float value { get; set; }
        }

        class DeserializationFactory : IDeserializationFactory
        {
            public EntityInitializer BuildDeserializedEntity
            (EGID egid, ISerializationData serializationData, ISerializableEntityDescriptor entityDescriptor
           , int serializationType, IEntitySerialization entitySerialization, IEntityFactory factory
           , bool enginesRootIsDeserializationOnly)
            {
                var initializer =
                    factory.BuildEntity<SerializableEntityDescriptorWithViews>(egid, new[] {new Implementor(1)});

                entitySerialization.DeserializeEntityComponents(serializationData, entityDescriptor, ref initializer
                                                              , (int) SerializationType.Storage);

                return initializer;
            }
        }
    }
}
