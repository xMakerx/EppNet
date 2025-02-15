///////////////////////////////////////////////////////
/// Filename: Int32Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class Int32Resolver : Resolver<int>
    {

        public static readonly Int32Resolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out int output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadInt32LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, int input)
            => BinaryPrimitives.TryWriteInt32LittleEndian(payload.Stream.GetSpan(Size), input);
    }

    public static class Int32ResolverExtensions
    {

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, int input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, int[] input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, int[] input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt(this BytePayload payload, int input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteIntArray(this BytePayload payload, int[] input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt32(this BytePayload payload, int input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 32-bit integer array to the stream.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteInt32Array(this BytePayload payload, int[] input)
            => Int32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a signed 32-bit integer from the stream.
        /// </summary>

        public static int ReadInt(this BytePayload payload)
        {
            Int32Resolver.Instance.Read(payload, out int output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream.
        /// </summary>
        public static int[] ReadIntArray(this BytePayload payload)
        {
            Int32Resolver.Instance.Read(payload, out int[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer from the stream.
        /// </summary>
        public static int ReadInt32(this BytePayload payload)
        {
            Int32Resolver.Instance.Read(payload, out int output);
            return output;
        }

        /// <summary>
        /// Reads a signed 32-bit integer array from the stream.
        /// </summary>
        public static int[] ReadInt32Array(this BytePayload payload)
        {
            Int32Resolver.Instance.Read(payload, out int[] output);
            return output;
        }

    }

}

