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
        private static readonly double[] _roundLookup = CreateRoundLookup();

        public static double GetTenPow(int a) => _roundLookup[a];

        private static double[] CreateRoundLookup()
        {
            double[] result = new double[15];
            for (int i = 0; i < result.Length; i++)
            {
                double r = Math.Pow(10, i);
                result[i] = r;
            }

            return result;
        }

        public static double Round(double value) => Math.Floor(value + 0.5);

        public static double Round(double value, int decimalPlaces)
        {
            double adjustment = _roundLookup[decimalPlaces];
            return Round(value * adjustment) / adjustment;
        }

    }
}
