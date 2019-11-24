using NUnit.Framework;
using Svelto.DataStructures;
using Svelto.ECS;
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
        public void TestSerializingToByteArray()
        {
            var init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(0, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value = 5 });
            init.Init(new EntityStructPartiallySerialized() { value1 = 3 });
            init = _entityFactory.BuildEntity<SerializableEntityDescriptor>(1, NamedGroup1.Group);
            init.Init(new EntityStructSerialized() { value           = 4 });
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

            generateEntitySerializer.DeserializeNewEntity(new EGID(0, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            generateEntitySerializer.DeserializeNewEntity(new EGID(1, NamedGroup1.Group), simpleSerializationData,
                                                          SerializationType.Storage);
            
            _simpleSubmissionEntityViewScheduler.SubmitEntities();
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
                        new SerializableEntityBuilder<EntityStructSerialized>
                            ((SerializationType.Storage, new DefaultSerializer<EntityStructSerialized>()) ,
                             (SerializationType.Network, new DefaultSerializer<EntityStructSerialized>())),
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
                entitiesDB.QueryEntities<T>(groupID, out var count);
                return count > 0;
            }

            public bool HasAnyEntityInGroupArray<T>(ExclusiveGroup groupID) where T: struct, IEntityStruct
            {
                entitiesDB.QueryEntities<T>(groupID, out var count);

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
    
        struct EntityStructPartiallySerialized : IEntityStruct
        {
            int value;
            [PartialSerializerField] public int value1;
        }
    }
}
