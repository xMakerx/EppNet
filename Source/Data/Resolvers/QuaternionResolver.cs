///////////////////////////////////////////////////////
/// Filename: QuaternionResolver.cs
/// Date: August 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{
    
    public class QuaternionResolver : Resolver<Quaternion>
    {

        public static readonly QuaternionResolver Instance = new();

        static QuaternionResolver()
            => BytePayload.AddResolver(typeof(Quaternion), Instance);

        /// <summary>
        /// Identity quaternions send this value to save bandwidth and computation time
        /// </summary>
        public const byte IdentityHeader = 32;

        /// <summary>
        /// Zero quaternions send this value to send bandwidth and computation time
        /// </summary>
        public const byte ZeroHeader = 16;

        /// <summary>
        /// Whether or not to use byte quantization for packing Quaternions<br/>
        /// This defaults to true as the cost per quaternion with quantization is 4 bytes total
        /// </summary>
        public bool ByteQuantization { set; get; } = true;

        public QuaternionResolver() : base(sizeof(byte), false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        protected override bool _Internal_Read(BytePayload payload, out Quaternion output)
        {
            // Let's consider the header
            byte header = payload.ReadByte();
            payload.Advance(1);

            if (header == IdentityHeader || header == ZeroHeader)
            {
                // Yay! Just match the quaternion and return!
                output = header == IdentityHeader ? Quaternion.Identity : Quaternion.Zero;
                return true;
            }

            // We didn't have the easy way out. Consider the most significant bit.
            bool negative = (header & (1 << 7)) != 0;
            int largestIndex = byte.TrailingZeroCount(negative ? (byte)(header & ~(1 << 7)) : header);
            Span<float> components = stackalloc float[4];

            for (int i = 0; i < components.Length; i++)
            {
                if (i == largestIndex)
                {
                    components[i] = 0f;
                    continue;
                }

                if (!ByteQuantization)
                {
                    // Read the float like any other (this auto advances)
                    FloatResolver.Instance.Read(payload, out components[i]);
                    continue;
                }

                // Byte dequantization
                components[i] = FastMath.Dequantize(payload.ReadByte());
                payload.Advance(1);
            }

            float sumOfSquares = 1.0f - (MathF.Pow(components[0], 2) + MathF.Pow(components[1], 2)
                + MathF.Pow(components[2], 2) + MathF.Pow(components[3], 2));

            // Generate the largest value
            components[largestIndex] = MathF.Sqrt(sumOfSquares) * (negative ? -1 : 1);
            output = new();

            for (int i = 0; i < components.Length; i++)
                output[i] = components[i];

            return true;
        }


        /// <summary>
        /// This is an implementation of the Smallest Three algorithm<br/>
        /// A byte header is sent indicating the index of largest component, and
        /// if the sign is 
        /// If <see cref="ByteQuantization"/>:<br/>
        /// - 4 bytes are sent (1 byte header indicating index of largest 
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, Quaternion input)
        {

            // Identity and zero quats use a single byte
            if (input == Quaternion.Identity || input == Quaternion.Zero)
            {
                payload.Stream.WriteByte(input == Quaternion.Identity ? IdentityHeader : ZeroHeader);
                payload.Stream.Advance(1);
                return true;
            }

            Quaternion normalized = Quaternion.Normalize(input);

            int largestIndex = 0;
            float largest = MathF.Abs(normalized.X);
            bool negative = normalized.X < 0;

            for (int i = 1; i < 4; i++)
            {
                float value = MathF.Abs(normalized[i]);

                if (value > largest)
                {
                    negative = normalized[i] < 0;
                    largest = value;
                    largestIndex = i;
                }
            }

            // Header includes the following:
            // a bit turned on indicating the largest index
            // and the leading bit on if the largest value is negative
            byte header = (byte)(1 << (byte)largestIndex);

            if (negative)
                header |= 1 << 7;

            payload.Stream.WriteByte(header);
            payload.Stream.Advance(1);

            if (ByteQuantization)
            {
                Span<byte> bytes = stackalloc byte[3];
                int byteIndex = 0;
                int quatIndex = 0;

                while (byteIndex < bytes.Length)
                {
                    if (quatIndex == largestIndex)
                    {
                        quatIndex++;
                        continue;
                    }

                    // Quantize the float into a byte
                    bytes[byteIndex] = FastMath.Quantize(normalized[quatIndex++]);

                    // Write the byte to the stream
                    payload.Stream.WriteByte(bytes[byteIndex]);
                    payload.Stream.Advance(1);

                    // Advance to the next index
                    byteIndex++;
                }
            }
            else
            {
                // Byte quantization is off. Let's send the floats
                for (int i = 0; i < 4; i++)
                {
                    if (i == largestIndex)
                        break;

                    FloatResolver.Instance.Write(payload, normalized[i]);
                }
            }

            return true;
        }
    }

    public static class QuaternionResolverExtensions
    {

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuat(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuatArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes a normalized <see cref="Quaternion"/> to the stream using the Smallest Three algorithm.<br/>
        /// Cost varies based on if <see cref="QuaternionResolver.ByteQuantization"/> is enabled, and the
        /// particular quaternion sent. <see cref="Quaternion.Zero"/> and <see cref="Quaternion.Identity"/> are
        /// the cheapest at 1 byte each. <br/><br/>
        /// With quantization enabled, the maximum size for a quaternion is 4 bytes; otherwise, it sends
        /// <br/>one single per value which is 4 bytes per value or 33 bytes including the header.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternion(this BytePayload payload, Quaternion input)
            => QuaternionResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="Quaternion"/> to the stream<br/>
        /// See <see cref="Write(BytePayload, Quaternion)"/> for more information on how each quaternion is written
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteQuaternionArray(this BytePayload payload, Quaternion[] input)
            => QuaternionResolver.Instance.Write(payload, input);


        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuat(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> array from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuatArray(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion[] output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion ReadQuaternion(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion output);
            return output;
        }

        /// <summary>
        /// Reads a normalized <see cref="Quaternion"/> from the stream.<br/>
        /// For more information on how these are encoded, see: <see cref="Write(BytePayload, Quaternion)"/>
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>

        public static Quaternion[] ReadQuaternionArray(this BytePayload payload)
        {
            QuaternionResolver.Instance.Read(payload, out Quaternion[] output);
            return output;
        }

    }

}