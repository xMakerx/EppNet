///////////////////////////////////////////////////////
/// Filename: BytePayloadBenchmarks.cs
/// Date: September 15, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using EppNet.Data;

namespace EppNet.Tests
{

    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BytePayloadBenchmarks
    {

        public int Runs = 100000;

        [Benchmark]
        public void ReadAndWriteBool()
        {
            bool input = true;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteBool(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                bool result = payloadIn.ReadBool();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteByte()
        {
            byte input = 7;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteByte(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                byte result = payloadIn.ReadByte();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteSByte()
        {
            sbyte input = -4;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteSByte(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                sbyte result = payloadIn.ReadSByte();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteInt()
        {
            int input = -697;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteInt(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                int result = payloadIn.ReadInt();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteUInt()
        {
            uint input = 799;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteUInt(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                uint result = payloadIn.ReadUInt();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteULong()
        {
            ulong input = 987827;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteULong(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                ulong result = payloadIn.ReadULong();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteLong()
        {
            long input = -273812;

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteLong(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                long result = payloadIn.ReadLong();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndString8()
        {
            string input = "Hello World!";

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteString8(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                string result = payloadIn.ReadString8();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndString16()
        {
            string input = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a";

            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteString16(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                string result = payloadIn.ReadString16();

                if (result == input)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        [Benchmark]
        public void ReadAndWriteFloat()
        {
            float input = 3.14159f;

            BytePayload.FloatPrecision = 4;
            BytePayload payloadOut = new BytePayload();
            BytePayload payloadIn = new BytePayload();

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteFloat(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                float result = payloadIn.ReadFloat();

                if (result == 3.1416f)
                {
                    payloadOut.Dispose();
                    payloadIn.Dispose();
                }
            }
        }

        public static void Main(string[] args) => BenchmarkRunner.Run<BytePayloadBenchmarks>();

    }

}
