///////////////////////////////////////////////////////
/// Filename: NetworkNodeManager.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System.Collections.Generic;
using System;

namespace EppNet.Node
{

    internal static class NetworkNodeManager
    {

        internal static Dictionary<Guid, NetworkNode> _nodes = new();


        /// <summary>
        /// Tries to register a <see cref="NetworkNode"/>. <br/>
        /// Node mustn't be added already and mustn't be null.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal static bool TryRegisterNode(NetworkNode node)
        {
            return node != null && _nodes.TryAddEntry(node.UUID, node);
        }

        internal static bool TryUnregisterNode(NetworkNode node)
        {
            return node != null && _nodes.Remove(node.UUID);
        }

    }

}