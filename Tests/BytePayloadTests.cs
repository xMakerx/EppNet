///////////////////////////////////////////////////////
/// Filename: BytePayloadTests.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EppNet.Tests
{

    [TestClass]
    public class BytePayloadTests
    {

        [TestMethod]
        public void ReadAndWriteString()
        {

            byte[] bufferIn = null;
            string input = "Hello World!";

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteString(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadString());
        }

        /// <summary>
        /// NOTE: This test currently runs first; so don't be surprised if its duration is the highest out of all.
        /// Writing booleans to the stream isn't slow, the issue is that the stream pool is being initialized.
        /// </summary>

        [TestMethod]
        public void ReadAndWriteBool()
        {
            byte[] bufferIn = null;
            bool input = true;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteBool(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadBool());
        }

        [TestMethod]
        public void ReadAndWriteByte()
        {
            byte[] bufferIn = null;
            byte input = 2;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteByte(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadByte());
        }

        [TestMethod]
        public void ReadAndWriteSByte()
        {
            byte[] bufferIn = null;
            sbyte input = 4;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteSByte(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadSByte());
        }

        [TestMethod]
        public void ReadAndWriteShort()
        {
            byte[] bufferIn = null;
            short input = 8;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteShort(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadShort());
        }

        [TestMethod]
        public void ReadAndWriteUShort()
        {
            byte[] bufferIn = null;
            ushort input = 14;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteUShort(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadUShort());
        }

        [TestMethod]
        public void ReadAndWriteULong()
        {
            byte[] bufferIn = null;
            ulong input = 99998;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteULong(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadULong());
        }

        [TestMethod]
        public void ReadAndWriteLong()
        {
            byte[] bufferIn = null;
            long input = 888888;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteLong(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadLong());
        }

        [TestMethod]
        public void ReadAndWriteUInt()
        {
            byte[] bufferIn = null;
            uint input = 7;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteUInt(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadUInt());
        }

        [TestMethod]
        public void ReadAndWriteInt()
        {
            byte[] bufferIn = null;
            int input = -30;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteInt(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadInt());
        }

    }

}