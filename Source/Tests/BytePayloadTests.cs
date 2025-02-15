///////////////////////////////////////////////////////
/// Filename: BytePayloadTests.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;
using EppNet.Utilities;

using System.Numerics;

namespace EppNet.Tests
{

    [TestClass]
    public class BytePayloadTests
    {

        [TestMethod]
        public void ReadAndWriteQuat()
        {
            const float MarginOfError = 0.0039f * 4f;

            QuaternionAdapter a = new();

            for (int i = 0; i < 4; i++)
            {
                a[i] = -Random.Shared.NextSingle() + Random.Shared.NextSingle();
            }

            Quaternion quantized = QuaternionAdapter.Dequantized(QuaternionAdapter.Quantized(a));
            byte[] bufferIn;

            using (BytePayload payloadOut = new())
            {
                payloadOut.Write(a);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new(bufferIn))
            {
                var result = payloadIn.ReadQuat();

                Quaternion normalized = Quaternion.Normalize(a);
                float errorMargin = 0f;

                Quaternion test = result - normalized;

                for (int i = 0; i < 4; i++)
                    errorMargin += test[i];

                Console.WriteLine(quantized);
                Console.WriteLine(result);
                Assert.IsTrue(quantized.Equals(result) && MathF.Abs(errorMargin) <= MarginOfError);
            }

        }

        [TestMethod]
        public void ReadAndWriteString8()
        {

            byte[] bufferIn;
            string input = "Hello World!";

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.Encoder = System.Text.Encoding.ASCII;
                payloadOut.WriteString8(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
            {
                payloadIn.Encoder = System.Text.Encoding.ASCII;
                Str8 result = payloadIn.ReadString8();
                Assert.IsTrue(string.Equals(input, result.Value, StringComparison.Ordinal));
            }
        }

        [TestMethod]
        public void ReadAndWriteString16()
        {

            byte[] bufferIn;
            string input = "Hello World!";

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteString16(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
            {
                Str16 result = payloadIn.ReadString16();
                Console.WriteLine(result.Value);
                Assert.IsTrue(string.Equals(input, result.Value, StringComparison.Ordinal));
            }
        }

        /// <summary>
        /// NOTE: This test currently runs first; so don't be surprised if its duration is the highest out of all.
        /// Writing booleans to the stream isn't slow, the issue is that the stream pool is being initialized.
        /// </summary>

        [TestMethod]
        public void ReadAndWriteBool()
        {
            byte[] bufferIn;
            const bool input = true;

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
            byte[] bufferIn;
            const byte input = 2;

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
            byte[] bufferIn;
            const sbyte input = -4;

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
            byte[] bufferIn;
            const short input = 8;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteShort(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadShort());
        }

        [TestMethod]
        public void WriteTwoShorts()
        {
            byte[] bufferIn;
            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteShort(7);
                payloadOut.WriteShort(13);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
            {
                Assert.AreEqual(7, payloadIn.ReadShort());
                Assert.AreEqual(13, payloadIn.ReadShort());
            }
        }

        [TestMethod]
        public void ReadAndWriteUShort()
        {
            byte[] bufferIn;
            const ushort input = 14;

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
            byte[] bufferIn;
            const ulong input = 99998;

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
            byte[] bufferIn;
            const long input = 888888;

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
            byte[] bufferIn;
            const uint input = 7;

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
            byte[] bufferIn;
            const int input = -30;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteInt(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadInt());
        }

        [TestMethod]
        public void ReadAndWriteFloat()
        {
            byte[] bufferIn;
            const float input = 3.14159f;

            BytePayload.FloatPrecision = 4;

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteFloat(input);
                bufferIn = payloadOut.Pack();
            }

            float expectedOutput = 3.1416f;

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(expectedOutput, payloadIn.ReadFloat());
        }

    }

}