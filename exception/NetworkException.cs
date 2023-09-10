///////////////////////////////////////////////////////
/// Filename: NetworkException.cs
/// Date: September 5, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.core;

using System;

namespace EppNet.exception
{
    public class NetworkException : Exception
    {

        public NetworkException(string message) : base(message)
        {
            Network.Instance?.PostException(this);
        }

    }

}
