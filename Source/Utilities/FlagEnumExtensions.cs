///////////////////////////////////////////////////////
/// Filename: FlagEnumExtensions.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace EppNet.Utilities
{

    public static class FlagEnumExtensions
    {

        private static void _EnsureEnum<T>()
        {
            Trace.Assert(typeof(T).IsEnum, $"Type '{typeof(T).Name}` is not a valid Enum!");
        }

        public static string ToListString<T>(this T input) where T : struct, Enum
        {

            StringBuilder builder = new();

            T[] allValues = Enum.GetValues<T>();
            int listedValues = 0;

            for (int i = 0; i < allValues.Length; i++)
            {
                T val = allValues[i];

                // This is equivalent to a flag of None
                if (Convert.ToInt32(val) == 0)
                    continue;

                if (input.HasFlag(val))
                {
                    listedValues++;
                    builder.Append(nameof(val));

                    if (++i < allValues.Length)
                        builder.Append(", ");
                }

            }

            if (listedValues == 0)
                builder.Append("None");

            return builder.ToString();

        }

        public static string ToString<T>(this T a) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            return nameof(a);
        }

        /*

        public static bool HandleOverlappingFlags<T>(this T a, T b, Dictionary<T, T> overlappingFlags) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            long aVal = Convert.ToInt64(a);
            long bVal = Convert.ToInt64(b);

            foreach (T key in overlappingFlags.Keys)
            {
                T value = overlappingFlags[key];
                long keyVal = Convert.ToInt64(key);
                long valueVal = Convert.ToInt64(value);

                if ((aVal & keyVal) != 0 && (bVal & keyVal != 0) (aVal & valueVal))
                
            }

        }*/

        public static bool IsFlagSet<T>(this T a, T b) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            long aVal = Convert.ToInt64(a);
            long bVal = Convert.ToInt64(b);

            return (aVal & bVal) != 0;
        }

        public static T SetFlag<T>(this T a, T b, bool on) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            long aVal = Convert.ToInt64(a);
            long bVal = Convert.ToInt64(b);

            if (on)
                aVal |= bVal;
            else
                aVal &= (~bVal);

            return (T)Enum.ToObject(a.GetType(), aVal);
        }

        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            foreach (T flag in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (value.IsFlagSet(flag))
                    yield return flag;
            }
        }

        public static T ClearFlags<T>(this T value, T flags) where T : struct, IConvertible, IComparable, IFormattable => value.SetFlag(flags, false);

        public static T CombineFlags<T>(this IEnumerable<T> flags) where T : struct, IConvertible, IComparable, IFormattable
        {
            _EnsureEnum<T>();
            long lVal = 0;
            foreach (T flag in flags)
            {
                long lFlag = Convert.ToInt64(flag);
                lVal |= lFlag;
            }

            return (T)Enum.ToObject(typeof(T), lVal);
        }

    }

}
