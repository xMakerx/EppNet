///////////////////////////////////////////////////////
/// Filename: UShortResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]

    public class UShortResolver : Resolver<ushort>
    {

        public static readonly UShortResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out ushort output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadUInt16LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, ushort input)
            => BinaryPrimitives.TryWriteUInt16LittleEndian(payload.Stream.GetSpan(Size), input);
    }

    public static class UShortResolverExtensions
    {

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, ushort input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, ushort[] input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer collection to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this BytePayload payload, TCollection input) where TCollection : ICollection<ushort>
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, ushort[] input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUShort(this BytePayload payload, ushort input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUShortArray(this BytePayload payload, ushort[] input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt16(this BytePayload payload, ushort input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an unsigned 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteUInt16Array(this BytePayload payload, ushort[] input)
            => UShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads an unsigned 16-bit integer collection from the stream.
        /// </summary>
        /// <param name="input"></param>
        public static TCollection Read<TCollection>(this BytePayload payload) where TCollection : class, ICollection<ushort>, new()
        {
            UShortResolver.Instance.Read(payload, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>
        public static ushort ReadUShort(this BytePayload payload)
        {
            UShortResolver.Instance.Read(payload, out ushort output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer array from the stream.
        /// </summary>
        public static ushort[] ReadUShortArray(this BytePayload payload)
        {
            UShortResolver.Instance.Read(payload, out ushort[] output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer from the stream.
        /// </summary>
        public static ushort ReadUInt16(this BytePayload payload)
        {
            UShortResolver.Instance.Read(payload, out ushort output);
            return output;
        }

        /// <summary>
        /// Reads an unsigned 16-bit integer array from the stream.
        /// </summary>
        public static ushort[] ReadUInt16Array(this BytePayload payload)
        {
            UShortResolver.Instance.Read(payload, out ushort[] output);
            return output;
        }

    }

}
