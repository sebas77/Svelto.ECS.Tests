using NUnit.Framework;

namespace Svelto.ECS.Tests.GroupCompounds
{
    [TestFixture]
    public class GroupCompoundTest2
    {
        public class DOOFUSES : GroupTag<DOOFUSES> { }
        public class EATING : GroupTag<EATING> { }
        public class RED : GroupTag<RED> { }
        public class ENABLED : GroupTag<ENABLED> { }
        
            public class EatingRedDoofuses1  : GroupCompound<DOOFUSES, RED, EATING, ENABLED>{};
            public class EatingRedDoofuses2  : GroupCompound<RED, DOOFUSES, EATING, ENABLED>{};
            public class EatingRedDoofuses3  : GroupCompound<DOOFUSES, EATING, RED, ENABLED>{};
            public class EatingRedDoofuses4  : GroupCompound<EATING, DOOFUSES, RED, ENABLED>{};
            public class EatingRedDoofuses5  : GroupCompound<EATING, RED, DOOFUSES, ENABLED>{};
            public class EatingRedDoofuses6  : GroupCompound<RED, EATING, DOOFUSES, ENABLED>{};
            public class EatingRedDoofuses7  : GroupCompound<DOOFUSES, RED, ENABLED, EATING>{};
            public class EatingRedDoofuses8  : GroupCompound<RED, DOOFUSES, ENABLED, EATING>{};
            public class EatingRedDoofuses9  : GroupCompound<DOOFUSES, EATING, ENABLED, RED>{};
            public class EatingRedDoofuses10 : GroupCompound<EATING, DOOFUSES, ENABLED, RED>{};
            public class EatingRedDoofuses11 : GroupCompound<EATING, RED, ENABLED, DOOFUSES>{};
            public class EatingRedDoofuses12 : GroupCompound<RED, EATING, ENABLED, DOOFUSES>{};
            public class EatingRedDoofuses13 : GroupCompound<DOOFUSES, ENABLED, RED, EATING>{};
            public class EatingRedDoofuses14 : GroupCompound<RED, ENABLED, DOOFUSES, EATING>{};
            public class EatingRedDoofuses15 : GroupCompound<DOOFUSES, ENABLED, EATING, RED>{};
            public class EatingRedDoofuses16 : GroupCompound<EATING, ENABLED, DOOFUSES, RED>{};
            public class EatingRedDoofuses17 : GroupCompound<EATING, ENABLED, RED, DOOFUSES>{};
            public class EatingRedDoofuses18 : GroupCompound<RED, ENABLED, EATING, DOOFUSES>{};
            public class EatingRedDoofuses19 : GroupCompound<ENABLED, DOOFUSES, RED, EATING>{};
            public class EatingRedDoofuses20 : GroupCompound<ENABLED, RED, DOOFUSES, EATING>{};
            public class EatingRedDoofuses21 : GroupCompound<ENABLED, DOOFUSES, EATING, RED>{};
            public class EatingRedDoofuses22 : GroupCompound<ENABLED, EATING, DOOFUSES, RED>{};
            public class EatingRedDoofuses23 : GroupCompound<ENABLED, EATING, RED, DOOFUSES>{};
            public class EatingRedDoofuses24 : GroupCompound<ENABLED, RED, EATING, DOOFUSES>{};

        [TestCase]
        public void Test()
        {
            Assert.AreEqual( EatingRedDoofuses1.Groups , EatingRedDoofuses2.Groups);
            Assert.AreEqual( EatingRedDoofuses2.Groups,  EatingRedDoofuses3.Groups);
            Assert.AreEqual( EatingRedDoofuses3.Groups,  EatingRedDoofuses4.Groups);
            Assert.AreEqual( EatingRedDoofuses4.Groups,  EatingRedDoofuses5.Groups);
            Assert.AreEqual( EatingRedDoofuses5.Groups,  EatingRedDoofuses6.Groups);
            Assert.AreEqual( EatingRedDoofuses6.Groups,  EatingRedDoofuses7.Groups);
            Assert.AreEqual( EatingRedDoofuses7.Groups,  EatingRedDoofuses8.Groups);
            Assert.AreEqual( EatingRedDoofuses8.Groups,  EatingRedDoofuses9.Groups);
            Assert.AreEqual( EatingRedDoofuses9.Groups, EatingRedDoofuses10.Groups);
            Assert.AreEqual(EatingRedDoofuses10.Groups, EatingRedDoofuses11.Groups);
            Assert.AreEqual(EatingRedDoofuses11.Groups, EatingRedDoofuses12.Groups);
            Assert.AreEqual(EatingRedDoofuses12.Groups, EatingRedDoofuses13.Groups);
            Assert.AreEqual(EatingRedDoofuses13.Groups, EatingRedDoofuses14.Groups);
            Assert.AreEqual(EatingRedDoofuses14.Groups, EatingRedDoofuses15.Groups);
            Assert.AreEqual(EatingRedDoofuses15.Groups, EatingRedDoofuses16.Groups);
            Assert.AreEqual(EatingRedDoofuses16.Groups, EatingRedDoofuses17.Groups);
            Assert.AreEqual(EatingRedDoofuses17.Groups, EatingRedDoofuses18.Groups);
            Assert.AreEqual(EatingRedDoofuses18.Groups, EatingRedDoofuses19.Groups);
            Assert.AreEqual(EatingRedDoofuses19.Groups, EatingRedDoofuses20.Groups);
            Assert.AreEqual(EatingRedDoofuses20.Groups, EatingRedDoofuses21.Groups);
            Assert.AreEqual(EatingRedDoofuses21.Groups, EatingRedDoofuses22.Groups);
            Assert.AreEqual(EatingRedDoofuses22.Groups, EatingRedDoofuses23.Groups);
            Assert.AreEqual(EatingRedDoofuses23.Groups, EatingRedDoofuses24.Groups);
            
            Assert.AreEqual( EatingRedDoofuses1.BuildGroup , EatingRedDoofuses2.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses2.BuildGroup,  EatingRedDoofuses3.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses3.BuildGroup,  EatingRedDoofuses4.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses4.BuildGroup,  EatingRedDoofuses5.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses5.BuildGroup,  EatingRedDoofuses6.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses6.BuildGroup,  EatingRedDoofuses7.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses7.BuildGroup,  EatingRedDoofuses8.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses8.BuildGroup,  EatingRedDoofuses9.BuildGroup);
            Assert.AreEqual( EatingRedDoofuses9.BuildGroup, EatingRedDoofuses10.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses10.BuildGroup, EatingRedDoofuses11.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses11.BuildGroup, EatingRedDoofuses12.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses12.BuildGroup, EatingRedDoofuses13.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses13.BuildGroup, EatingRedDoofuses14.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses14.BuildGroup, EatingRedDoofuses15.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses15.BuildGroup, EatingRedDoofuses16.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses16.BuildGroup, EatingRedDoofuses17.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses17.BuildGroup, EatingRedDoofuses18.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses18.BuildGroup, EatingRedDoofuses19.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses19.BuildGroup, EatingRedDoofuses20.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses20.BuildGroup, EatingRedDoofuses21.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses21.BuildGroup, EatingRedDoofuses22.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses22.BuildGroup, EatingRedDoofuses23.BuildGroup);
            Assert.AreEqual(EatingRedDoofuses23.BuildGroup, EatingRedDoofuses24.BuildGroup);

            //Doofuses
            //Doofuses, Red
            //Doofuses, EATING
            //Doofuses, ENABLED
            //Doofuses, ENABLED, RED
            //Doofuses, ENABLED, EATING
            //Doofuses, EATING, RED
            //Doofuses, EATING, RED, ENABLED
            Assert.That(GroupTag<DOOFUSES>.Groups.count, Is.EqualTo(49));
            Assert.That(GroupTag<EATING>.Groups.count, Is.EqualTo(49));
            Assert.That(GroupTag<RED>.Groups.count, Is.EqualTo(49));
            Assert.That(GroupTag<ENABLED>.Groups.count, Is.EqualTo(49));
        }
    }
}