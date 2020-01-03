using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS;
using Svelto.ECS.Hybrid;
using Svelto.ECS.Serialization;

namespace UnitTests
{
    [TestFixture]
    public class TestSveltoECSSerialisation
    {
        class NamedGroup1 : NamedExclusiveGroup<NamedGroup1> {}

        [SetUp]
        public void Init()
        {
            _simpleSubmissionEntityViewScheduler = new SimpleSubmissionEntityViewScheduler();
            _enginesRoot                         = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest         = new TestEngine();

            _enginesRoot.AddEngine(_neverDoThisIsJustForTheTest);

            _entityFactory   = _enginesRoot.GenerateEntityFactory();
            _entityFunctions = _enginesRoot.GenerateEntityFunctions();
        }

        [TestCase]
        public void TestSerializingToByteArrayRemoveGroup()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value = 5 });
            init.Init(new EntityStructSerialized2() { value = 4 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 3 });
            init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(1, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value           = 4 });
            init.Init(new EntityStructSerialized2() { value = 3 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 2 });
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes = new FasterList<byte>();
            var generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var simpleSerializationData = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);
            generateEntitySerializer.SerializeEntity(new EGID(1, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);
            
            _entityFunctions.RemoveGroupAndEntities(NamedGroup1.Group);
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
            
            simpleSerializationData.Reset();

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(0, NamedGroup1.Group).value, Is.EqualTo(5));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1, Is.EqualTo(3));
            
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(1, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(1, NamedGroup1.Group).value, Is.EqualTo(3));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(1, NamedGroup1.Group).value1, Is.EqualTo(2));
        }
        
        [TestCase]
        public void TestSerializingToByteArrayNewEnginesRoot()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value = 5 });
            init.Init(new EntityStructSerialized2() { value = 4 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 3 });
            init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(1, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value           = 4 });
            init.Init(new EntityStructSerialized2() { value = 3 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 2 });
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes = new FasterList<byte>();
            var generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var simpleSerializationData = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);
            generateEntitySerializer.SerializeEntity(new EGID(1, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);

            _enginesRoot.Dispose();
            var newEnginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            
            _neverDoThisIsJustForTheTest = new TestEngine();
            
            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);
            
            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(0, NamedGroup1.Group).value, Is.EqualTo(5));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1, Is.EqualTo(3));
            
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(1, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(1, NamedGroup1.Group).value, Is.EqualTo(3));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(1, NamedGroup1.Group).value1, Is.EqualTo(2));
        }
        
        [TestCase]
        public void TestSerializingWithEntityStructsWithVersioning()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptorV0>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized2() { value = 4 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 3 });
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes = new FasterList<byte>();
            var generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var simpleSerializationData = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);

            _enginesRoot.Dispose();
            var newEnginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();
            
            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);
            
            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();
            
            //needed for the versioning to work
            generateEntitySerializer.RegisterSerializationFactory<SerializableEntityDescriptorV0>(new DefaultVersioningFactory<SerializableEntityDescriptorV1>(newEnginesRoot.GenerateEntityFactory()));

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1, Is.EqualTo(3));
        }
        
        [TestCase]
        public void TestSerializingWithEntityViewStructsAndFactories()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptorWithViews>(0, NamedGroup1.Group, new []{new Implementor(1)});
            init.Init(new EntityStructSerialized() { value = 5 });
            init.Init(new EntityStructSerialized2() { value = 4 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 3 });
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            FasterList<byte> bytes = new FasterList<byte>();
            var generateEntitySerializer = _enginesRoot.GenerateEntitySerializer();
            var simpleSerializationData = new SimpleSerializationData(bytes);
            generateEntitySerializer.SerializeEntity(new EGID(0, NamedGroup1.Group), 
                                                     simpleSerializationData, SerializationType.Storage);

            _enginesRoot.Dispose();
            var newEnginesRoot = new EnginesRoot(_simpleSubmissionEntityViewScheduler);
            _neverDoThisIsJustForTheTest = new TestEngine();
            
            newEnginesRoot.AddEngine(_neverDoThisIsJustForTheTest);
            
            simpleSerializationData.Reset();
            generateEntitySerializer = newEnginesRoot.GenerateEntitySerializer();
            DeserializationFactory factory = new DeserializationFactory(newEnginesRoot.GenerateEntityFactory());
            generateEntitySerializer.RegisterSerializationFactory<SerializableEntityDescriptorWithViews>(factory);

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();

            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized>(0, NamedGroup1.Group).value, Is.EqualTo(5));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructSerialized2>(0, NamedGroup1.Group).value, Is.EqualTo(4));
            Assert.That(_neverDoThisIsJustForTheTest.entitiesDB.QueryEntity<EntityStructPartiallySerialized>(0, NamedGroup1.Group).value1, Is.EqualTo(3));
        }

        EnginesRoot                         _enginesRoot;
        IEntityFactory                      _entityFactory;
        IEntityFunctions                    _entityFunctions;
        SimpleSubmissionEntityViewScheduler _simpleSubmissionEntityViewScheduler;
        TestEngine                          _neverDoThisIsJustForTheTest;

        class SerializableEntityDescriptor : SerializableEntityDescriptor<
                SerializableEntityDescriptor.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptor")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IEntityBuilder[] entitiesToBuild => _entitiesToBuild;
                
                static readonly IEntityBuilder[] _entitiesToBuild = {
                        new EntityBuilder<EntityStructNotSerialized>(),    
                        new SerializableEntityBuilder<EntityStructSerialized>(),
                        new SerializableEntityBuilder<EntityStructSerialized2>
                            ((SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>()) ,
                             (SerializationType.Network, new DefaultSerializer<EntityStructSerialized2>())),
                        new SerializableEntityBuilder<EntityStructPartiallySerialized>
                            ((SerializationType.Storage, new PartialSerializer <EntityStructPartiallySerialized>())
                        )
                };
            }
        }
        
        class SerializableEntityDescriptorV0 : SerializableEntityDescriptor<
            SerializableEntityDescriptorV0.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorV0")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IEntityBuilder[] entitiesToBuild => _entitiesToBuild;
                
                static readonly IEntityBuilder[] _entitiesToBuild = {
                    new EntityBuilder<EntityStructNotSerialized>(),    
                    new SerializableEntityBuilder<EntityStructSerialized2>
                        ((SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>())),
                    new SerializableEntityBuilder<EntityStructPartiallySerialized>
                        ((SerializationType.Storage, new PartialSerializer <EntityStructPartiallySerialized>())
                        )
                };
            }
        }
        
        class SerializableEntityDescriptorV1 : SerializableEntityDescriptor<
            SerializableEntityDescriptorV1.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorV1")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IEntityBuilder[] entitiesToBuild => _entitiesToBuild;
                
                static readonly IEntityBuilder[] _entitiesToBuild = {
                    new EntityBuilder<EntityStructNotSerialized>(),    
                    new SerializableEntityBuilder<EntityStructSerialized>(),
                    new SerializableEntityBuilder<EntityStructSerialized2>
                        ((SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>())),
                    new SerializableEntityBuilder<EntityStructPartiallySerialized>
                        ((SerializationType.Storage, new PartialSerializer <EntityStructPartiallySerialized>())
                        )
                };
            }
        }
        
        class SerializableEntityDescriptorWithViews : SerializableEntityDescriptor<
            SerializableEntityDescriptorWithViews.DefaultPatternForEntityDescriptor>
        {
            [HashName("DefaultPatternForEntityDescriptorWithView")]
            internal class DefaultPatternForEntityDescriptor : IEntityDescriptor
            {
                public IEntityBuilder[] entitiesToBuild => _entitiesToBuild;
                
                static readonly IEntityBuilder[] _entitiesToBuild = {
                    new EntityBuilder<EntityViewStructNotSerialized>(),    
                    new SerializableEntityBuilder<EntityStructSerialized>(),
                    new SerializableEntityBuilder<EntityStructSerialized2>
                        ((SerializationType.Storage, new DefaultSerializer<EntityStructSerialized2>()) ,
                         (SerializationType.Network, new DefaultSerializer<EntityStructSerialized2>())),
                    new SerializableEntityBuilder<EntityStructPartiallySerialized>
                        ((SerializationType.Storage, new PartialSerializer <EntityStructPartiallySerialized>())
                        )
                };
            }
        }
        
        class TestEngine: IQueryingEntitiesEngine
        {
            public IEntitiesDB entitiesDB { get; set; }
            public void        Ready()    {}

            public bool HasEntity<T>(EGID ID) where T : struct, IEntityStruct
            {
                return entitiesDB.Exists<T>(ID);
            }

            public bool HasAnyEntityInGroup<T>(ExclusiveGroup groupID) where T : struct, IEntityStruct
            {
                entitiesDB.QueryEntities<T>(groupID).ToFastAccess(out var count);
                return count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(ExclusiveGroup groupID) where T: struct, IEntityStruct
            {
                entitiesDB.QueryEntities<T>(groupID).ToFastAccess(out var count);

                return count != 0;
            }
        }
        
        struct EntityStructNotSerialized : IEntityStruct
        {
        }

        struct EntityStructSerialized : IEntityStruct
        {
            public int value;
        }
        
        struct EntityStructSerialized2 : IEntityStruct
        {
            public int value;
        }
    
        struct EntityStructPartiallySerialized : IEntityStruct
        {
            int value;
            [PartialSerializerField] public int value1;
        }
        
        struct EntityViewStructNotSerialized : IEntityViewStruct
        {
#pragma warning disable 649
            public ITestIt TestIt;
#pragma warning restore 649
            
            public EGID ID { get; set; }
        }
        
        interface ITestIt
        {
            float value { get; set; }
        }
        
        class Implementor : ITestIt
        {
            public Implementor(int i)
            {
                value = i;
            }

            public float value { get; set; }
        }
        class DeserializationFactory : IDeserializationFactory
        {
            readonly IEntityFactory _factory;

            public EntityStructInitializer BuildDeserializedEntity(EGID                          egid,
                                                                   ISerializationData            serializationData,
                                                                   ISerializableEntityDescriptor entityDescriptor,
                                                                   SerializationType             serializationType,
                                                                   IEntitySerialization          entitySerialization)
            {
                var initializer = _factory.BuildEntity<SerializableEntityDescriptorWithViews>(egid, new []{new Implementor(1)});
                
                entitySerialization.DeserializeEntityStructs(serializationData, entityDescriptor, ref initializer, SerializationType.Storage);

                return initializer;
            }

            public DeserializationFactory(IEntityFactory factory) { _factory = factory; }
        }
    }
}
