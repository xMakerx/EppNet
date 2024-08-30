///////////////////////////////////////////////////////
/// Filename: ShortResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class ShortResolver : Resolver<short>
    {

        public static readonly ShortResolver Instance = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out short output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            return BinaryPrimitives.TryReadInt16LittleEndian(buffer, out output) ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, short input)
            => BinaryPrimitives.TryWriteInt16LittleEndian(payload.Stream.GetSpan(Size), input);
    }

    public static class ShortResolverExtensions
    {

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, short input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, short[] input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, short[] input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteShort(this BytePayload payload, short input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteShortArray(this BytePayload payload, short[] input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt16(this BytePayload payload, short input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a signed 16-bit integer array to the stream.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteInt16Array(this BytePayload payload, short[] input)
            => ShortResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a signed 16-bit integer from the stream.
        /// </summary>
        public static short ReadShort(this BytePayload payload)
        {
            ShortResolver.Instance.Read(payload, out short output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer array from the stream.
        /// </summary>
        public static short[] ReadShortArray(this BytePayload payload)
        {
            ShortResolver.Instance.Read(payload, out short[] output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer from the stream.
        /// </summary>
        public static short ReadInt16(this BytePayload payload)
        {
            ShortResolver.Instance.Read(payload, out short output);
            return output;
        }

        /// <summary>
        /// Reads a signed 16-bit integer array from the stream.
        /// </summary>
        public static short[] ReadInt16Array(this BytePayload payload)
        {
            ShortResolver.Instance.Read(payload, out short[] output);
            return output;
        }

    }

}

