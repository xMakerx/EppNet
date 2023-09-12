﻿///////////////////////////////////////////////////////
/// Filename: DisconnectReason.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Connections
{

    public struct DisconnectReason
    {
        public static DisconnectReason Unknown = new(0, "Connection to the remote host was lost.");
        public static DisconnectReason TimedOut = new(1, "Connection to the remote host has timed out.");
        public static DisconnectReason Ejected = new(2, "Connection was forcibly closed by the remote host.");
        public static DisconnectReason Quit = new(3, "");

        public static DisconnectReason[] Reasons = { Unknown, TimedOut, Ejected, Quit };

        public readonly byte ID;
        public string Message { internal set; get; }

        public DisconnectReason(byte id, string message)
        {
            this.ID = id;
            this.Message = message;
        }

    }

}
