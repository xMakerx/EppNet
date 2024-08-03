///////////////////////////////////////////////////////
/// Filename: NetworkNodeManager.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Collections;
using EppNet.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    public static class NetworkNodeManager
    {

        internal static OrderedDictionary<Guid, NetworkNode> _nodes = new();

        /// <summary>
        /// Returns if we aren't managing any nodes
        /// </summary>
        /// <returns></returns>
        public static bool IsEmpty() => _nodes.Count == 0;

        /// <summary>
        /// Tries to register the specified <see cref="NetworkNode"/>. <br/>
        /// Node mustn't be null and cannot be registered already.<br/><br/>
        /// <b>NOTE:</b> If the C++ ENet library hasn't been initialized already, it will<br/>
        /// initialize it with default callbacks. See <see cref="InitializeENet()"/>
        /// </summary>
        /// <param name="node">The node to register</param>
        /// <returns>Whether or not the node was registered</returns>
        internal static bool _Internal_TryRegisterNode([NotNull] NetworkNode node, out int index)
        {
            index = _nodes.Count;

            // Absolutely no duplicates
            if (node == null || _nodes.ContainsKey(node.UUID))
                return false;

            bool tryInitEnet = IsEmpty();
            _nodes.Add(node.UUID, node);

            if (tryInitEnet)
            {
                // Try to initialize ENet with no special callbacks
                if (EppNet.InitializeENet())
                {
                    node.Notify.Info(new TemplatedMessage("Initialized C++ ENet library ver-{version}!", ENet.Library.version));

                    // Let's ensure we unregister nodes when the process ends.
                    AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
                    {
                        Iterator<KeyValuePair<Guid, NetworkNode>> iterator = _nodes.Iterator();

                        while (iterator.HasNext())
                        {
                            var pair = iterator.Next();
                            NetworkNode node = pair.Value;
                            node.Dispose(true);
                        }
                    };
                }
            }

            return true;
        }

        /// <summary>
        /// Tries to unregister the specified <see cref="NetworkNode"/>. <br/>
        /// Node mustn't be null and registered already.<br/><br/>
        /// <b>NOTE:</b> If this is the last <see cref="NetworkNode"/>, the C++ ENet library<br/>
        /// will be deinitialized.
        /// </summary>
        /// <param name="node">The node to register</param>
        /// <returns>Whether or not the node was unregistered</returns>

        internal static bool _Internal_TryUnregisterNode([NotNull] NetworkNode node, bool manageENet = true)
        {
            bool removed = node != null && _nodes.Remove(node.UUID);

            if (removed && manageENet)
                EppNet.DeinitializeENet();

            return removed;
        }

    }

}
