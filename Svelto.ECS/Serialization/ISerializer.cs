
namespace Svelto.ECS.Serialization
{
    public interface ISerializer<T>
        where T : unmanaged, IEntityStruct
    {
        void Serialize(in T value, byte[] data, ref uint dataPos);
        void Deserialize(ref T value, byte[] data, ref uint dataPos);

        uint size { get; }
    }
}
