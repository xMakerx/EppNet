/////////////////////////////////////////////
/// Filename: ChannelFlags.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Data
{
    public enum ChannelFlags : byte
    {
        None = 0,

        /// <summary>
        /// Datagrams received are sent to this channel to be processed
        /// immediately after reception rather than waiting for a new simulation
        /// tick.
        /// </summary>
        ProcessImmediately = 1 << 0,
    }

}
