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
                Console.WriteLine(result);
                Assert.IsTrue(read && l.Equals(result), $"Failed to resolve read for {l.GetType().Name}");
            }

        }

        [TestMethod]
        public void TestVec3DeltaResolver()
        {
            byte[] buffer;

            Vector3 l = new(3f, 0.1f, 4.0f);
            Vector3 b = new(2f, 0.1f, 3.0f);

            Vector3 delta = l - b;

            using (BytePayload payloadIn = new BytePayload())
            {

                bool written = Vector3Resolver.Instance.Write(payloadIn, delta, false);

                Assert.IsTrue(written, $"Failed to resolve write for {l.GetType().Name}");
                buffer = payloadIn.Pack();
            }

            using (BytePayload payloadOut = new BytePayload(buffer))
            {
                bool read = payloadOut.TryRead(out Vector3 result);
                Console.WriteLine(result);
                Assert.IsTrue(read && l.Equals(b + result), $"Failed to resolve read for {l.GetType().Name}");
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
