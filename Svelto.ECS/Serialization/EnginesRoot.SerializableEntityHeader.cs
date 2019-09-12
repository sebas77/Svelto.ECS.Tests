using Svelto.DataStructures;

namespace Svelto.ECS
{
    public partial class EnginesRoot
    {
        struct SerializableEntityHeader
        {
            public readonly uint descriptorHash;
            public readonly byte entityStructsCount;

            const uint SIZE = 4 + 4 + 4 + 1;

            internal SerializableEntityHeader(uint descriptorHash_, EGID egid_, byte entityStructsCount_)
            {
                entityStructsCount = entityStructsCount_;
                descriptorHash = descriptorHash_;
                egid = egid_;
            }

            internal SerializableEntityHeader(in FasterReadOnlyList<byte> data, ref uint dataPos)
            {
                descriptorHash = (uint)
                    (data[dataPos++]
                     | data[dataPos++] << 8
                     | data[dataPos++] << 16
                     | data[dataPos++] << 24);

                uint entityID = (uint)
                    (data[dataPos++]
                     | data[dataPos++] << 8
                     | data[dataPos++] << 16
                     | data[dataPos++] << 24);

                uint groupID = (uint)
                    (data[dataPos++]
                     | data[dataPos++] << 8
                     | data[dataPos++] << 16
                     | data[dataPos++] << 24);

                entityStructsCount = data[dataPos];
                dataPos++;

                egid = new EGID(entityID, new ExclusiveGroup.ExclusiveGroupStruct(groupID));
            }

            internal void Copy(FasterList<byte> data)
            {
                int dataPos = data.Count;
                data.ExpandBy(SIZE);

                // Splitting the descriptorHash_ (uint, 32 bit) into four bytes.
                data[dataPos++] = (byte) (descriptorHash & 0xff);
                data[dataPos++] = (byte) ((descriptorHash >> 8) & 0xff);
                data[dataPos++] = (byte) ((descriptorHash >> 16) & 0xff);
                data[dataPos++] = (byte) ((descriptorHash >> 24) & 0xff);

                // Splitting the entityID (uint, 32 bit) into four bytes.
                uint entityID = egid.entityID;
                data[dataPos++] = (byte) (entityID & 0xff);
                data[dataPos++] = (byte) ((entityID >> 8) & 0xff);
                data[dataPos++] = (byte) ((entityID >> 16) & 0xff);
                data[dataPos++] = (byte) ((entityID >> 24) & 0xff);

                // Splitting the groupID (uint, 32 bit) into four bytes.
                uint groupID = egid.groupID;
                data[dataPos++] = (byte) (groupID & 0xff);
                data[dataPos++] = (byte) ((groupID >> 8) & 0xff);
                data[dataPos++] = (byte) ((groupID >> 16) & 0xff);
                data[dataPos++] = (byte) ((groupID >> 24) & 0xff);

                data[dataPos] = entityStructsCount;
            }

            internal readonly EGID egid; //this can't be used safely!
        }
    }
}
