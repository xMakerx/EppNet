///////////////////////////////////////////////////////
/// Filename: FastMath.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty, Will Calderwood
/// Link: Modified adaptation of 
/// https://stackoverflow.com/a/48448292
///////////////////////////////////////////////////////
using System;

namespace EppNet.Utilities
{
    public static class FastMath
    {
        /// <summary>
        /// How many decimal places to cache
        /// </summary>
        public const int CachedDecimals = 15;

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

    }
}
