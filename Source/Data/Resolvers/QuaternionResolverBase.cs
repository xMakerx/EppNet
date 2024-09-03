///////////////////////////////////////////////////////
/// Filename: QuaternionResolver.cs
/// Date: August 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public abstract class QuaternionResolverBase<T> : Resolver<T> where T : unmanaged, IEquatable<T>
    {
        /// <summary>
        /// Whether or not to use byte quantization for packing Quaternions<br/>
        /// This defaults to true as the cost per quaternion with quantization is 4 bytes total
        /// </summary>
        public static bool ByteQuantization { set; get; } = true;

        /// <summary>
        /// Identity quaternions send this value to save bandwidth and computation time
        /// </summary>
        public const byte IdentityHeader = 32;

        /// <summary>
        /// Zero quaternions send this value to send bandwidth and computation time
        /// </summary>
        public const byte ZeroHeader = 16;

        public T Zero { protected set; get; }
        public T Identity { protected set; get; }

        protected QuaternionResolverBase() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]

        protected override ReadResult _Internal_Read(BytePayload payload, out T output)
        {
            // Let's consider the header
            byte header = payload.ReadByte();

            if (header == IdentityHeader || header == ZeroHeader)
            {
                // Yay! Just match the quaternion and return!
                output = header == IdentityHeader ? Identity : Zero;
                return ReadResult.Success;
            }

            // We didn't have the easy way out. Consider the most significant bit.
            bool negative = (header & (1 << 7)) != 0;
            int largestIndex = BitOperations.TrailingZeroCount(negative ? (byte)(header & ~(1 << 7)) : header);
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
                components[i] = QuaternionAdapter.Dequantize(payload.ReadByte());
            }

            float sumOfSquares = 1.0f - (MathF.Pow(components[0], 2) + MathF.Pow(components[1], 2)
                + MathF.Pow(components[2], 2) + MathF.Pow(components[3], 2));

            // Generate the largest value
            components[largestIndex] = MathF.Sqrt(sumOfSquares) * (negative ? -1 : 1);

            QuaternionAdapter adapter = new(components[0], 
                components[1], 
                components[2], 
                components[3]);

            output = FromAdapter(adapter);

            return ReadResult.Success;
        }

        public abstract QuaternionAdapter ToAdapter(T input);

        public abstract T FromAdapter(QuaternionAdapter adapter);


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
        protected override bool _Internal_Write(BytePayload payload, T input)
        {

            // Identity and zero quats use a single byte
            if (input.Equals(Identity) || input.Equals(Zero))
            {
                payload.Stream.WriteByte(input.Equals(Identity)
                    ? IdentityHeader
                    : ZeroHeader);
                return true;
            }

            QuaternionAdapter normalized = QuaternionAdapter.Normalize(ToAdapter(input));

            int largestIndex = 0;
            float largest = MathF.Abs(normalized.X);
            bool negative = normalized.X < 0;

            for (int i = 1; i < 4; i++)
            {
                float f = normalized[i];
                float value = MathF.Abs(f);

                if (value > largest)
                {
                    negative = f < 0;
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
                    bytes[byteIndex] = QuaternionAdapter.Quantize(normalized[quatIndex++]);

                    // Write the byte to the stream
                    payload.Stream.WriteByte(bytes[byteIndex]);

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

}