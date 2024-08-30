///////////////////////////////////////////////////////
/// Filename: FastMath.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty, Will Calderwood
/// Link: Modified adaptation of 
/// https://stackoverflow.com/a/48448292
///////////////////////////////////////////////////////
using System;
using System.Numerics;

namespace EppNet.Utilities
{
    public static class FastMath
    {
        /// <summary>
        /// How many decimal places to cache
        /// </summary>
        public const int CachedDecimals = 15;

        /// <summary>
        /// Magic number for quaternion quantization
        /// </summary>
        public const float ByteQuantizer = 127.5f;

        private static readonly double[] _roundLookup = CreateRoundLookup();

        /// <summary>
        /// Essentially a wrapper around <see cref="Math.Pow(double, double)"/> with 10^index<br/>
        /// Indices from 0 to <see cref="CachedDecimals"/> - 1 are cached.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>Math.Pow(10, index)</returns>
        public static double GetTenPow(int index)
        {
            if (0 <= index && index < CachedDecimals)
                return _roundLookup[index];

            return Math.Pow(10, index);
        }

        private static double[] CreateRoundLookup()
        {
            double[] result = new double[CachedDecimals];

            for (int i = 0; i < result.Length; i++)
                result[i] = Math.Pow(10, i);

            return result;
        }

        /// <summary>
        /// Rounds the specified number to the specified decimal places<br/>
        /// - Rounding to 0 decimal places returns 1
        /// - Rounding to <0 decimal places returns the provided number
        /// </summary>
        /// <returns></returns>
        public static double Round(this float f, int decimals) => ((double)f).Round(decimals);


        /// <summary>
        /// Rounds the specified number to the specified decimal places<br/>
        /// - Rounding to 0 decimal places returns 1
        /// - Rounding to <0 decimal places returns the provided number
        /// </summary>
        /// <returns></returns>

        public static double Round(this double d, int decimals)
        {
            double result;

            if (decimals == 0)
            {
                // Just return 1 as 10^0 is 1.
                result = 1d;
            }
            else if (decimals < 0)
            {
                // We don't support rounding to negative decimal places.
                result = d;
            }
            else
            {
                double adjustment = GetTenPow(decimals);
                result = Math.Floor((d * adjustment) + 0.5d) / adjustment;
            }

            return result;
        }

        /// <summary>
        /// Maps a floating point number from [-1, 1] to [0, 255]<br/>
        /// <b>NOTE:</b> Input float must be normalized!
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Byte in domain [0, 255]</returns>
        public static byte Quantize(float input)
            => (byte)MathF.Round((input + 1.0f) * ByteQuantizer);

        /// <summary>
        /// Maps a byte value from [0, 255] back to [-1, 1]
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Float in domain [-1, 1]</returns>
        public static float Dequantize(byte input)
            => (input / ByteQuantizer) - 1.0f;

        public static Quaternion Quantized(Quaternion input)
        {
            Quaternion result = Quaternion.Normalize(input);

            for (int i = 0; i < 4; i++)
                result[i] = Quantize(result[i]);

            return result;
        }

        public static Quaternion Dequantized(Quaternion input)
        {
            Quaternion result = input;

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
            return result;
        }

    }
}
