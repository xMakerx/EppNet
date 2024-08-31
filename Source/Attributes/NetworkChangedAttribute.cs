///////////////////////////////////////////////////////
/// Filename: NetworkChangedAttribute.cs
/// Date: August 31, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Attributes
{

    /// <summary>
    /// Called when the specified property is changed by the network<br/>
    /// Ensure the property has the <see cref="NetworkPropertyAttribute"/>!<br/><br/>
    /// Permitted signatures:<br/>
    /// <code>MyMethod() // Basic callback method</code><br/>
    /// <code>MyMethod(T oldValue) // A callback method that receives the previous value of the property</code><br/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class NetworkChangedAttribute(string propertyName) : Attribute
    {

        /// <summary>
        /// The property that is monitored for changes<br/>
        /// <b>NOTE:</b> Please use nameof and ensure the property has the <see cref="NetworkPropertyAttribute"/>!
        /// </summary>
        public readonly string PropertyName = propertyName;

    }

}
