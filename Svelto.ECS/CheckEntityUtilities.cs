#if DEBUG && !PROFILE_SVELTO
using System;
using Svelto.DataStructures;

#else
using System;
using System.Diagnostics;
#endif

namespace Svelto.ECS
{
    /// <summary>
    ///     Note: this check doesn't catch the case when an add and remove is done on the same entity before the nextI am
    ///     submission. Two operations on the same entity are not allowed between submissions.
    /// </summary>
    public partial class EnginesRoot
    {
#if DEBUG && !PROFILE_SVELTO
        void CheckRemoveEntityID
        (EGID egid, Type entityDescriptorType, EntitiesDB entitiesDB
       , IComponentBuilder[] descriptorComponentsToVerify, string caller = "")
        {
            if (_multipleOperationOnSameEGIDChecker.ContainsKey(egid) == true)
                throw new ECSException(
                    "Executing multiple structural changes in one submission on the same entity is not supported "
                       .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                       .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                       .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                       .FastConcat(" operation was: ")
                       .FastConcat(_multipleOperationOnSameEGIDChecker[egid] == 1 ? "add" : "remove"));

            if (_idChecker.ContainsKey(egid) == false)
                throw new ECSException("Trying to remove an Entity never submitted in the database "
                                      .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID)
                                      .FastConcat(" groupid: ").FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                                      .FastConcat(entityDescriptorType != null
                                                      ? entityDescriptorType.Name
                                                      : "not available"));

            _multipleOperationOnSameEGIDChecker.Add(egid, 0);
            _idChecker.Remove(egid);
        }

        void CheckAddEntityID
        (EGID egid, Type entityDescriptorType, EntitiesDB entitiesDB
       , IComponentBuilder[] descriptorComponentsToVerify, string caller = "")
        {
            if (_multipleOperationOnSameEGIDChecker.ContainsKey(egid) == true)
                throw new ECSException(
                    "Executing multiple structural changes in one submission on the same entity is not supported "
                       .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID).FastConcat(" groupid: ")
                       .FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                       .FastConcat(entityDescriptorType != null ? entityDescriptorType.Name : "not available")
                       .FastConcat(" operation was: ")
                       .FastConcat(_multipleOperationOnSameEGIDChecker[egid] == 1 ? "add" : "remove"));

            if (_idChecker.ContainsKey(egid) == true)
                throw new ECSException("Trying to add an Entity already submitted to the database "
                                      .FastConcat(" caller: ", caller, " ").FastConcat(egid.entityID)
                                      .FastConcat(" groupid: ").FastConcat(egid.groupID.ToName()).FastConcat(" type: ")
                                      .FastConcat(entityDescriptorType != null
                                                      ? entityDescriptorType.Name
                                                      : "not available"));

            _multipleOperationOnSameEGIDChecker.Add(egid, 1);
            _idChecker.Add(egid, 1);
        }

        readonly FasterDictionary<EGID, uint> _multipleOperationOnSameEGIDChecker = new FasterDictionary<EGID, uint>();
        readonly FasterDictionary<EGID, uint> _idChecker                          = new FasterDictionary<EGID, uint>();
#else
        [Conditional("_CHECKS_DISABLED")]
        void CheckRemoveEntityID(EGID egid, Type entityDescriptorType, EntitiesDB entitiesDB, IComponentBuilder descriptorComponentToVerify
, string caller = "")
        {
        }

        [Conditional("_CHECKS_DISABLED")]
        void CheckAddEntityID(EGID egid, Type entityDescriptorType, EntitiesDB entitiesDB, IComponentBuilder descriptorComponentToVerify
, string caller = "")
        {
        }
#endif
    }
}