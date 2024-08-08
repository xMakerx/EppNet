///////////////////////////////////////////////////////
/// Filename: PageListTests.cs
/// Date: July 31, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
// Happy birthday, Harry Potter. Are you ready for your
// OWLs?

using EppNet.Collections;
using EppNet.Objects;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

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

            Assert.IsTrue(slot.ID == 125, $"Did not allocate the proper slot! Slot: {slot.ID}");
            Assert.IsTrue(objs.PageIndexWithAvailability == 0, "Did not set the first page as available");
            Assert.IsTrue(objs.Pages.Count == 1, "More than 1 page allocated!");
            Assert.IsTrue(objs.Pages[0].AvailableIndex == 0, "Index 0 is not available?");

            Console.WriteLine(objs.Pages[0].GetBitString());
        }

        [TestMethod]
        public void AllocateIdx127AndCall()
        {
            PageList<ObjectSlot> objs = new(128);
            const int id = 127;

            objs.TryAllocate(id, out ObjectSlot slot);

            Assert.IsTrue(slot.ID == id, $"Did not allocate the proper slot! Slot: {slot.ID}");
            Assert.IsTrue(objs.PageIndexWithAvailability == 0, "Did not set the first page as available");
            Assert.IsTrue(objs.Pages.Count == 1, "More than 1 page allocated!");
            Assert.IsTrue(objs.Pages[0].AvailableIndex == 0, "Index 0 is not available?");

            Console.WriteLine(objs.Pages[0].GetBitString());

            objs.DoOnActive((ObjectSlot a) =>
            {
                Console.WriteLine($"Hello {a.ID}!");
                Assert.IsTrue(a != null, "Wrong slot!");
                Assert.IsTrue(a.ID == id, "Invalid slot id!");
            });
        }

        [TestMethod]
        public void AllocateFirstBlock()
        {
            PageList<ObjectSlot> objs = new(128);

            for (int i = 0; i < objs.ItemsPerPage / 2; i++)
            {
                objs.TryAllocate(i, out ObjectSlot slot);
                Assert.IsTrue(slot.ID == i, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            int shouldBeAvailable = (objs.ItemsPerPage / 2);
            Assert.IsTrue(!p1.Empty, "Page should be half full!");
            Assert.IsTrue(p1.AvailableIndex == shouldBeAvailable, $"Available Index should be {shouldBeAvailable}");
            Console.WriteLine(p1.GetBitString());
        }

        [TestMethod]
        public void AllocateRandom()
        {
            PageList<ObjectSlot> objs = new(128);
            int allocations = objs.ItemsPerPage / 4;

            Random rand = new Random();

            for (int i = 0; i < allocations; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (!objs.IsAvailable(id));

                objs.TryAllocate(id, out ObjectSlot slot);
                Assert.IsTrue(slot.ID == id, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsTrue(!p1.Empty, "Page should not be full!");
            Assert.IsTrue(p1.AvailableIndex != -1, "Page shouldn't be full, so an index should be available!");
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
                Assert.IsTrue(slot.ID == id, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsTrue(!p1.Empty, "Page should be full!");
            Assert.IsTrue(p1.AvailableIndex == -1, "Page should be full! An index is available!");
        }

        [TestMethod]
        public void RandomFillAndClearPage()
        {
            PageList<ObjectSlot> objs = new(128);
            int allocations = objs.ItemsPerPage;

            Random rand = new Random();
            int toClear = rand.Next(allocations / 2);


            for (int i = 0; i < allocations; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (!objs.IsAvailable(id));

                objs.TryAllocate(id, out ObjectSlot slot);
                Assert.IsTrue(slot.ID == id, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Page<ObjectSlot> p1 = objs.Pages[0];
            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsTrue(!p1.Empty, "Page should be full!");
            Assert.IsTrue(p1.AvailableIndex == -1, "Page should be full! An index is available!");

            for (int i = 0; i < toClear; i++)
            {
                int id;
                do
                {
                    id = rand.Next(objs.ItemsPerPage);
                } while (objs.IsAvailable(id));

                ObjectSlot slot = objs.Get(id);
                objs.TryFree(slot);
                Assert.IsTrue(slot.ID == id, $"Did not allocate the proper slot! Slot: {slot.ID}");
            }

            Console.WriteLine(p1.AvailableIndex);
            Console.WriteLine(p1.GetBitString());
            Assert.IsTrue(!p1.Empty, "Page shouldn't be empty but not full!");
            Assert.IsTrue(p1.AvailableIndex != -1, "Page shouldn't be full! An index isn't available!");
        }

    }

}