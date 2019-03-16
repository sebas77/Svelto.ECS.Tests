using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable 660,661

namespace Svelto.ECS
{
    public struct EGID:IEquatable<EGID>,IEqualityComparer<EGID>,IComparable<EGID>
    {
        readonly ulong _GID;
        
        public EGID(uint entityID, ExclusiveGroup.ExclusiveGroupStruct groupID) : this()
        {
            DBC.ECS.Check.Require(entityID < bit22, "the entityID value is outside the range");
            DBC.ECS.Check.Require(groupID < bit20, "the groupID value is outside the range");
            
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0);
        }

        const uint bit22 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111;
        const uint bit20 = 0b0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_0000_1111_1111_1111_1111_1111;
        const long bit42 = 0b0000_0000_0000_0000_0000_0011_1111_1111_1111_1111_1111_1111_1111_1111_1111_1111;

        public uint entityID
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (uint) (_GID & bit22); }
        }

        public ExclusiveGroup.ExclusiveGroupStruct groupID =>
            new ExclusiveGroup.ExclusiveGroupStruct((uint) ((_GID >> 22) & bit20));

        public uint maskedGID => (uint) _GID >> 42;

        public static bool operator ==(EGID obj1, EGID obj2)
        {
            return obj1.maskedGID == obj2.maskedGID;
        }    
        
        public static bool operator !=(EGID obj1, EGID obj2)
        {
            return obj1.maskedGID != obj2.maskedGID;
        }
//            22       20        22        
//        | realid | groupid | entityID |
        
        static ulong MAKE_GLOBAL_ID(uint entityId, uint groupId, uint realId)
        {
            return (((ulong)realId & bit22) << (22+20)) | (((ulong)groupId & bit20) << 22) | ((ulong)entityId & bit22);
        }

        public static explicit operator uint(EGID id)
        {
            return id.entityID;
        }
        
        public static explicit operator ulong(EGID id)
        {
            return id._GID & bit42;
        }

        public bool Equals(EGID other)
        {
            return _GID == other._GID;
        }

        public bool Equals(EGID x, EGID y)
        {
            return x._GID == y._GID;
        }

        public int GetHashCode(EGID egid) { return (int) _GID.GetHashCode(); }

        public int CompareTo(EGID other)
        {
            return _GID.CompareTo(other._GID);
        }
        
        internal EGID(uint entityID, uint groupID) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID, 0);
        }
        
        internal EGID(uint entityID, uint groupID, uint realid) : this()
        {
            _GID = MAKE_GLOBAL_ID(entityID, groupID, realid);
        }
        
        internal EGID(ulong egid) : this()
        {
            _GID = egid;
        }
    }
}