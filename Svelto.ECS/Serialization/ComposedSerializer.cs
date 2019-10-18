namespace Svelto.ECS.Serialization
{
    public class ComposedSerializer<T, X, Y> : ISerializer<T>
        where T : unmanaged, IEntityStruct
        where X : class, ISerializer<T>, new()
        where Y : class, ISerializer<T>, new()
    {
        public ComposedSerializer()
        {
            _serializers = new ISerializer<T>[2];
            _serializers[0] = new X();
            _serializers[1] = new Y();
        }

        public bool Serialize(in T value, ISerializationData serializationData)
        {
            foreach (ISerializer<T> s in _serializers)
            {
                serializationData.data.ExpandBy(s.size);
                if (s.Serialize(value, serializationData))
                    return true;
            }

            return false;
        }

        public bool Deserialize(ref T value, ISerializationData serializationData)
        {
            foreach (ISerializer<T> s in _serializers)
            {
                if (s.Deserialize(ref value, serializationData))
                    return true;
            }

            return false;
        }

        public uint size => 0;
        ISerializer<T>[] _serializers;
    }
}