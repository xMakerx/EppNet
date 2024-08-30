///////////////////////////////////////////////////////
/// Filename: LongResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class LongResolver : Resolver<long>
    {

        public static readonly LongResolver Instance = new();

        static LongResolver()
            => BytePayload.AddResolver(typeof(long), Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out long output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadInt64LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, long input)
            => BinaryPrimitives.TryWriteInt64LittleEndian(payload.Stream.GetSpan(Size), input);

    }

    public static class LongResolverExtensions
    {

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, long input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, long[] input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, long[] input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteLong(this BytePayload payload, long input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteLongArray(this BytePayload payload, long[] input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt64(this BytePayload payload, long input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteInt64Array(this BytePayload payload, long[] input)
            => LongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a signed 64-bit integer from the stream.
        /// </summary>

        public static long ReadLong(this BytePayload payload)
        {
            LongResolver.Instance.Read(payload, out long output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer array from the stream.
        /// </summary>

        public static long[] ReadLongArray(this BytePayload payload)
        {
            LongResolver.Instance.Read(payload, out long[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer from the stream.
        /// </summary>
        public static long ReadInt64(this BytePayload payload)
        {
            LongResolver.Instance.Read(payload, out long output);
            return output;
        }

        /// <summary>
        /// Reads a signed 64-bit integer array from the stream.
        /// </summary>
        public static long[] ReadInt64Array(this BytePayload payload)
        {
            LongResolver.Instance.Read(payload, out long[] output);
            return output;
        }

    }

}
