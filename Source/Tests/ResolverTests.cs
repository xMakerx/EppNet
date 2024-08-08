///////////////////////////////////////////////////////
/// Filename: ResolverTests.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Numerics;

namespace EppNet.Tests
{

    [TestClass]
    public class ResolverTests
    {

        [TestMethod]
        public void TestVec3Resolver()
        {
            byte[] buffer = null;

            Vector3 l = new(3f, 0.1f, 4.0f);

            using (BytePayload payloadIn = new BytePayload())
            {
                bool written = payloadIn.TryWrite(l);

                Assert.IsTrue(written, $"Failed to resolve write for {l.GetType().Name}");
                buffer = payloadIn.Pack();
            }

            using (BytePayload payloadOut = new BytePayload(buffer))
            {
                object result = payloadOut.TryRead(l.GetType());
                Assert.IsTrue(l.Equals(result), $"Failed to resolve read for {l.GetType().Name}");
            }

        }


    }

}
