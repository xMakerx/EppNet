///////////////////////////////////////////////////////
/// Filename: ColorResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class ColorResolver : Resolver<Color>
    {

        public static ColorResolver Instance = new();

        public ColorResolver() : base(size: 4, autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out Color output)
        {
            Span<byte> bytes = stackalloc byte[4];
            int index = 0;
            output = default;

            while (index < bytes.Length)
            {
                int result = payload.Stream.ReadByte();

                if (result == -1)
                    return ReadResult.Failed;

                bytes[index++] = (byte)result;
            }

            output = Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
            return ReadResult.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, Color input)
        {
            payload.Stream.WriteByte(input.A);
            payload.Stream.WriteByte(input.R);
            payload.Stream.WriteByte(input.G);
            payload.Stream.WriteByte(input.B);
            return true;
        }

    }

    public static class ColorResolverExtensions
    {

        /// <summary>
        /// Writes 4 unsigned 8-bit integers to the stream denoting the Color
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Color input)
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Color"/> to the stream. </br>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written.
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Color[] input)
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this BytePayload payload, TCollection input) where TCollection : ICollection<Color>
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Color"/> to the stream. </br>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written.
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Color[] input)
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes 4 unsigned 8-bit integers to the stream denoting the Color
        /// </summary>
        /// <param name="input"></param>
        public static void WriteColor(this BytePayload payload, Color input)
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        /// <param name="input"></param>
        public static void WriteColorArray(this BytePayload payload, Color[] input)
            => ColorResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a Guid collection from the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>

        public static TCollection Read<TCollection>(this BytePayload payload) where TCollection : class, ICollection<Color>, new()
        {
            ColorResolver.Instance.Read(payload, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads 16 unsigned 8-bit integers from the stream denoting a Color
        /// </summary>
        public static Color ReadColor(this BytePayload payload)
        {
            ColorResolver.Instance.Read(payload, out Color output);
            return output;
        }

        /// <summary>
        /// Reads a Color array from the stream<br/>
        /// See <see cref="Write(BytePayload, Color)"/> for more info on how each Color is written
        /// </summary>
        public static Color[] ReadGuidArray(this BytePayload payload)
        {
            ColorResolver.Instance.Read(payload, out Color[] output);
            return output;
        }

    }

}
