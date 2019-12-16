﻿using System;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        uint Count { get; }
        ITypeSafeDictionary Create();

        void AddEntitiesToEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDb,
            ITypeSafeDictionary realDic, in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group);

        void RemoveEntitiesFromEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group);

        void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId);

        void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler);

        void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup);
        void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler);

        void SetCapacity(uint size);
        void Trim();
        void Clear();
        void FastClear();
    }
    
    public interface ITypeSafeDictionary<TValue>:ITypeSafeDictionary where TValue : struct, IEntityStruct
    {
        bool ContainsKey(uint egidEntityId);
        void Add(uint egidEntityId, in TValue entityView);
        ref TValue GetValueByRef(uint valueKey);
        TValue this[uint idEntityId] { get; set; }
        uint GetIndex(uint valueEntityId);
        bool TryGetValue(uint entityId, out TValue o);
        bool TryFindIndex(uint entityGidEntityId, out uint index);
        ref TValue GetDirectValue(uint findElementIndex);
        ref TValue GetOrCreate(uint idEntityId);
        FasterDictionaryStruct<uint,TValue> ToStruct();
        TValue[] GetValuesArray(out uint count);
        FasterDictionaryStruct<uint, TValue>.FasterDictionaryKeyValueEnumerator GetEnumerator();
        TValue[] unsafeValues { get; }
    }

    sealed class TypeSafeDictionary<TValue>: ITypeSafeDictionary<TValue> where TValue : struct, IEntityStruct
    {
        static readonly Type   _type     = typeof(TValue);
        static readonly string _typeName = _type.Name;
        static readonly bool   _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);

        public TypeSafeDictionary(uint size)
        {
            _implementation = new FasterDictionaryStruct<uint, TValue>(size);
        }

        public TypeSafeDictionary()
        {
            _implementation = new FasterDictionaryStruct<uint, TValue>(1);
        }
        
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as ITypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref tuple.Value, new EGID(tuple.Key, groupId));

                    _implementation.Add(tuple.Key, tuple.Value);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                            .FastConcat(typeof(TValue).ToString()).FastConcat(", group ").FastConcat(groupId).FastConcat(", id ").FastConcat(tuple.Key), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            ITypeSafeDictionary realDic, in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            var typeSafeDictionary = realDic as ITypeSafeDictionary<TValue>;

            //this can be optimized, should pass all the entities and not restart the process for each one
            foreach (var value in _implementation)
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                    in profiler, new EGID(value.Key, group));
        }

        public void RemoveEntitiesFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            foreach (var value in _implementation)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref _implementation.GetValueByRef(value.Key), null, in profiler,
                    new EGID(value.Key, group));
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FastClear()
        {
            _implementation.FastClear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler)
        {
            _implementation.Remove(fromEntityGid.entityID);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetCapacity(uint size)
        {
            _implementation.SetCapacity(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Trim()
        {
            _implementation.Trim();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _implementation.Clear();
        }

        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                ref var entity = ref _implementation.unsafeValues[(int) valueIndex];

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID);

                toGroupCasted.Add(fromEntityGid.entityID, entity);
            }
        }

        public void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler)
        {
            var valueIndex = _implementation.GetIndex(fromEntityGid.entityID);
            
            ref var entity = ref _implementation.unsafeValues[(int) valueIndex];

            if (toGroup != null)
            {
                RemoveEntityViewFromEngines(engines, ref entity, fromEntityGid.groupID, in profiler,
                    fromEntityGid);

                var toGroupCasted = toGroup as ITypeSafeDictionary<TValue>;
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid) SetEGIDWithoutBoxing<TValue>.SetIDWithoutBoxing(ref entity, toEntityID.Value);

                var index = toGroupCasted.GetIndex(toEntityID.Value.entityID);

                AddEntityViewToEngines(engines, ref toGroupCasted.unsafeValues[(int) index], previousGroup,
                    in profiler, toEntityID.Value);
            }
            else
                RemoveEntityViewFromEngines(engines, ref entity, null, in profiler, fromEntityGid);
        }

        public uint Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ITypeSafeDictionary Create()
        {
            return new TypeSafeDictionary<TValue>();
        }

        void AddEntityViewToEngines(FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            ref TValue entity, ExclusiveGroup.ExclusiveGroupStruct? previousGroup,
            in PlatformProfiler profiler, EGID egid)
        {
            //get all the engines linked to TValue
            if (!entityViewEnginesDB.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Add(ref entity, egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                        {
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedTo(ref entity, previousGroup.Value,
                                egid);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside MovedTo callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
        }

        static void RemoveEntityViewFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> @group, ref TValue entity,
            ExclusiveGroup.ExclusiveGroupStruct? previousGroup, in PlatformProfiler profiler, EGID egid)
        {
            if (!@group.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

            if (previousGroup == null)
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnAddAndRemove<TValue>).Remove(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#if SEEMS_UNNECESSARY            
            else
            {
                for (var i = 0; i < entityViewsEngines.Count; i++)
                    try
                    {
                        using (profiler.Sample(entityViewsEngines[i], _typeName))
                            (entityViewsEngines[i] as IReactOnSwap<TValue>).MovedFrom(ref entity, egid);
                    }
                    catch (Exception e)
                    {
                        throw new ECSException(
                            "Code crashed inside Remove callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TValue[] GetValuesArray(out uint count)
        {
            return _implementation.GetValuesArray(out count);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(uint egidEntityId)
        {
            return _implementation.ContainsKey(egidEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(uint egidEntityId, in TValue entityView) 
        {
            _implementation.Add(egidEntityId, entityView);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterDictionaryStruct<uint, TValue>.FasterDictionaryKeyValueEnumerator GetEnumerator()
        {
            return _implementation.GetEnumerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetValueByRef(uint valueKey)
        {
            return ref _implementation.GetValueByRef(valueKey);
        }

        public TValue this[uint idEntityId]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation[idEntityId];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => _implementation[idEntityId] = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetIndex(uint valueEntityId)
        {
            return _implementation.GetIndex(valueEntityId);
        }

        public TValue[] unsafeValues
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _implementation.unsafeValues;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(uint entityId, out TValue o)
        {
            return _implementation.TryGetValue(entityId, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetOrCreate(uint idEntityId)
        {
            return ref _implementation.GetOrCreate(idEntityId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public FasterDictionaryStruct<uint, TValue> ToStruct()
        {
            return _implementation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryFindIndex(uint entityId, out uint u)
        {
            return _implementation.TryFindIndex(entityId, out u);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref TValue GetDirectValue(uint findElementIndex) 
        {
            return ref _implementation.GetDirectValue(findElementIndex);
        }
        
        FasterDictionaryStruct<uint, TValue> _implementation;
    }
}