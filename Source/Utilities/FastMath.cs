///////////////////////////////////////////////////////
/// Filename: FastMath.cs
/// Date: September 10, 2023
/// Author: Maverick Liberty, Will Calderwood
/// Link: Modified adaptation of 
/// https://stackoverflow.com/a/48448292
///////////////////////////////////////////////////////
using System;
using System.Collections.Generic;

namespace EppNet.Utilities
{
    public static class FastMath
    {
        private static readonly Dictionary<double, double> _reciprocalTable = new Dictionary<double, double>();
        private static readonly double[] _roundLookup = CreateRoundLookup();

        public static double GetReciprocal(double a)
        {
            bool needs_gen = _reciprocalTable.TryGetValue(a, out double r);

            if (needs_gen)
            {
                r = Math.ReciprocalEstimate(a);
                _reciprocalTable[a] = r;
            }

            return r;
        }

        private static double[] CreateRoundLookup()
        {
            double[] result = new double[15];
            for (int i = 0; i < result.Length; i++)
            {
                double r = Math.Pow(10, i);
                result[i] = r;

                // Generates the reciprocal
                GetReciprocal(r);
            }

            return result;
        }
        public static int Div(int a, int b) => (int)(a * GetReciprocal((double)b));
        public static float Div(float a, float b) => (float) (a * GetReciprocal((double)b));
        public static double Div(double a, double b) => a * GetReciprocal(b);

        public static double Round(double value) => Math.Floor(value + 0.5);

        public static double Round(double value, int decimalPlaces)
        {
            double adjustment = _roundLookup[decimalPlaces];
            return Round(value * adjustment) / adjustment;
        }

        public static int RoundInt(double value, int decimalPlaces)
        {
            double adjustment = _roundLookup[decimalPlaces];
            return (int) Round(value * adjustment);
        }

    }
}
