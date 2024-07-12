/////////////////////////////////////////////
/// Filename: ChannelFlags.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Text;

namespace EppNet.Messaging
{

    [Flags]
    public enum ChannelFlags : byte
    {

        None                = 0,

        /// <summary>
        /// Messages sent to this channel are processed immediately rather
        /// than waiting for the next simulation tick.
        /// </summary>
        ProcessImmediately = 1 << 0,

        /// <summary>
        /// Data sent over this channel is encrypted.
        /// </summary>
        Encrypted = 1 << 1

    }

}
