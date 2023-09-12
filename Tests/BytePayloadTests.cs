///////////////////////////////////////////////////////
/// Filename: BytePayloadTests.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Diagnostics;

namespace EppNet.Tests
{

    [TestClass]
    public class BytePayloadTests
    {

        [TestMethod]
        public void ReadAndWriteString8()
        {

            byte[] bufferIn = null;
            string input = "Hello World!";

            using (BytePayload payloadOut = new BytePayload())
            {
                payloadOut.WriteString8(input);
                bufferIn = payloadOut.Pack();
            }

            using (BytePayload payloadIn = new BytePayload(bufferIn))
                Assert.AreEqual(input, payloadIn.ReadString8());
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
        public void ReadAndWrite1000Bool()
        {

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteBool();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a bool: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000Byte()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteByte();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a byte: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000SByte()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteSByte();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a sbyte: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000Short()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteShort();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a short: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000UShort()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteUShort();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a ushort: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000ULong()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteULong();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a ulong: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000Long()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteLong();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a long: {watch.ElapsedMilliseconds * 0.001} ms");
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
        public void ReadAndWrite1000UInt()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteUInt();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a uint: {watch.ElapsedMilliseconds * 0.001} ms");
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

        [TestMethod]
        public void ReadAndWrite1000Int()
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteInt();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a int: {watch.ElapsedMilliseconds * 0.001} ms");
        }

        [TestMethod]
        public void ReadAndWriteFloat()
        {
            byte[] bufferIn = null;
            float input = 3.14159f;

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

        [TestMethod]
        public void ReadAndWrite1000Floats()
        {

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
                ReadAndWriteFloat();

            watch.Stop();
            Console.WriteLine($"Average time to read and write a float: {watch.ElapsedMilliseconds * 0.001} ms");

        }

    }

}