#if UNITY_5 || UNITY_5_3_OR_NEWER
using System;
using System.Collections.Generic;
using Svelto.ECS.Hybrid;
using UnityEngine;

namespace Svelto.ECS.Extensions.Unity
{
    public static class EntityDescriptorHolderHelper
    {
        public static EntityComponentInitializer CreateEntity<T>(this Transform contextHolder, EGID ID,
                                                           IEntityFactory factory, out T holder)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
        
        public static EntityComponentInitializer Create<T>(this Transform contextHolder, EGID ID,
                                                           IEntityFactory factory)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder       = contextHolder.GetComponentInChildren<T>(true);
            var implementors = holder.GetComponents<IImplementor>();

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
    }
    
    public static class SveltoGUIHelper
    {
        public static T CreateFromPrefab<T>(ref uint startIndex, Transform contextHolder, IEntityFactory factory,
            ExclusiveGroup group, bool searchImplementorsInChildren = false, string groupNamePostfix = null) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            Create<T>(new EGID(startIndex++, group), contextHolder, factory, out var holder);
            var children = contextHolder.GetComponentsInChildren<IEntityDescriptorHolder>(true);

            foreach (var child in children)
            {
                IImplementor[] childImplementors;
                if (child.GetType() != typeof(T))
                {
                    var monoBehaviour = child as MonoBehaviour;
                    if (searchImplementorsInChildren == false)
                        childImplementors = monoBehaviour.GetComponents<IImplementor>();
                    else
                        childImplementors = monoBehaviour.GetComponentsInChildren<IImplementor>(true);
                    startIndex = InternalBuildAll(
                        startIndex,
                        child,
                        factory,
                        group,
                        childImplementors,
                        groupNamePostfix);
                }
            }

            return holder;
        }

        public static EntityComponentInitializer Create<T>(EGID ID, Transform contextHolder,
            IEntityFactory factory, out T holder, bool searchImplementorsInChildren = false)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            holder = contextHolder.GetComponentInChildren<T>(true);
            if (holder == null)
            {
                throw new Exception($"Could not find holder {typeof(T).Name} in {contextHolder.name}");
            }
            var implementors = searchImplementorsInChildren == false ? holder.GetComponents<IImplementor>() : holder.GetComponentsInChildren<IImplementor>(true) ;

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }
        
        public static EntityComponentInitializer Create<T>(EGID ID, Transform contextHolder,
                                                           IEntityFactory factory, bool searchImplementorsInChildren = false)
            where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holder       = contextHolder.GetComponentInChildren<T>(true);
            if (holder == null)
            {
                throw new Exception($"Could not find holder {typeof(T).Name} in {contextHolder.name}");
            }
            var implementors = searchImplementorsInChildren == false ? holder.GetComponents<IImplementor>() : holder.GetComponentsInChildren<IImplementor>(true) ;

            return factory.BuildEntity(ID, holder.GetDescriptor(), implementors);
        }

        public static uint CreateAll<T>(uint startIndex, ExclusiveGroup group,
            Transform contextHolder, IEntityFactory factory, string groupNamePostfix = null) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                var implementors = holder.GetComponents<IImplementor>();
                startIndex = InternalBuildAll(startIndex, holder, factory, group, implementors, groupNamePostfix);
            }

            return startIndex;
        }

        static uint InternalBuildAll(uint startIndex, IEntityDescriptorHolder descriptorHolder,
            IEntityFactory factory, ExclusiveGroup group, IImplementor[] implementors, string groupNamePostfix)
        {
            ExclusiveGroupStruct realGroup = group;

            if (string.IsNullOrEmpty(descriptorHolder.groupName) == false)
            {
                realGroup = ExclusiveGroup.Search(!string.IsNullOrEmpty(groupNamePostfix)
                    ? $"{descriptorHolder.groupName}{groupNamePostfix}"
                    : descriptorHolder.groupName);
            }

            EGID egid;
            var holderId = descriptorHolder.id;
            if (holderId == 0)
                egid = new EGID(startIndex++, realGroup);
            else
                egid = new EGID(holderId, realGroup);

            var init = factory.BuildEntity(egid, descriptorHolder.GetDescriptor(), implementors);

            init.Init(new EntityHierarchyComponent(group));

            return startIndex;
        }

        /// <summary>
        /// Works like CreateAll but only builds entities with holders that have the same group specified
        /// </summary>
        /// <param name="startId"></param>
        /// <param name="group">The group to match</param>
        /// <param name="contextHolder"></param>
        /// <param name="factory"></param>
        /// <typeparam name="T">EntityDescriptorHolder type</typeparam>
        /// <returns>Next available ID</returns>
        public static uint CreateAllInMatchingGroup<T>(uint startId, ExclusiveGroup exclusiveGroup,
            Transform contextHolder, IEntityFactory factory) where T : MonoBehaviour, IEntityDescriptorHolder
        {
            var holders = contextHolder.GetComponentsInChildren<T>(true);

            foreach (var holder in holders)
            {
                if (string.IsNullOrEmpty(holder.groupName) == false)
                {
                    var realGroup = ExclusiveGroup.Search(holder.groupName);
                    if (realGroup != exclusiveGroup)
                        continue;
                }
                else
                {
                    continue;
                }

                var implementors = holder.GetComponents<IImplementor>();

                startId = InternalBuildAll(startId, holder, factory, exclusiveGroup, implementors, null);
            }

            return startId;
        }
        
        public delegate void CustomInitializer(EntityComponentInitializer init, IEntityDescriptor descriptor);

        // Start at 1000 to not collide with IDs specified in holders, which usually come from enums
        private static uint _nextGeneratedId = 1000;

        // Use this to build GUI entities which are entirely described in prefab, in a single call.
        // Use custom initializer and check descriptor type if you want to initialize extra stuff.
        // If your prefabs are nested within another one:
        // - You may want to instantiate them dynamically,
        // - Expect the parent to build yours,
        // - Or use a marker to stop recursion (not implemented)
        // If you want even more special building for your widget, better do it manually instead of using this helper.
        public static void BuildAll(
            IEntityFactory factory, Transform root, uint? customRootId = null, CustomInitializer customInitializer = null)
        {
            // Starting high to make sure we separate the range of generated IDs and the specified ones
            var holders = new List<IEntityDescriptorHolder>();
            var implementors = new List<IImplementor>();
            
            holders.Clear();
            root.GetComponentsInChildren(true, holders);

            if (holders.Count == 0)
            {
                throw new Exception($"Could not find any descriptor holder in {root.gameObject.name}");
            }

            for (uint iHolder = 0; iHolder < holders.Count; ++iHolder)
            {
                var holder = holders[(int)iHolder];
                var holderMb = (MonoBehaviour) holder;
                implementors.Clear();
                holderMb.GetComponents(implementors);

                if (string.IsNullOrEmpty(holder.groupName))
                {
                    throw new Exception($"Descriptor holder on game object {holderMb.gameObject.name} " +
                                        $"does not specify a group");
                }

                ExclusiveGroupStruct groupStruct;
                try
                {
                    groupStruct = ExclusiveGroup.Search(holder.groupName);
                }
                catch (Exception ex)
                {
                    // Rethrow with more context information to help debugging
                    throw new Exception($"Descriptor holder on game object {holderMb.gameObject.name} " +
                                        $"does not specify a valid group: \"{holder.groupName}\"", ex);
                }

                EGID egid;
                if (customRootId != null && holderMb.transform == root)
                {
                    // The ID is hardcoded
                    egid = new EGID(holder.id, groupStruct);
                }
                else if (holder.id != 0)
                {
                    // The ID is specified in prefab
                    egid = new EGID(holder.id, groupStruct);
                }
                else
                {
                    // The ID does not matter, generate a new one
                    egid = new EGID(_nextGeneratedId++, groupStruct);
                }

                var descriptor = holder.GetDescriptor();
                var init = factory.BuildEntity(egid, descriptor, implementors);

                // TODO May need a constructor taking as struct
                //init.Init(new EntityHierarchyComponent(groupStruct));

                if (customInitializer != null)
                {
                    customInitializer(init, descriptor);
                }
            }
        }
    }
}
#endif
