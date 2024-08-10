///////////////////////////////////////////////////////
/// Filename: DisconnectReason.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Connections
{

    public static class DisconnectReasons
    {
        internal static List<DisconnectReason> _reasons = [];

        public static readonly DisconnectReason Unknown = new("Connection to the remote host was lost.");
        public static readonly DisconnectReason TimedOut = new("Connection to the remote host has timed out.");
        public static readonly DisconnectReason Ejected = new("Connection was forcibly closed by the remote host.");
        public static readonly DisconnectReason Quit = new(string.Empty);

        public static DisconnectReason GetFromID(byte id)
        {
            int index = (int)id;

            if (-1 < index && index < _reasons.Count)
                return _reasons[index];

            return Unknown;
        }

        public static DisconnectReason GetFromID(uint id) => GetFromID((byte)id);

        public static DisconnectReason From(DisconnectReason baseReason, string message) => new(baseReason.ID, message);
    }

    public readonly struct DisconnectReason : IEquatable<DisconnectReason>
    {

        #region Operators

        public static bool operator ==(DisconnectReason left, DisconnectReason right) => left.Equals(right);
        public static bool operator !=(DisconnectReason left, DisconnectReason right) => !left.Equals(right);

        #endregion

        public readonly byte ID;
        public readonly string Message;

        public DisconnectReason(string message)
        {
            this.ID = (byte) DisconnectReasons._reasons.Count;
            this.Message = message;

            DisconnectReasons._reasons.Add(new DisconnectReason(message));
        }

        internal DisconnectReason(byte id, string message)
        {
            this.ID = id;
            this.Message = message;
        }

        public bool Equals(DisconnectReason other) => ID == other.ID && Message.Equals(other.Message, StringComparison.Ordinal);

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is DisconnectReason otherReason)
                return ID == otherReason.ID && Message.Equals(otherReason.Message, StringComparison.Ordinal);

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = ID;
            hashCode ^= Message.GetHashCode();
            return hashCode;
        }

    }

}
