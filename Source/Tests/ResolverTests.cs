///////////////////////////////////////////////////////
/// Filename: ResolverTests.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System.Numerics;

namespace EppNet.Tests
{

    [TestClass]
    public class ResolverTests
    {

        [TestMethod]
        public void TestVec3Resolver()
        {
            byte[] buffer;

            Vector3 l = new(3f, 0.1f, 4.0f);

            using (BytePayload payloadIn = new BytePayload())
            {
                bool written = payloadIn.TryWrite(l);

                Assert.IsTrue(written, $"Failed to resolve write for {l.GetType().Name}");
                buffer = payloadIn.Pack();
            }

            using (BytePayload payloadOut = new BytePayload(buffer))
            {
                bool read = payloadOut.TryRead(out Vector3 result);
                Assert.IsTrue(read && l.Equals(result), $"Failed to resolve read for {l.GetType().Name}");
            }

        }

        [TestMethod]
        public void TestVec3ZeroResolver()
        {
            byte[] buffer;

            Vector3 l = Vector3.Zero;

            using (BytePayload payloadIn = new BytePayload())
            {
                bool written = payloadIn.TryWrite(l);

                Assert.IsTrue(written, $"Failed to resolve write for {l.GetType().Name}");
                buffer = payloadIn.Pack();
            }

            using (BytePayload payloadOut = new BytePayload(buffer))
            {
                bool read = payloadOut.TryRead(out Vector3 result);

                Console.WriteLine($"Buffer size: {buffer.Length}");
                Assert.IsTrue(read && l.Equals(result) && buffer.Length == 1, $"Failed to resolve read for {l.GetType().Name}");
            }

        }


    }

}
