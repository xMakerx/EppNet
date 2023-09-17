///////////////////////////////////////////////////////
/// Filename: ResolverTests.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EppNet.Tests
{

    [TestClass]
    public class ResolverTests
    {

        [TestMethod]
        public void TestLoc3Resolver()
        {
            byte[] buffer = null;

            Loc3 l = new(3f, 0.1f, 4.0f);

            using (BytePayload payloadIn = new BytePayload())
            {
                bool written = payloadIn.TryWrite(l);

                Assert.IsTrue(written, "Failed to resolve write for Loc3!");
                buffer = payloadIn.Pack();
            }

            using (BytePayload payloadOut = new BytePayload(buffer))
            {
                object result = payloadOut.TryRead(typeof(Loc3));
                Assert.IsTrue(l.Equals(result), "Failed to resolve read for Loc3!");
            }

        }


    }

}
