namespace Svelto.ECS.Serialization
{
    public interface ISerializer<T>
        where T : unmanaged, IEntityStruct
    {
        bool Serialize(in T value, ISerializationData serializationData);
        bool Deserialize(ref T value, ISerializationData serializationData);

        uint size { get; }
    }
}
