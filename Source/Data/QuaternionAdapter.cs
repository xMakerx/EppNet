///////////////////////////////////////////////////////
/// Filename: QuaternionAdapter.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;


namespace EppNet.Data
{

    public struct QuaternionAdapter
    {

        /// <summary>
        /// Magic number for quaternion quantization
        /// </summary>
        public const float ByteQuantizer = 127.5f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Normalize(QuaternionAdapter quat)
            => new(
                quat.X / quat.Length(),
                quat.Y / quat.Length(),
                quat.Z / quat.Length(),
                quat.W / quat.Length()
            );

        /// <summary>
        /// Maps a floating point number from [-1, 1] to [0, 255]<br/>
        /// <b>NOTE:</b> Input float must be normalized!
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Byte in domain [0, 255]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte Quantize(float input)
            => (byte)MathF.Round((input + 1.0f) * ByteQuantizer);

        /// <summary>
        /// Maps a byte value from [0, 255] back to [-1, 1]
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Float in domain [-1, 1]</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dequantize(byte input)
            => (input / ByteQuantizer) - 1.0f;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Quantized(QuaternionAdapter input)
        {
            QuaternionAdapter result = Normalize(input);
            Span<float> floats = stackalloc float[4];
            floats[0] = result.X;
            floats[1] = result.Y;
            floats[2] = result.Z;
            floats[3] = result.W;

            for (int i = 0; i < floats.Length; i++)
                floats[i] = Quantize(floats[i]);

            return new QuaternionAdapter(floats[0], floats[1],
                floats[2], floats[3]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static QuaternionAdapter Dequantized(QuaternionAdapter input)
        {
            Span<float> result = stackalloc float[4];
            result[0] = input.X;
            result[1] = input.Y;
            result[2] = input.Z;
            result[3] = input.W;

            for (int i = 0; i < 4; i++)
                result[i] = Dequantize((byte)result[i]);

            int largestIndex = 0;
            float largest = MathF.Abs(result[0]);
            bool negative = result[0] < 0;

            for (int i = 1; i < 4; i++)
            {
                if (MathF.Abs(result[i]) > largest)
                {
                    largestIndex = i;
                    largest = MathF.Abs(result[i]);
                    negative = result[i] < 0;
                }
            }

            result[largestIndex] = 0;

            largest = MathF.Sqrt(1.0f - (MathF.Pow(result[0], 2f) + MathF.Pow(result[1], 2f)
                + MathF.Pow(result[2], 2f) + MathF.Pow(result[3], 2f)));

            result[largestIndex] = largest * (negative ? -1 : 1);
            return new QuaternionAdapter(result[0], result[1],
                result[2], result[3]);
        }

        public float X;
        public float Y;
        public float Z;
        public float W;

        public QuaternionAdapter(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly float Length()
        {
            float dotProd = (X * X) + (Y * Y) + (Z * Z) + (W * W);
            return MathF.Sqrt(dotProd);
        }

        public float this[int index]
        {

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get
            {
                return index switch
                {
                    0 => X,
                    1 => Y,
                    2 => Z,
                    3 => W,
                    _ => throw new System.ArgumentOutOfRangeException(nameof(index)),
                };
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                this = index switch
                {
                    0 => new(value, Y, Z, W),
                    1 => new(X, value, Z, W),
                    2 => new(X, Y, value, W),
                    3 => new(X, Y, Z, value),
                    _ => throw new System.ArgumentOutOfRangeException(nameof(index))
                };
            }

        }

        public static implicit operator System.Numerics.Quaternion(QuaternionAdapter a)
            => new System.Numerics.Quaternion(a.X, a.Y, a.Z, a.W);

        public static explicit operator QuaternionAdapter(System.Numerics.Quaternion q)
            => new(q.X, q.Y, q.Z, q.W);
    }

}
