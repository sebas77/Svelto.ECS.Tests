using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Svelto.Common;
using Svelto.DataStructures;
using Svelto.DataStructures.Experimental;
using Svelto.Utilities;

namespace Svelto.ECS.Internal
{
    public interface ITypeSafeDictionary
    {
        int Count { get; }
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
        bool Has(uint entityIdEntityId);
    }

    class TypeSafeDictionary<TValue> : FasterDictionary<uint, TValue>,
        ITypeSafeDictionary where TValue : struct, IEntityStruct
    {
        static readonly Type       _type     = typeof(TValue);
        static readonly          string     _typeName = _type.Name;
        static readonly          bool       _hasEgid  = typeof(INeedEGID).IsAssignableFrom(_type);
        
        public delegate void ActionCast(ref TValue target, EGID egid);
        static readonly          ActionCast Setter    = MakeSetter();
        public static ActionCast MakeSetter()
        {
            if (_hasEgid)
            {
                Type myTypeA = typeof(TValue);
                PropertyInfo myFieldInfo = myTypeA.GetProperty("ID");

                ParameterExpression targetExp = Expression.Parameter(typeof(TValue).MakeByRefType(), "target");
                ParameterExpression valueExp = Expression.Parameter(typeof(EGID), "value");
                MemberExpression fieldExp = Expression.Property(targetExp, myFieldInfo);
                BinaryExpression assignExp = Expression.Assign(fieldExp, valueExp);

                var setter = Expression.Lambda<ActionCast>(assignExp, targetExp, valueExp).Compile();

                return setter;
            }

            return null;
        }

        public TypeSafeDictionary(uint size) : base(size) {}
        public TypeSafeDictionary() {}
        
        public void AddEntitiesFromDictionary(ITypeSafeDictionary entitiesToSubmit, uint groupId)
        {
            var typeSafeDictionary = entitiesToSubmit as TypeSafeDictionary<TValue>;

            foreach (var tuple in typeSafeDictionary)
            {
                try
                {
                    if (_hasEgid) Setter(ref tuple.Value, new EGID(tuple.Key, groupId));

                    Add(tuple.Key, ref tuple.Value);
                }
                catch (Exception e)
                {
                    throw new TypeSafeDictionaryException(
                        "trying to add an EntityView with the same ID more than once Entity: "
                            .FastConcat(typeof(TValue).ToString()).FastConcat("id ").FastConcat(tuple.Key), e);
                }
            }
        }

        public void AddEntitiesToEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            ITypeSafeDictionary realDic, in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            var typeSafeDictionary = realDic as TypeSafeDictionary<TValue>;

            foreach (KeyValuePairFast<uint, TValue> value in this)
                AddEntityViewToEngines(entityViewEnginesDB, ref typeSafeDictionary.GetValueByRef(value.Key), null,
                    in profiler, new EGID(value.Key, group));
        }

        public void RemoveEntitiesFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB,
            in PlatformProfiler profiler, ExclusiveGroup.ExclusiveGroupStruct @group)
        {
            foreach (KeyValuePairFast<uint, TValue> value in this)
                RemoveEntityViewFromEngines(entityViewEnginesDB, ref GetValueByRef(value.Key), null, in profiler,
                    new EGID(value.Key, group));
        }

        public bool Has(uint entityIdEntityId)
        {
            return ContainsKey(entityIdEntityId);
        }

        public void RemoveEntityFromDictionary(EGID fromEntityGid, in PlatformProfiler profiler)
        {
            Remove(fromEntityGid.entityID);
        }

        public void AddEntityToDictionary(EGID fromEntityGid, EGID toEntityID, ITypeSafeDictionary toGroup)
        {
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                ref var entity = ref _values[valueIndex];

                if (_hasEgid)
                {
                    var needEgid = (INeedEGID) entity;
                    needEgid.ID = toEntityID;
                    entity = (TValue) needEgid;
                }

                toGroupCasted.Add(fromEntityGid.entityID, ref entity);
            }
        }

        public void MoveEntityFromEngines(EGID fromEntityGid, EGID? toEntityID, ITypeSafeDictionary toGroup,
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> engines, in PlatformProfiler profiler)
        {
            var valueIndex = GetValueIndex(fromEntityGid.entityID);

            if (toGroup != null)
            {
                RemoveEntityViewFromEngines(engines, ref _values[valueIndex], fromEntityGid.groupID, in profiler,
                    fromEntityGid);

                var toGroupCasted = toGroup as TypeSafeDictionary<TValue>;
                ref var entity = ref _values[valueIndex];
                var previousGroup = fromEntityGid.groupID;

                if (_hasEgid)
                {
                    var needEgid = (INeedEGID) entity;
                    needEgid.ID = toEntityID.Value;
                    entity = (TValue) needEgid;
                }

                var index = toGroupCasted.GetValueIndex(toEntityID.Value.entityID);

                AddEntityViewToEngines(engines, ref toGroupCasted._values[index], previousGroup,
                    in profiler, fromEntityGid);
            }
            else
                RemoveEntityViewFromEngines(engines, ref _values[valueIndex], null, in profiler, fromEntityGid);
        }

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
                            "Code crashed inside Add callback ".FastConcat(typeof(TValue).ToString()), e);
                    }
            }
        }

        static void RemoveEntityViewFromEngines(
            FasterDictionary<RefWrapper<Type>, FasterList<IEngine>> entityViewEnginesDB, ref TValue entity,
            ExclusiveGroup.ExclusiveGroupStruct? previousGroup, in PlatformProfiler profiler, EGID egid)
        {
            if (!entityViewEnginesDB.TryGetValue(new RefWrapper<Type>(_type), out var entityViewsEngines)) return;

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
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ref TValue FindElement(uint entityGidEntityId)
        {
#if DEBUG && !PROFILER
            if (TryFindIndex(entityGidEntityId, out var findIndex) == false)
                throw new Exception("Entity not found in this group ".FastConcat(typeof(TValue).ToString()));
#else
            TryFindIndex(entityGidEntityId, out var findIndex);
#endif
            return ref _values[findIndex];
        }
    }
}