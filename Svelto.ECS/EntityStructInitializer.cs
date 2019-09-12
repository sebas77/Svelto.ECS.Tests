using System;
using Svelto.DataStructures.Experimental;
using Svelto.ECS.Internal;

namespace Svelto.ECS
{
    public ref struct EntityStructInitializer
    {
        public EntityStructInitializer(EGID id, FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> group)
        {
            _group = group;
            _ID = id;
        }

        public void Init<T>(T initializer) where T : struct, IEntityStruct
        {
            if (!_group.TryGetValue(new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE),
                out var typeSafeDictionary))
                return;

            var dictionary = (TypeSafeDictionary<T>) typeSafeDictionary;

            if (EntityBuilder<T>.HAS_EGID) ((INeedEGID) initializer).ID = _ID;

            if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                dictionary.GetDirectValue(findElementIndex) = initializer;
        }

        public T Get<T>() where T : struct, IEntityStruct
        {
            if (_group != null && _group.TryGetValue(new RefWrapper<Type>(EntityBuilder<T>.ENTITY_VIEW_TYPE),
                    out var typeSafeDictionary))
            {
                var dictionary = (TypeSafeDictionary<T>) typeSafeDictionary;

                if (dictionary.TryFindIndex(_ID.entityID, out var findElementIndex))
                    return dictionary.GetDirectValue(findElementIndex);
            }

            return new T();
        }

        readonly EGID                                                    _ID;
        readonly FasterDictionary<RefWrapper<Type>, ITypeSafeDictionary> _group;
    }
}