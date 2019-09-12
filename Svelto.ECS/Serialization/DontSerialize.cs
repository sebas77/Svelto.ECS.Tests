namespace Svelto.ECS.Serialization
{
    public class DontSerialize<T> : ISerializer<T> where T : unmanaged, IEntityStruct
    {
        public uint size => 0;
        
        public void Serialize(in T value, byte[] data, ref uint dataPos)
        {
        }

        public void Deserialize(ref T value, byte[] data, ref uint dataPos)
        {
        }
    }
}