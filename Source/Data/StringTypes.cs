///////////////////////////////////////////////////////
/// Filename: StringTypes.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;


namespace EppNet.Data
{

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 16-bit strings to be used for wire communications.
    /// </summary>

    public readonly struct Str16
    {

        public readonly string Value;

        public Str16(string value)
        {
            if (value?.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"String16 must be between 0 and {ushort.MaxValue}");

            this.Value = value;
        }

        public static implicit operator Str16(string value)
            => new(value);

        public static implicit operator string(Str16 a)
            => a.Value;

    }

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 8-bit strings to be used for wire communications.
    /// </summary>

    public readonly struct Str8
    {

        public readonly string Value;

        public Str8(string value)
        {
            if (value.Length > byte.MaxValue)
                throw new ArgumentOutOfRangeException($"String8 must be between 0 and {byte.MaxValue}");

            this.Value = value;
        }

        public static implicit operator Str8(string value)
            => new(value);

        public static implicit operator string(Str8 a)
            => a.Value;

    }


}
