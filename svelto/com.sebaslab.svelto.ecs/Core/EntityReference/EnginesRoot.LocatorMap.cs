using System.Runtime.CompilerServices;
using Svelto.DataStructures;

namespace Svelto.ECS
{
    // The EntityLocatorMap provides a bidirectional map to help locate entities without using an EGID which might
    // change in runtime. The Entity Locator map uses a reusable unique identifier struct called EntityLocator to
    // find the last known EGID from last entity submission.
    public partial class EnginesRoot
    {
        void CreateReferenceLocator(EGID egid)
        {
            // Check if we need to create a new EntityLocator or whether we can recycle an existing one.s
            EntityReference reference;
            if (_nextReferenceIndex == _entityReferenceMap.count)
            {
                _entityReferenceMap.Add(new EntityLocatorMapElement(egid));
                reference = new EntityReference(_nextReferenceIndex++);
            }
            //if _nextEntityId is not equivalent to the count of entity references added so far, it is 
            //pointing to the first deleted entity. A tombstone system recycles the entityID field to point
            //it to the next deleted entity, similar to a linked list algorithm.
            else
            {
                ref var element = ref _entityReferenceMap[_nextReferenceIndex];
                reference = new EntityReference(_nextReferenceIndex, element.version);
                // The recycle entities form a linked list, using the egid.entityID to store the next element.
                _nextReferenceIndex = element.egid.entityID;
                element.egid  = egid;
            }

            // Update reverse map from egid to locator.
            var groupMap = _egidToReferenceMap.GetOrCreate(egid.groupID, () => new FasterDictionary<uint, EntityReference>());
            
            groupMap[egid.entityID] = reference;
        }

        void UpdateReferenceLocator(EGID from, EGID to)
        {
            var egidToReference = _egidToReferenceMap[@from.groupID];
            var reference       = egidToReference[from.entityID];
            egidToReference.Remove(from.entityID);

            _entityReferenceMap[reference.uniqueID].egid = to;
            
            var groupMap = _egidToReferenceMap.GetOrCreate(to.groupID, () => new FasterDictionary<uint, EntityReference>());

            groupMap[to.entityID] = reference;
        }

        void RemoveReferenceLocator(EGID egid)
        {
            var egidToReference = _egidToReferenceMap[@egid.groupID];
            var reference       = egidToReference[egid.entityID];
            egidToReference.Remove(egid.entityID);

            // Invalidate the entity locator element by bumping its version and setting the egid to point to a unexisting element.
            _entityReferenceMap[reference.uniqueID].egid = new EGID((uint) _entityReferenceMap.count, 0);
            _entityReferenceMap[reference.uniqueID].version++;

            // Mark the element as the last element used.
            _nextReferenceIndex = reference.uniqueID;
        }

        void RemoveAllGroupReferenceLocators(uint groupId)
        {
            if (_egidToReferenceMap.TryGetValue(groupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and remove the locator using the egid.
            // RemoveLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                RemoveReferenceLocator(new EGID(keys[i].key, groupId));
                if (i == 0)
                    break;
            }

            _egidToReferenceMap.Remove(groupId);
        }

        void UpdateAllGroupReferenceLocators(uint fromGroupId, uint toGroupId)
        {
            if (_egidToReferenceMap.TryGetValue(fromGroupId, out var groupMap) == false)
            {
                return;
            }

            // We need to traverse all entities in the group and update the locator using the egid.
            // UpdateLocator would modify the enumerator so this is why we traverse the dictionary from last to first.
            var keys = groupMap.unsafeKeys;
            for (var i = groupMap.count - 1; true; i--)
            {
                UpdateReferenceLocator(new EGID(keys[i].key, fromGroupId), new EGID(keys[i].key, toGroupId));
                if (i == 0)
                    break;
            }

            _egidToReferenceMap.Remove(fromGroupId);
        }

        internal EntityReference GetEntityReference(EGID egid)
        {
            if (_egidToReferenceMap.TryGetValue(egid.groupID, out var groupMap))
            {
                if (groupMap.TryGetValue(egid.entityID, out var locator))
                {
                    return locator;
                }
            }

            return EntityReference.Invalid;
        }

        internal bool TryGetEGID(EntityReference reference, out EGID egid)
        {
            egid = new EGID();
            if (reference == EntityReference.Invalid)
                return false;
            // Make sure we are querying for the current version of the locator.
            // Otherwise the locator is pointing to a removed entity.
            if (_entityReferenceMap[reference.uniqueID].version == reference.version)
            {
                egid = _entityReferenceMap[reference.uniqueID].egid;
                return true;
            }

            return false;
        }

        uint                                                                     _nextReferenceIndex;
        readonly FasterList<EntityLocatorMapElement>                             _entityReferenceMap;
        readonly FasterDictionary<uint, FasterDictionary<uint, EntityReference>> _egidToReferenceMap;
    }
}