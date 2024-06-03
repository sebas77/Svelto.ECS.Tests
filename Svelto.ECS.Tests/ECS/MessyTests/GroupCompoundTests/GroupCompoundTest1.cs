using NUnit.Framework;

namespace Svelto.ECS.Tests.GroupCompounds
{
    [TestFixture]
    public class GroupCompoundTest1
    {
        public class DOOFUSES : GroupTag<DOOFUSES> { }
        public class EATING : GroupTag<EATING> { }
        public class NOTEATING : GroupTag<NOTEATING> { }
        public class RED : GroupTag<RED> { }

        [TestCase]
        public void Test()
        {
            var EatingRedDoofuses1 = GroupCompound<DOOFUSES, RED, EATING>.Groups;
            var EatingRedDoofuses2 = GroupCompound<RED, DOOFUSES, EATING>.Groups;
            var EatingRedDoofuses3 = GroupCompound<DOOFUSES, EATING, RED>.Groups;
            var EatingRedDoofuses4 = GroupCompound<EATING, DOOFUSES, RED>.Groups;
            var EatingRedDoofuses5 = GroupCompound<EATING, RED, DOOFUSES>.Groups;
            var EatingRedDoofuses6 = GroupCompound<RED, EATING, DOOFUSES>.Groups;

            var NoEatingRedDoofuses1 = GroupCompound<DOOFUSES, RED, NOTEATING>.Groups;
            var NoEatingRedDoofuses2 = GroupCompound<RED, DOOFUSES, NOTEATING>.Groups;
            var NoEatingRedDoofuses3 = GroupCompound<DOOFUSES, NOTEATING, RED>.Groups;
            var NoEatingRedDoofuses4 = GroupCompound<NOTEATING, DOOFUSES, RED>.Groups;
            var NoEatingRedDoofuses5 = GroupCompound<NOTEATING, RED, DOOFUSES>.Groups;
            var NoEatingRedDoofuses6 = GroupCompound<RED, NOTEATING, DOOFUSES>.Groups;

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

            Assert.AreEqual(EatingRedDoofuses1, EatingRedDoofuses2);
            Assert.AreEqual(EatingRedDoofuses2, EatingRedDoofuses3);
            Assert.AreEqual(EatingRedDoofuses3, EatingRedDoofuses4);
            Assert.AreEqual(EatingRedDoofuses4, EatingRedDoofuses5);
            Assert.AreEqual(EatingRedDoofuses5, EatingRedDoofuses6);

            Assert.AreEqual(NoEatingRedDoofuses1, NoEatingRedDoofuses2);
            Assert.AreEqual(NoEatingRedDoofuses2, NoEatingRedDoofuses3);
            Assert.AreEqual(NoEatingRedDoofuses3, NoEatingRedDoofuses4);
            Assert.AreEqual(NoEatingRedDoofuses4, NoEatingRedDoofuses5);
            Assert.AreEqual(NoEatingRedDoofuses5, NoEatingRedDoofuses6);

            Assert.AreNotEqual(EatingRedDoofuses5, NoEatingRedDoofuses6);

            //The number of groups I expected linked to each tag is not the number of permutation, but the number
            //of unique combinations
            Assert.That(GroupTag<DOOFUSES>.Groups.count, Is.EqualTo(6));
            Assert.That(GroupTag<EATING>.Groups.count, Is.EqualTo(4));
            Assert.That(GroupTag<RED>.Groups.count, Is.EqualTo(6));
            Assert.That(GroupTag<NOTEATING>.Groups.count, Is.EqualTo(4));
        }
    }
}