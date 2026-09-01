using Svelto.DataStructures;

namespace Svelto.Common.Tests
{
    public partial class SlotMapTests
    {
        [Test]
        public void Add_UsesInitialCapacity_BeforeGrowing()
        {
            var c = new ManagedSlotMap<int>(3);
            try
            {
                c.Add(10);
                c.Add(20);
                c.Add(30);

                Assert.That(c.capacity, Is.EqualTo(3));

                var fourth = c.Add(40);

                Assert.That(c.capacity, Is.GreaterThan(3));
                Assert.That(c[fourth], Is.EqualTo(40));
            }
            finally
            {
                c.Dispose();
            }
        }

        [Test]
        public void Clear_InvalidatesHandles_AndResetsTheFreeList()
        {
            var c = new ManagedSlotMap<int>(2);
            try
            {
                var first = c.Add(10);
                var second = c.Add(20);
                c.Remove(second);

                c.Clear();

                Assert.That(c.Has(first), Is.False);
                Assert.That(c.Has(second), Is.False);

                var next = c.Add(30);

                Assert.That(c.count, Is.EqualTo(1));
                Assert.That(c.Has(next), Is.True);
                Assert.That(c[next], Is.EqualTo(30));
            }
            finally
            {
                c.Dispose();
            }
        }
    }
}
