namespace Svelto.ECS.Tests
{
    static class Groups
    {
        public static readonly ExclusiveGroup GroupA = new ExclusiveGroup();
        public static readonly ExclusiveGroup GroupB = new ExclusiveGroup();
    }

    static class TestGroups
    {
        internal static readonly ExclusiveGroupStruct Disabled = new ExclusiveGroup(ExclusiveGroupBitmask.DISABLED_BIT);
        
        internal class TestGroupTag : GroupTag<TestGroupTag> { }
    }

    static class TestFilters
    {
        private static readonly  FilterContextID  TestContext = EntitiesDB.SveltoFilters.GetNewContextID();
        internal static readonly CombinedFilterID TestID      = new(0, TestContext);
    }
}