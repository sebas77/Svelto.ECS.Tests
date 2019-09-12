using System;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.ECS.Internal;

namespace Svelto.ECS.Serialization
{
    public class SerializableEntityBuilder<T> : EntityBuilder<T>, ISerializableEntityBuilder
        where T : unmanaged, IEntityStruct
    {
        public static readonly uint SIZE = UnsafeUtils.SizeOf<T>();

        internal T _lastSerialisedValue; // TODO: I have to think this through

        static SerializableEntityBuilder()
        {}

        public SerializableEntityBuilder()
        {
            _serializers = new ISerializer<T>[(int) SerializationType.Length];
            for (int i = 0; i < (int) SerializationType.Length; i++)
            {
                _serializers[i] = new DefaultSerializer<T>();
            }
        }

        public SerializableEntityBuilder(params ValueTuple<SerializationType, ISerializer<T>>[] serializers)
        {
            _serializers = new ISerializer<T>[(int) SerializationType.Length];
            for (int i = 0; i < serializers.Length; i++)
            {
                ref (SerializationType, ISerializer<T>) s = ref serializers[i];
                _serializers[(int) s.Item1] = s.Item2;
            }

            // Just in case the above are the same type
            for (int i = 0; i < (int) SerializationType.Length; i++)
            {
                if (_serializers[i] == null)
                {
                    _serializers[i] = new DefaultSerializer<T>();
                }
            }
        }

        public void Serialize(uint entityID, ITypeSafeDictionary dictionary, FasterList<byte> data,
            in SerializationType serializationType)
        {
            uint dataPos = (uint) data.Count;
            ISerializer<T> serializer = _serializers[(int) serializationType];

            var safeDictionary = (TypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Serialization failed");
            }

            data.ExpandBy(serializer.size);

            T[] values = safeDictionary.GetValuesArray(out _);

            byte[] arrayFast = data.ToArrayFast();
            serializer.Serialize(values[index], arrayFast, ref dataPos);
        }

        public void Deserialize(uint entityID, ITypeSafeDictionary dictionary, in FasterReadOnlyList<byte> data,
            ref uint dataPos, in SerializationType serializationType)
        {
            byte[] arrayFast = data.ToArrayFast();
            ISerializer<T> serializer = _serializers[(int) serializationType];

            // Handle the case when an entity struct is gone
            var safeDictionary = (TypeSafeDictionary<T>) dictionary;
            if (safeDictionary.TryFindIndex(entityID, out uint index) == false)
            {
                throw new ECSException("Entity Deserialization failed");
            }

            T[] values = safeDictionary.GetValuesArray(out _);

            serializer.Deserialize(ref values[index], arrayFast, ref dataPos);
        }

        void ISerializableEntityBuilder.Deserialize(in FasterReadOnlyList<byte> data, ref uint dataPos,
            in SerializationType serializationType, in EntityStructInitializer initializer)
        {
            byte[] arrayFast = data.ToArrayFast();
            ISerializer<T> serializer = _serializers[(int) serializationType];

            _lastSerialisedValue = initializer.Get<T>();

            serializer.Deserialize(ref _lastSerialisedValue, arrayFast, ref dataPos);
        }

        public void Set(ref EntityStructInitializer initializer)
        {
            initializer.Init(_lastSerialisedValue);
        }

        readonly ISerializer<T>[] _serializers;
    }
}