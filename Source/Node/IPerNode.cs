///////////////////////////////////////////////////////
/// Filename: IPerNode.cs
/// Date: July 10, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    public interface IPerNode
    {

        /// <summary>
        /// The <see cref="NetworkNode"/> this object is concerned with
        /// </summary>

        [MemberNotNull]
        public NetworkNode Node { get; }

    }

}