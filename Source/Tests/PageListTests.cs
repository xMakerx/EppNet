///////////////////////////////////////////////////////
/// Filename: PageListTests.cs
/// Date: July 31, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
// Happy birthday, Harry Potter. Are you ready for your
// OWLs?

using EppNet.Collections;
using EppNet.Objects;

namespace EppNet.Tests
{

    [TestClass]
    public class PageListTests
    {

        [TestMethod]
        public void AllocateIdx125()
        {
            PageList<ObjectSlot> objs = new(128);

            objs.TryAllocate(125L, out ObjectSlot slot);

            Assert.AreEqual(125, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            Assert.AreEqual(0, objs.PageIndexWithAvailability, "Did not set the first page as available");
            Assert.AreEqual(1, objs.Pages.Count, "More than 1 page allocated!");
            Assert.AreEqual(0, objs.Pages[0].AvailableIndex, "Index 0 is not available?");

            Console.WriteLine(objs.Pages[0].GetBitString());
        }

        [TestMethod]
        public void AllocateIdx127AndCall()
        {
            PageList<ObjectSlot> objs = new(128);
            const int id = 127;

            objs.TryAllocate(id, out ObjectSlot slot);

            Assert.AreEqual(id, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            Assert.AreEqual(0, objs.PageIndexWithAvailability, "Did not set the first page as available");
            Assert.AreEqual(1, objs.Pages.Count, "More than 1 page allocated!");
            Assert.AreEqual(0, objs.Pages[0].AvailableIndex, "Index 0 is not available?");

            Console.WriteLine(objs.Pages[0].GetBitString());

            objs.DoOnActive((ObjectSlot a) =>
            {
                Console.WriteLine($"Hello {a.ID}!");
                Assert.IsNotNull(a, "Wrong slot!");
                Assert.AreEqual(id, a.ID, "Invalid slot id!");
            });
        }

        [TestMethod]
        public void AllocateFirstBlock()
        {
            PageList<ObjectSlot> objs = new(128);

            for (int i = 0; i < objs.ItemsPerPage / 2; i++)
            {
                objs.TryAllocate(i, out ObjectSlot slot);
                Assert.AreEqual(i, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            int shouldBeAvailable = (objs.ItemsPerPage / 2);
            Assert.IsFalse(p1.Empty, "Page should be half full!");
            Assert.AreEqual(shouldBeAvailable, p1.AvailableIndex, $"Available Index should be {shouldBeAvailable}");
            Console.WriteLine(p1.GetBitString());
        }

        [TestMethod]
        public void AllocateRandom()
        {
            PageList<ObjectSlot> objs = new(128);
            int allocations = objs.ItemsPerPage / 4;

            Random rand = new();

            for (int i = 0; i < allocations; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (!objs.IsAvailable(id));

                objs.TryAllocate(id, out ObjectSlot slot);
                Assert.AreEqual(id, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsFalse(p1.Empty, "Page should not be empty!");
            Assert.AreNotEqual(-1, p1.AvailableIndex, "Page shouldn't be full, so an index should be available!");
        }

        [TestMethod]
        public void RandomFillPage()
        {
            PageList<ObjectSlot> objs = new(128);
            int allocations = objs.ItemsPerPage;

            Random rand = new Random();

            for (int i = 0; i < allocations; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (!objs.IsAvailable(id));

                objs.TryAllocate(id, out ObjectSlot slot);
                Assert.AreEqual(id, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsFalse(p1.Empty, "Page should be full!");
            Assert.AreEqual(-1, p1.AvailableIndex, "Page should be full! An index is available!");
        }

        [TestMethod]
        public void RandomFillAndClearPage()
        {
            PageList<ObjectSlot> objs = new(128);
            int allocations = objs.ItemsPerPage;

            Random rand = new Random();
            int toClear = rand.Next(allocations / 2);

            int failures = 0;
            int allocated = 0;

            while (failures <= allocations * 3 && allocated < allocations)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (!objs.IsAvailable(id));

                int retries = 0;
                do
                {
                    bool didAllocate = objs.TryAllocate(id, out ObjectSlot slot);

                    if (didAllocate)
                    {
                        Assert.AreEqual(id, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
                        allocated++;
                    }
                    else
                    {
                        failures++;
                        retries++;
                    }

                } while (retries < 2);
            }

            if (failures >= allocations * 3)
                Assert.Fail($"Failed to allocate {failures} times. Something is seriously wrong!");

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsFalse(p1.Empty, "Page should be full!");
            Assert.AreEqual(-1, p1.AvailableIndex, "Page should be full! An index is available!");

            for (int i = 0; i < toClear; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (objs.IsAvailable(id));

                ObjectSlot slot = objs.Get(id);
                objs.TryFree(slot);
                Assert.AreEqual(id, slot.ID, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsFalse(p1.Empty, "Page shouldn't be empty but not full!");
            Assert.AreNotEqual(-1, p1.AvailableIndex, "Page shouldn't be full! An index isn't available!");
        }

    }

}