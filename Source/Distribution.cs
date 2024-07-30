///////////////////////////////////////////////////////
/// Filename: Distribution.cs
/// Date: September 22, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet
{

    public enum Distribution
    {
        /// <summary>
        /// Types or network objects with this distribution setting are
        /// shared between the server and the client.
        /// </summary>
        Shared = 0,

        /// <summary>
        /// Labels a type or network object as server-only; i.e. intended
        /// for use on the server side.
        /// </summary>
        Server = 1,

        /// <summary>
        /// Labels a type or network object as client-only; i.e. intended
        /// for use on the client side.
        /// </summary>
        Client = 2
    }

}