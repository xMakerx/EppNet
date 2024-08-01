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
            Assert.IsTrue(objs._pageIndexWithAvaliability == 0, "Did not set the first page as available");
            Assert.IsTrue(objs._pages.Count == 1, "More than 1 page allocated!");
            Assert.IsTrue(objs._pages[0].AvailableIndex == 0, "Index 0 is not available?");

            Console.WriteLine(objs._pages[0].GetFreeString());
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

            Page<ObjectSlot> p1 = objs._pages[0];
            int shouldBeAvailable = (objs.ItemsPerPage / 2);
            Assert.IsTrue(!p1.Empty, "Page should be half full!");
            Assert.IsTrue(p1.AvailableIndex == shouldBeAvailable, $"Available Index should be {shouldBeAvailable}");
            Console.WriteLine(p1.GetFreeString());
        }

    }

}