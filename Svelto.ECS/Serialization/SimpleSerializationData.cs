using Svelto.DataStructures;

namespace Svelto.ECS
{
    public class SimpleSerializationData : ISerializationData
    {
        public uint dataPos { get; set; }
        public FasterList<byte> data { get; set; }

        public SimpleSerializationData(FasterList<byte> d)
        {
            data = d;
        }

        public virtual void ResetWithNewData(FasterList<byte> newData)
        {
            dataPos = 0;

            data = newData;
        }

        public virtual void ResetToReuse()
        {
            dataPos = 0;

            data.ResetToReuse();
        }

        public virtual void BeginNextEntityStruct()
        {}
    }
}