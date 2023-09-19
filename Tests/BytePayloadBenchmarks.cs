///////////////////////////////////////////////////////
/// Filename: BytePayloadBenchmarks.cs
/// Date: September 15, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using EppNet.Data;

using System.Numerics;

namespace EppNet.Tests
{

    [MemoryDiagnoser]
    [RankColumn]
    [Orderer(BenchmarkDotNet.Order.SummaryOrderPolicy.FastestToSlowest)]
    public class BytePayloadBenchmarks
    {

        public const int Runs = 1000000;

        public BytePayload payloadIn = new BytePayload();
        public BytePayload payloadOut = new BytePayload();

        public readonly string str8Input = "Hello World!";
        public readonly string str16Input = "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a";

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteBool()
        {
            bool input = true;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteBool(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                bool result = payloadIn.ReadBool();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]//(OperationsPerInvoke = Runs)]
        public void WriteBool()
        {
            payloadOut.WriteBool(true);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteByte()
        {
            byte input = 7;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteByte(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                byte result = payloadIn.ReadByte();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]
        public void WriteByte()
        {
            payloadOut.WriteByte(3);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteSByte()
        {
            sbyte input = -4;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteSByte(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                sbyte result = payloadIn.ReadSByte();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]//(OperationsPerInvoke = Runs)]
        public void WriteSByte()
        {
            payloadOut.WriteSByte(-8);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteInt()
        {
            int input = -697;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteInt(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                int result = payloadIn.ReadInt();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]//(OperationsPerInvoke = Runs)]
        public void WriteInt()
        {
            payloadOut.WriteInt(-277);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteUInt()
        {
            uint input = 799;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteUInt(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                uint result = payloadIn.ReadUInt();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]
        public void WriteUInt()
        {
            payloadOut.WriteUInt(777);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteULong()
        {
            ulong input = 987827;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteULong(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                ulong result = payloadIn.ReadULong();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]//(OperationsPerInvoke = Runs)]
        public void WriteULong()
        {
            payloadOut.WriteULong(12812);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteLong()
        {
            long input = -273812;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteLong(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                long result = payloadIn.ReadLong();

                if (result == input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]
        public void WriteLong()
        {
            payloadOut.WriteLong(-273812);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteString8()
        {

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteString8(str8Input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                string result = payloadIn.ReadString8();

                if (result == str8Input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]//(OperationsPerInvoke = Runs)]
        public void WriteString8()
        {
            payloadOut.WriteString8(str8Input);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteString16()
        {

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteString16(str16Input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                string result = payloadIn.ReadString16();

                if (result == str16Input)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]
        public void WriteString16()
        {
            payloadOut.WriteString16(str16Input);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public void ReadAndWriteFloat()
        {
            float input = 3.14159f;

            BytePayload.FloatPrecision = 4;

            for (int i = 0; i < Runs; i++)
            {
                payloadOut.WriteFloat(input);
                var bufferIn = payloadOut.Pack();

                payloadIn._stream = BytePayload.ObtainStream(bufferIn);
                float result = payloadIn.ReadFloat();

                if (result == 3.1416f)
                {
                    payloadOut.Reset();
                    payloadIn.Reset();
                }
            }
        }

        [Benchmark]
        public void WriteFloat()
        {
            payloadOut.WriteFloat(3.14159f);
            payloadOut.Reset();
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public int ResolveUsingIf()
        {
            object input = new Vector2(0f, 0f);
            int f = 0;

            for (int i = 0; i < Runs; i++)
            {
                if (input is Vector2)
                    f++;
            }

            return f;
        }

        //[Benchmark(OperationsPerInvoke = Runs)]
        public int ResolveUsingDict()
        {
            int f = 0;

            for (int i = 0; i < Runs; i++)
            {
                if (BytePayload.GetResolver(typeof(Vector2)) != null)
                    f++;
            }

            return f;
        }

        public static void Main(string[] args) => BenchmarkRunner.Run<BytePayloadBenchmarks>();

    }

}
