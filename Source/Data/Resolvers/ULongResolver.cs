///////////////////////////////////////////////////////
/// Filename: ULongResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class ULongResolver : Resolver<ulong>
    {

        public static readonly ULongResolver Instance = new();

        static ULongResolver()
            => BytePayload.AddResolver(typeof(ulong), Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out ulong output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadUInt64LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, ulong input)
            => BinaryPrimitives.TryWriteUInt64LittleEndian(payload.Stream.GetSpan(Size), input);
    }

    public static class ULongResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, ulong input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, ulong[] input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, ulong[] input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteULong(this BytePayload payload, ulong input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteULongArray(this BytePayload payload, ulong[] input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt64(this BytePayload payload, ulong input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 64-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteUInt64Array(this BytePayload payload, ulong[] input)
            => ULongResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>
        public static ulong ReadULong(this BytePayload payload)
        {
            ULongResolver.Instance.Read(payload, out ulong output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer array from the stream.
        /// </summary>
        public static ulong[] ReadULongArray(this BytePayload payload)
        {
            ULongResolver.Instance.Read(payload, out ulong[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer from the stream.
        /// </summary>
        public static ulong ReadUInt64(this BytePayload payload)
        {
            ULongResolver.Instance.Read(payload, out ulong output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 64-bit integer array from the stream.
        /// </summary>
        public static ulong[] ReadUInt64Array(this BytePayload payload)
        {
            ULongResolver.Instance.Read(payload, out ulong[] output);
            return output;
        }

    }

}
