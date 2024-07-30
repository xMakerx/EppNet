///////////////////////////////////////////////////////
/// Filename: INodeDescendant.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    /// <summary>
    /// Indicates something that descends from and is managed by a <see cref="NetworkNode"/>
    /// </summary>

    public interface INodeDescendant
    {

        /// <summary>
        /// The <see cref="NetworkNode"/> this object is concerned with
        /// </summary>

        [MemberNotNull]
        public NetworkNode Node { get; }

    }

}