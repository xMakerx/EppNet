///////////////////////////////////////////////////////
/// Filename: StringTypes.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Data
{

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 16-bit strings to be used for wire communications.
    /// </summary>

    public struct str16
    {

        public string Value;

        public str16(string value)
        {
            this.Value = value;
        }

        public static implicit operator str16(string value)
        {
            if (value == null)
                return null;

            return new str16(value);
        }

        public static implicit operator string(str16 a) => a.Value;

    }

    /// <summary>
    /// Wrapper around strings that signify you would
    /// like 8-bit strings to be used for wire communications.
    /// </summary>

    public struct str8
    {

        public string Value;

        public str8(string value)
        {
            this.Value = value;
        }

        public static implicit operator str8(string value)
        {
            if (value == null)
                return null;

            return new str8(value);
        }

        public static implicit operator string(str8 a) => a.Value;

    }


}
