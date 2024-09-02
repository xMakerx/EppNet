///////////////////////////////////////////////////////
/// Filename: NumberExtensions.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
using System;
using System.Collections.Generic;


namespace EppNet.Utilities
{

    /// <summary>
    /// Absolutely do not ask me about this garbage.
    /// This is used to convert a number from one type to another without boxing
    /// using delegate magic. I generated the code
    /// </summary>
    public static class NumberExtensions
    {


        #region Byte Conversions
        private static byte _Internal_ByteToByte(byte input)
        {
            return (byte)input;
        }


        private static Func<byte, byte> _byte2Byte = _Internal_ByteToByte;

        private static byte _Internal_SbyteToByte(sbyte input)
        {
            return (byte)input;
        }


        private static Func<sbyte, byte> _sbyte2Byte = _Internal_SbyteToByte;

        private static byte _Internal_UshortToByte(ushort input)
        {
            return (byte)input;
        }


        private static Func<ushort, byte> _ushort2Byte = _Internal_UshortToByte;

        private static byte _Internal_ShortToByte(short input)
        {
            return (byte)input;
        }


        private static Func<short, byte> _short2Byte = _Internal_ShortToByte;

        private static byte _Internal_UintToByte(uint input)
        {
            return (byte)input;
        }


        private static Func<uint, byte> _uint2Byte = _Internal_UintToByte;

        private static byte _Internal_IntToByte(int input)
        {
            return (byte)input;
        }


        private static Func<int, byte> _int2Byte = _Internal_IntToByte;

        private static byte _Internal_UlongToByte(ulong input)
        {
            return (byte)input;
        }


        private static Func<ulong, byte> _ulong2Byte = _Internal_UlongToByte;

        private static byte _Internal_LongToByte(long input)
        {
            return (byte)input;
        }


        private static Func<long, byte> _long2Byte = _Internal_LongToByte;

        private static byte _Internal_FloatToByte(float input)
        {
            return (byte)input;
        }


        private static Func<float, byte> _float2Byte = _Internal_FloatToByte;

        private static byte _Internal_DoubleToByte(double input)
        {
            return (byte)input;
        }


        private static Func<double, byte> _double2Byte = _Internal_DoubleToByte;
        #endregion

        #region Sbyte Conversions
        private static sbyte _Internal_ByteToSbyte(byte input)
        {
            return (sbyte)input;
        }


        private static Func<byte, sbyte> _byte2Sbyte = _Internal_ByteToSbyte;

        private static sbyte _Internal_SbyteToSbyte(sbyte input)
        {
            return (sbyte)input;
        }


        private static Func<sbyte, sbyte> _sbyte2Sbyte = _Internal_SbyteToSbyte;

        private static sbyte _Internal_UshortToSbyte(ushort input)
        {
            return (sbyte)input;
        }


        private static Func<ushort, sbyte> _ushort2Sbyte = _Internal_UshortToSbyte;

        private static sbyte _Internal_ShortToSbyte(short input)
        {
            return (sbyte)input;
        }


        private static Func<short, sbyte> _short2Sbyte = _Internal_ShortToSbyte;

        private static sbyte _Internal_UintToSbyte(uint input)
        {
            return (sbyte)input;
        }


        private static Func<uint, sbyte> _uint2Sbyte = _Internal_UintToSbyte;

        private static sbyte _Internal_IntToSbyte(int input)
        {
            return (sbyte)input;
        }


        private static Func<int, sbyte> _int2Sbyte = _Internal_IntToSbyte;

        private static sbyte _Internal_UlongToSbyte(ulong input)
        {
            return (sbyte)input;
        }


        private static Func<ulong, sbyte> _ulong2Sbyte = _Internal_UlongToSbyte;

        private static sbyte _Internal_LongToSbyte(long input)
        {
            return (sbyte)input;
        }


        private static Func<long, sbyte> _long2Sbyte = _Internal_LongToSbyte;

        private static sbyte _Internal_FloatToSbyte(float input)
        {
            return (sbyte)input;
        }


        private static Func<float, sbyte> _float2Sbyte = _Internal_FloatToSbyte;

        private static sbyte _Internal_DoubleToSbyte(double input)
        {
            return (sbyte)input;
        }


        private static Func<double, sbyte> _double2Sbyte = _Internal_DoubleToSbyte;
        #endregion

        #region Ushort Conversions
        private static ushort _Internal_ByteToUshort(byte input)
        {
            return (ushort)input;
        }


        private static Func<byte, ushort> _byte2Ushort = _Internal_ByteToUshort;

        private static ushort _Internal_SbyteToUshort(sbyte input)
        {
            return (ushort)input;
        }


        private static Func<sbyte, ushort> _sbyte2Ushort = _Internal_SbyteToUshort;

        private static ushort _Internal_UshortToUshort(ushort input)
        {
            return (ushort)input;
        }


        private static Func<ushort, ushort> _ushort2Ushort = _Internal_UshortToUshort;

        private static ushort _Internal_ShortToUshort(short input)
        {
            return (ushort)input;
        }


        private static Func<short, ushort> _short2Ushort = _Internal_ShortToUshort;

        private static ushort _Internal_UintToUshort(uint input)
        {
            return (ushort)input;
        }


        private static Func<uint, ushort> _uint2Ushort = _Internal_UintToUshort;

        private static ushort _Internal_IntToUshort(int input)
        {
            return (ushort)input;
        }


        private static Func<int, ushort> _int2Ushort = _Internal_IntToUshort;

        private static ushort _Internal_UlongToUshort(ulong input)
        {
            return (ushort)input;
        }


        private static Func<ulong, ushort> _ulong2Ushort = _Internal_UlongToUshort;

        private static ushort _Internal_LongToUshort(long input)
        {
            return (ushort)input;
        }


        private static Func<long, ushort> _long2Ushort = _Internal_LongToUshort;

        private static ushort _Internal_FloatToUshort(float input)
        {
            return (ushort)input;
        }


        private static Func<float, ushort> _float2Ushort = _Internal_FloatToUshort;

        private static ushort _Internal_DoubleToUshort(double input)
        {
            return (ushort)input;
        }


        private static Func<double, ushort> _double2Ushort = _Internal_DoubleToUshort;
        #endregion

        #region Short Conversions
        private static short _Internal_ByteToShort(byte input)
        {
            return (short)input;
        }


        private static Func<byte, short> _byte2Short = _Internal_ByteToShort;

        private static short _Internal_SbyteToShort(sbyte input)
        {
            return (short)input;
        }


        private static Func<sbyte, short> _sbyte2Short = _Internal_SbyteToShort;

        private static short _Internal_UshortToShort(ushort input)
        {
            return (short)input;
        }


        private static Func<ushort, short> _ushort2Short = _Internal_UshortToShort;

        private static short _Internal_ShortToShort(short input)
        {
            return (short)input;
        }


        private static Func<short, short> _short2Short = _Internal_ShortToShort;

        private static short _Internal_UintToShort(uint input)
        {
            return (short)input;
        }


        private static Func<uint, short> _uint2Short = _Internal_UintToShort;

        private static short _Internal_IntToShort(int input)
        {
            return (short)input;
        }


        private static Func<int, short> _int2Short = _Internal_IntToShort;

        private static short _Internal_UlongToShort(ulong input)
        {
            return (short)input;
        }


        private static Func<ulong, short> _ulong2Short = _Internal_UlongToShort;

        private static short _Internal_LongToShort(long input)
        {
            return (short)input;
        }


        private static Func<long, short> _long2Short = _Internal_LongToShort;

        private static short _Internal_FloatToShort(float input)
        {
            return (short)input;
        }


        private static Func<float, short> _float2Short = _Internal_FloatToShort;

        private static short _Internal_DoubleToShort(double input)
        {
            return (short)input;
        }


        private static Func<double, short> _double2Short = _Internal_DoubleToShort;
        #endregion

        #region Uint Conversions
        private static uint _Internal_ByteToUint(byte input)
        {
            return (uint)input;
        }


        private static Func<byte, uint> _byte2Uint = _Internal_ByteToUint;

        private static uint _Internal_SbyteToUint(sbyte input)
        {
            return (uint)input;
        }


        private static Func<sbyte, uint> _sbyte2Uint = _Internal_SbyteToUint;

        private static uint _Internal_UshortToUint(ushort input)
        {
            return (uint)input;
        }


        private static Func<ushort, uint> _ushort2Uint = _Internal_UshortToUint;

        private static uint _Internal_ShortToUint(short input)
        {
            return (uint)input;
        }


        private static Func<short, uint> _short2Uint = _Internal_ShortToUint;

        private static uint _Internal_UintToUint(uint input)
        {
            return (uint)input;
        }


        private static Func<uint, uint> _uint2Uint = _Internal_UintToUint;

        private static uint _Internal_IntToUint(int input)
        {
            return (uint)input;
        }


        private static Func<int, uint> _int2Uint = _Internal_IntToUint;

        private static uint _Internal_UlongToUint(ulong input)
        {
            return (uint)input;
        }


        private static Func<ulong, uint> _ulong2Uint = _Internal_UlongToUint;

        private static uint _Internal_LongToUint(long input)
        {
            return (uint)input;
        }


        private static Func<long, uint> _long2Uint = _Internal_LongToUint;

        private static uint _Internal_FloatToUint(float input)
        {
            return (uint)input;
        }


        private static Func<float, uint> _float2Uint = _Internal_FloatToUint;

        private static uint _Internal_DoubleToUint(double input)
        {
            return (uint)input;
        }


        private static Func<double, uint> _double2Uint = _Internal_DoubleToUint;
        #endregion

        #region Int Conversions
        private static int _Internal_ByteToInt(byte input)
        {
            return (int)input;
        }


        private static Func<byte, int> _byte2Int = _Internal_ByteToInt;

        private static int _Internal_SbyteToInt(sbyte input)
        {
            return (int)input;
        }


        private static Func<sbyte, int> _sbyte2Int = _Internal_SbyteToInt;

        private static int _Internal_UshortToInt(ushort input)
        {
            return (int)input;
        }


        private static Func<ushort, int> _ushort2Int = _Internal_UshortToInt;

        private static int _Internal_ShortToInt(short input)
        {
            return (int)input;
        }


        private static Func<short, int> _short2Int = _Internal_ShortToInt;

        private static int _Internal_UintToInt(uint input)
        {
            return (int)input;
        }


        private static Func<uint, int> _uint2Int = _Internal_UintToInt;

        private static int _Internal_IntToInt(int input)
        {
            return (int)input;
        }


        private static Func<int, int> _int2Int = _Internal_IntToInt;

        private static int _Internal_UlongToInt(ulong input)
        {
            return (int)input;
        }


        private static Func<ulong, int> _ulong2Int = _Internal_UlongToInt;

        private static int _Internal_LongToInt(long input)
        {
            return (int)input;
        }


        private static Func<long, int> _long2Int = _Internal_LongToInt;

        private static int _Internal_FloatToInt(float input)
        {
            return (int)input;
        }


        private static Func<float, int> _float2Int = _Internal_FloatToInt;

        private static int _Internal_DoubleToInt(double input)
        {
            return (int)input;
        }


        private static Func<double, int> _double2Int = _Internal_DoubleToInt;
        #endregion

        #region Ulong Conversions
        private static ulong _Internal_ByteToUlong(byte input)
        {
            return (ulong)input;
        }


        private static Func<byte, ulong> _byte2Ulong = _Internal_ByteToUlong;

        private static ulong _Internal_SbyteToUlong(sbyte input)
        {
            return (ulong)input;
        }


        private static Func<sbyte, ulong> _sbyte2Ulong = _Internal_SbyteToUlong;

        private static ulong _Internal_UshortToUlong(ushort input)
        {
            return (ulong)input;
        }


        private static Func<ushort, ulong> _ushort2Ulong = _Internal_UshortToUlong;

        private static ulong _Internal_ShortToUlong(short input)
        {
            return (ulong)input;
        }


        private static Func<short, ulong> _short2Ulong = _Internal_ShortToUlong;

        private static ulong _Internal_UintToUlong(uint input)
        {
            return (ulong)input;
        }


        private static Func<uint, ulong> _uint2Ulong = _Internal_UintToUlong;

        private static ulong _Internal_IntToUlong(int input)
        {
            return (ulong)input;
        }


        private static Func<int, ulong> _int2Ulong = _Internal_IntToUlong;

        private static ulong _Internal_UlongToUlong(ulong input)
        {
            return (ulong)input;
        }


        private static Func<ulong, ulong> _ulong2Ulong = _Internal_UlongToUlong;

        private static ulong _Internal_LongToUlong(long input)
        {
            return (ulong)input;
        }


        private static Func<long, ulong> _long2Ulong = _Internal_LongToUlong;

        private static ulong _Internal_FloatToUlong(float input)
        {
            return (ulong)input;
        }


        private static Func<float, ulong> _float2Ulong = _Internal_FloatToUlong;

        private static ulong _Internal_DoubleToUlong(double input)
        {
            return (ulong)input;
        }


        private static Func<double, ulong> _double2Ulong = _Internal_DoubleToUlong;
        #endregion

        #region Long Conversions
        private static long _Internal_ByteToLong(byte input)
        {
            return (long)input;
        }


        private static Func<byte, long> _byte2Long = _Internal_ByteToLong;

        private static long _Internal_SbyteToLong(sbyte input)
        {
            return (long)input;
        }


        private static Func<sbyte, long> _sbyte2Long = _Internal_SbyteToLong;

        private static long _Internal_UshortToLong(ushort input)
        {
            return (long)input;
        }


        private static Func<ushort, long> _ushort2Long = _Internal_UshortToLong;

        private static long _Internal_ShortToLong(short input)
        {
            return (long)input;
        }


        private static Func<short, long> _short2Long = _Internal_ShortToLong;

        private static long _Internal_UintToLong(uint input)
        {
            return (long)input;
        }


        private static Func<uint, long> _uint2Long = _Internal_UintToLong;

        private static long _Internal_IntToLong(int input)
        {
            return (long)input;
        }


        private static Func<int, long> _int2Long = _Internal_IntToLong;

        private static long _Internal_UlongToLong(ulong input)
        {
            return (long)input;
        }


        private static Func<ulong, long> _ulong2Long = _Internal_UlongToLong;

        private static long _Internal_LongToLong(long input)
        {
            return (long)input;
        }


        private static Func<long, long> _long2Long = _Internal_LongToLong;

        private static long _Internal_FloatToLong(float input)
        {
            return (long)input;
        }


        private static Func<float, long> _float2Long = _Internal_FloatToLong;

        private static long _Internal_DoubleToLong(double input)
        {
            return (long)input;
        }


        private static Func<double, long> _double2Long = _Internal_DoubleToLong;
        #endregion

        #region Float Conversions
        private static float _Internal_ByteToFloat(byte input)
        {
            return (float)input;
        }


        private static Func<byte, float> _byte2Float = _Internal_ByteToFloat;

        private static float _Internal_SbyteToFloat(sbyte input)
        {
            return (float)input;
        }


        private static Func<sbyte, float> _sbyte2Float = _Internal_SbyteToFloat;

        private static float _Internal_UshortToFloat(ushort input)
        {
            return (float)input;
        }


        private static Func<ushort, float> _ushort2Float = _Internal_UshortToFloat;

        private static float _Internal_ShortToFloat(short input)
        {
            return (float)input;
        }


        private static Func<short, float> _short2Float = _Internal_ShortToFloat;

        private static float _Internal_UintToFloat(uint input)
        {
            return (float)input;
        }


        private static Func<uint, float> _uint2Float = _Internal_UintToFloat;

        private static float _Internal_IntToFloat(int input)
        {
            return (float)input;
        }


        private static Func<int, float> _int2Float = _Internal_IntToFloat;

        private static float _Internal_UlongToFloat(ulong input)
        {
            return (float)input;
        }


        private static Func<ulong, float> _ulong2Float = _Internal_UlongToFloat;

        private static float _Internal_LongToFloat(long input)
        {
            return (float)input;
        }


        private static Func<long, float> _long2Float = _Internal_LongToFloat;

        private static float _Internal_FloatToFloat(float input)
        {
            return (float)input;
        }


        private static Func<float, float> _float2Float = _Internal_FloatToFloat;

        private static float _Internal_DoubleToFloat(double input)
        {
            return (float)input;
        }


        private static Func<double, float> _double2Float = _Internal_DoubleToFloat;
        #endregion

        #region Double Conversions
        private static double _Internal_ByteToDouble(byte input)
        {
            return (double)input;
        }


        private static Func<byte, double> _byte2Double = _Internal_ByteToDouble;

        private static double _Internal_SbyteToDouble(sbyte input)
        {
            return (double)input;
        }


        private static Func<sbyte, double> _sbyte2Double = _Internal_SbyteToDouble;

        private static double _Internal_UshortToDouble(ushort input)
        {
            return (double)input;
        }


        private static Func<ushort, double> _ushort2Double = _Internal_UshortToDouble;

        private static double _Internal_ShortToDouble(short input)
        {
            return (double)input;
        }


        private static Func<short, double> _short2Double = _Internal_ShortToDouble;

        private static double _Internal_UintToDouble(uint input)
        {
            return (double)input;
        }


        private static Func<uint, double> _uint2Double = _Internal_UintToDouble;

        private static double _Internal_IntToDouble(int input)
        {
            return (double)input;
        }


        private static Func<int, double> _int2Double = _Internal_IntToDouble;

        private static double _Internal_UlongToDouble(ulong input)
        {
            return (double)input;
        }


        private static Func<ulong, double> _ulong2Double = _Internal_UlongToDouble;

        private static double _Internal_LongToDouble(long input)
        {
            return (double)input;
        }


        private static Func<long, double> _long2Double = _Internal_LongToDouble;

        private static double _Internal_FloatToDouble(float input)
        {
            return (double)input;
        }


        private static Func<float, double> _float2Double = _Internal_FloatToDouble;

        private static double _Internal_DoubleToDouble(double input)
        {
            return (double)input;
        }


        private static Func<double, double> _double2Double = _Internal_DoubleToDouble;
        #endregion


        private static Dictionary<Type, Dictionary<Type, object>> _lookupTable = new()
        {
            {
                typeof(byte), new()
                {
                    { typeof(byte), _byte2Byte },
                    { typeof(sbyte), _sbyte2Byte },
                    { typeof(ushort), _ushort2Byte },
                    { typeof(short), _short2Byte },
                    { typeof(uint), _uint2Byte },
                    { typeof(int), _int2Byte },
                    { typeof(ulong), _ulong2Byte },
                    { typeof(long), _long2Byte },
                    { typeof(float), _float2Byte },
                    { typeof(double), _double2Byte },
                }
            },
            {
                typeof(sbyte), new()
                {
                    { typeof(byte), _byte2Sbyte },
                    { typeof(sbyte), _sbyte2Sbyte },
                    { typeof(ushort), _ushort2Sbyte },
                    { typeof(short), _short2Sbyte },
                    { typeof(uint), _uint2Sbyte },
                    { typeof(int), _int2Sbyte },
                    { typeof(ulong), _ulong2Sbyte },
                    { typeof(long), _long2Sbyte },
                    { typeof(float), _float2Sbyte },
                    { typeof(double), _double2Sbyte },
                }
            },
            {
                typeof(ushort), new()
                {
                    { typeof(byte), _byte2Ushort },
                    { typeof(sbyte), _sbyte2Ushort },
                    { typeof(ushort), _ushort2Ushort },
                    { typeof(short), _short2Ushort },
                    { typeof(uint), _uint2Ushort },
                    { typeof(int), _int2Ushort },
                    { typeof(ulong), _ulong2Ushort },
                    { typeof(long), _long2Ushort },
                    { typeof(float), _float2Ushort },
                    { typeof(double), _double2Ushort },
                }
            },
            {
                typeof(short), new()
                {
                    { typeof(byte), _byte2Short },
                    { typeof(sbyte), _sbyte2Short },
                    { typeof(ushort), _ushort2Short },
                    { typeof(short), _short2Short },
                    { typeof(uint), _uint2Short },
                    { typeof(int), _int2Short },
                    { typeof(ulong), _ulong2Short },
                    { typeof(long), _long2Short },
                    { typeof(float), _float2Short },
                    { typeof(double), _double2Short },
                }
            },
            {
                typeof(uint), new()
                {
                    { typeof(byte), _byte2Uint },
                    { typeof(sbyte), _sbyte2Uint },
                    { typeof(ushort), _ushort2Uint },
                    { typeof(short), _short2Uint },
                    { typeof(uint), _uint2Uint },
                    { typeof(int), _int2Uint },
                    { typeof(ulong), _ulong2Uint },
                    { typeof(long), _long2Uint },
                    { typeof(float), _float2Uint },
                    { typeof(double), _double2Uint },
                }
            },
            {
                typeof(int), new()
                {
                    { typeof(byte), _byte2Int },
                    { typeof(sbyte), _sbyte2Int },
                    { typeof(ushort), _ushort2Int },
                    { typeof(short), _short2Int },
                    { typeof(uint), _uint2Int },
                    { typeof(int), _int2Int },
                    { typeof(ulong), _ulong2Int },
                    { typeof(long), _long2Int },
                    { typeof(float), _float2Int },
                    { typeof(double), _double2Int },
                }
            },
            {
                typeof(ulong), new()
                {
                    { typeof(byte), _byte2Ulong },
                    { typeof(sbyte), _sbyte2Ulong },
                    { typeof(ushort), _ushort2Ulong },
                    { typeof(short), _short2Ulong },
                    { typeof(uint), _uint2Ulong },
                    { typeof(int), _int2Ulong },
                    { typeof(ulong), _ulong2Ulong },
                    { typeof(long), _long2Ulong },
                    { typeof(float), _float2Ulong },
                    { typeof(double), _double2Ulong },
                }
            },
            {
                typeof(long), new()
                {
                    { typeof(byte), _byte2Long },
                    { typeof(sbyte), _sbyte2Long },
                    { typeof(ushort), _ushort2Long },
                    { typeof(short), _short2Long },
                    { typeof(uint), _uint2Long },
                    { typeof(int), _int2Long },
                    { typeof(ulong), _ulong2Long },
                    { typeof(long), _long2Long },
                    { typeof(float), _float2Long },
                    { typeof(double), _double2Long },
                }
            },
            {
                typeof(float), new()
                {
                    { typeof(byte), _byte2Float },
                    { typeof(sbyte), _sbyte2Float },
                    { typeof(ushort), _ushort2Float },
                    { typeof(short), _short2Float },
                    { typeof(uint), _uint2Float },
                    { typeof(int), _int2Float },
                    { typeof(ulong), _ulong2Float },
                    { typeof(long), _long2Float },
                    { typeof(float), _float2Float },
                    { typeof(double), _double2Float },
                }
            },
            {
                typeof(double), new()
                {
                    { typeof(byte), _byte2Double },
                    { typeof(sbyte), _sbyte2Double },
                    { typeof(ushort), _ushort2Double },
                    { typeof(short), _short2Double },
                    { typeof(uint), _uint2Double },
                    { typeof(int), _int2Double },
                    { typeof(ulong), _ulong2Double },
                    { typeof(long), _long2Double },
                    { typeof(float), _float2Double },
                    { typeof(double), _double2Double },
                }
            }

        };

        public static TOutput CreateChecked<T, TOutput>(T input)
            where T : unmanaged, IComparable<T>, IConvertible
            where TOutput : unmanaged, IComparable<TOutput>, IConvertible
        {
            Type inputType = typeof(T);
            Type outputType = typeof(TOutput);

            if (_lookupTable.TryGetValue(outputType, out var dict)
                && dict.TryGetValue(inputType, out object func))
            {
                var function = (Func<T, TOutput>)func;
                return function(input);
            }

            throw new NotSupportedException();
        }
    }
}
