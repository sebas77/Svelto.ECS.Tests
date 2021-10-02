using NUnit.Framework;

namespace Svelto.ECS.Tests.GroupCompounds
{
    [TestFixture]
    public class GroupCompoundTest4
    {
        public class DOOFUSES : GroupTag<DOOFUSES> { }
        public class EATING : GroupTag<EATING> { }
        public class NOTEATING : GroupTag<NOTEATING> { }
        public class RED : GroupTag<RED> { }

        [TestCase]
        public void Test()
        {
            var RedDoofuses1 = GroupCompound<DOOFUSES, RED>.Groups;
            var RedDoofuses2 = GroupCompound<RED, DOOFUSES>.Groups;

            var EatingDoofuses1 = GroupCompound<DOOFUSES, EATING>.Groups;
            var EatingDoofuses2 = GroupCompound<EATING, DOOFUSES>.Groups;

            var NotEatingDoofuses1 = GroupCompound<NOTEATING, DOOFUSES>.Groups;
            var NotEatingDoofuses2 = GroupCompound<DOOFUSES, NOTEATING>.Groups;

            var RedEating1 = GroupCompound<NOTEATING, RED>.Groups;
            var RedEating2 = GroupCompound<RED, NOTEATING>.Groups;

            var NotReadEating1 = GroupCompound<EATING, RED>.Groups;
            var NotReadEating2 = GroupCompound<RED, EATING>.Groups;

            Assert.AreEqual(RedDoofuses1, RedDoofuses2);
            Assert.AreEqual(EatingDoofuses1, EatingDoofuses2);
            Assert.AreEqual(NotEatingDoofuses1, NotEatingDoofuses2);
            Assert.AreEqual(RedEating1, RedEating2);
            Assert.AreEqual(NotReadEating1, NotReadEating2);

            Assert.That(GroupTag<DOOFUSES>.Groups.count, Is.EqualTo(7));
            Assert.That(GroupTag<EATING>.Groups.count, Is.EqualTo(5));
            Assert.That(GroupTag<RED>.Groups.count, Is.EqualTo(7));
            Assert.That(GroupTag<NOTEATING>.Groups.count, Is.EqualTo(5));
        }
    }
}