///////////////////////////////////////////////////////
/// Filename: ExceptionStrategy.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Exceptions
{

    public enum ExceptionStrategy
    {

        /// <summary>
        /// All exceptions will be thrown --potentially leading to crashes or
        /// an unstable state. <see cref="EppNet.Node.NetworkNode"/> will try its
        /// best to clean up and politely disconnect users.
        /// </summary>
        ThrowAll     = 0,

        /// <summary>
        /// Will log exceptions as they occur
        /// </summary>
        LogOnly      = 1


    }

}