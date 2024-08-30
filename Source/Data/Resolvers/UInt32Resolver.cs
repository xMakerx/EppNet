///////////////////////////////////////////////////////
/// Filename: UInt32Resolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class UInt32Resolver : Resolver<uint>
    {

        public static readonly UInt32Resolver Instance = new();

        static UInt32Resolver()
            => BytePayload.AddResolver(typeof(uint), Instance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out uint output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadUInt32LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, uint input)
            => BinaryPrimitives.TryWriteUInt32LittleEndian(payload.Stream.GetSpan(Size), input);
    }

    public static class UInt32ResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, uint input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, uint[] input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, uint[] input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt(this BytePayload payload, uint input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUIntArray(this BytePayload payload, uint[] input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt32(this BytePayload payload, uint input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 32-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt32Array(this BytePayload payload, uint[] input)
            => UInt32Resolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>

        public static uint ReadUInt(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer array from the stream.
        /// </summary>
        public static uint[] ReadUIntArray(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer from the stream.
        /// </summary>
        public static uint ReadUInt32(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 32-bit integer array from the stream.
        /// </summary>
        public static uint[] ReadUInt32Array(this BytePayload payload)
        {
            UInt32Resolver.Instance.Read(payload, out uint[] output);
            return output;
        }

    }

}

