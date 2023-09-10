///////////////////////////////////////////////////////
/// Filename: NetworkStatus.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.core
{

    public enum NetworkStatus : byte
    {
        /// <summary>
        /// No status available; or no <see cref="Network"/> singleton
        /// </summary>
        None = 0,

        /// <summary>
        /// The <see cref="Network"/> singleton has been constructed
        /// </summary>
        Ready = 1 << 0,

        /// <summary>
        /// The native ENet library has been initialized
        /// </summary>
        Initialized = 1 << 1,

        /// <summary>
        /// Some sort of critical error has occurred.
        /// </summary>
        Error = 1 << 2,

        /// <summary>
        /// We're either connected to a host or operational as a server. 
        /// </summary>
        Online = 1 << 3,

        /// <summary>
        /// We've lost connection to a host or are no longer operational as a server. <see cref="Error"/>
        /// </summary>
        Disconnected = 1 << 4,

    }

}
