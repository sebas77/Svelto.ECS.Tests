using System.Threading;
using Svelto.DataStructures;

namespace Svelto.ECS.Experimental
{
    struct GroupsList
    {
        static GroupsList()
        {
            groups = new FasterList<ExclusiveGroupStruct>();
        }

        static readonly FasterList<ExclusiveGroupStruct> groups;
        
        public FasterList<ExclusiveGroupStruct> reference => groups;
    }
    
    public ref struct QueryGroups
    {
        static readonly ThreadLocal<GroupsList> groups = new ThreadLocal<GroupsList>();

        public FasterReadOnlyList<ExclusiveGroupStruct> result => groups.Value.reference;
        
        public QueryGroups(LocalFasterReadOnlyList<ExclusiveGroupStruct> findGroups)
        {
            var groupsValue = groups.Value;
            var group = groupsValue.reference;

            group.FastClear();
            for (int i = 0; i < findGroups.count; i++)
            {
                group.Add(findGroups[i]);
            }
        }

        public QueryGroups Except(ExclusiveGroupStruct[] groupsToIgnore)
        {
            var group = groups.Value.reference;
            var groupsCount = group.count;

            for (int i = 0; i < groupsToIgnore.Length; i++)
            {
                for (int j = 0; j < groupsCount; j++)
                    if (groupsToIgnore[i] == group[j])
                    {
                        group.UnorderedRemoveAt(j);
                        j--;
                        groupsCount--;
                    }
            }

            return this;
        }
        
        public QueryGroups Except(ExclusiveGroupStruct groupsToIgnore)
        {
            var group       = groups.Value.reference;
            var groupsCount = group.count;

            for (int j = 0; j < groupsCount; j++)
                if (groupsToIgnore == group[j])
                {
                    group.UnorderedRemoveAt(j);
                    j--;
                    groupsCount--;
                }

            return this;
        }
   }
}