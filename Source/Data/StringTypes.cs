///////////////////////////////////////////////////////
/// Filename: StringTypes.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data
{

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 16-bit strings to be used for wire communications.
    /// </summary>

    public struct Str16
    {

        public string Value;

        public Str16(string value)
        {
            this.Value = value;
        }

        public static implicit operator Str16(string value)
        {
            if (value == null)
                return null;

            return new Str16(value);
        }

        public static implicit operator string(Str16 a) => a.Value;

    }

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 8-bit strings to be used for wire communications.
    /// </summary>

    public struct Str8
    {

        public string Value;

        public Str8(string value)
        {
            this.Value = value;
        }

        public static implicit operator Str8(string value)
        {
            if (value == null)
                return null;

            return new Str8(value);
        }

        public static implicit operator string(Str8 a) => a.Value;

    }


}
