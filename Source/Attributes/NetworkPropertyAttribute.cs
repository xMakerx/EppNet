///////////////////////////////////////////////////////
/// Filename: NetworkPropertyAttribute.cs
/// Date: September 23, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NetworkPropertyAttribute : NetworkMemberAttribute { }

}
