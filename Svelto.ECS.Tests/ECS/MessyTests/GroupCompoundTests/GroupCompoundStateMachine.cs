
using NUnit.Framework;

//todo: initial idea on how I was picturing GroupCompound state machines
//would also have replaced the current way to swap between abstracted group compounds
namespace Svelto.ECS.Tests.GroupCompounds
{
    // [TestFixture]
    // public class GroupCompoundAbstractStateChangeTest
    // {
    //     public class DOOFUSES : GroupTag<DOOFUSES> { }
    //     public class EATING : GroupTag<EATING> { }
    //     public class FALLING : GroupTag<FALLING> { }
    //     public class GROUNDED : GroupTag<GROUNDED> { }
    //     public class ENABLED : GroupTag<ENABLED> { }
    //     public class DISABLED : GroupTag<DISABLED> { }
    //     
    //     public class FallingDoofusesEnabled : GroupCompound<DOOFUSES, FALLING, ENABLED>
    //     {
    //         static FallingDoofusesEnabled()
    //         {
    //             AddTransition<DOOFUSES, GROUNDED, ENABLED>();
    //             
    //             AddTransition<GroupCompound<Enabled, Falling>, GroupCompound<Disabled, Grounded>>();
    //         }
    //     };
    //
    //     public class EatingFallingDoofusesEnabled : GroupCompound<DOOFUSES, FALLING, ENABLED>
    //     {
    //         static EatingFallingDoofusesEnabled()
    //         {
    //             AddTransition<DOOFUSES, GROUNDED, EATING, ENABLED>();
    //             
    //             AddTransition<GroupCompound<Enabled, Falling>, GroupCompound<Disabled, Grounded>>();
    //         }
    //     };
    //     
    //     public class EatingGroundedDoofusesEnabled : GroupCompound<DOOFUSES, EATING, ENABLED>
    //     {
    //         static EatingGroundedDoofusesEnabled()
    //         {
    //             AddTransition<DOOFUSES, FALLING, EATING, ENABLED>();
    //             
    //             AddTransition<GroupCompound<Enabled, Falling>, GroupCompound<Disabled, Grounded>>();
    //         }
    //     };
    //
    //     [TestCase]
    //     public void Test()
    //     {
    //         
    //         //Swap(current, current.Swap<Ground, Falling>);
    //         //Swap(current, current.Swap<GroupCompound<Enabled, Falling>, GroupCompound<Disabled, Grounded>>);
    //     }
    //}
}