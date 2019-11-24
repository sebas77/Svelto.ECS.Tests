using Svelto.DataStructures;

namespace Svelto.ECS
{
    public interface ISerializationData
    {
        uint dataPos { get; set; }
        FasterList<byte> data { get; }

        void Reuse();
        void Reset();
        void BeginNextEntityStruct();
    }
}