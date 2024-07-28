///////////////////////////////////////////////////////
/// Filename: NetworkNodeManager.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Collections;
using EppNet.Logging;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Node
{

    public static class NetworkNodeManager
    {

        /// <summary>
        /// Whether or not the C++ ENet library has been initialized.<br/>
        /// Don't worry about manually initializing ENet unless you're an advanced user.<br/><br/>
        /// See <see cref="_Internal_TryRegisterNode(NetworkNode)"/> and <br/><see cref="_Internal_TryUnregisterNode(NetworkNode)"/>
        /// </summary>
        public static bool ENet_Initialized { private set; get; }

        /// <summary>
        /// Tries to initialize the ENet library with no special callbacks<br/>
        /// For more information on <see cref="Callbacks"/>, please visit:<br/>
        /// https://github.com/nxrighthere/ENet-CSharp/tree/master?tab=readme-ov-file#integrate-with-a-custom-memory-allocator
        /// </summary>
        /// <exception cref="InvalidOperationException">C++ library failed to initialize</exception>
        /// <returns>Whether or not ENet was initialized</returns>
        public static bool InitializeENet() => _Internal_TryInitializeENet(null);

        /// <summary>
        /// Tries to initialize the ENet library with the specified callbacks<br/>
        /// For more information on <see cref="Callbacks"/>, please visit:<br/>
        /// https://github.com/nxrighthere/ENet-CSharp/tree/master?tab=readme-ov-file#integrate-with-a-custom-memory-allocator
        /// </summary>
        /// <exception cref="ArgumentNullException">Callbacks is null!</exception>
        /// <exception cref="InvalidOperationException">C++ library failed to initialize</exception>
        /// <returns>Whether or not ENet was initialized</returns>
        public static bool InitializeENet(Callbacks callbacks) => _Internal_TryInitializeENet(callbacks);

        /// <summary>
        /// Tries to initialize the ENet library if it hasn't been initialized already
        /// </summary>
        /// <param name="callbacks">ENet callbacks to use</param>
        /// <returns>Whether it was initialized</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static bool _Internal_TryInitializeENet(Callbacks callbacks)
        {
            // Did we already initialize?
            if (ENet_Initialized)
                return false;

            ENet_Initialized = callbacks == null ? Library.Initialize() : Library.Initialize(callbacks);

            if (!ENet_Initialized)
                throw new InvalidOperationException("Something went wrong while trying to initialize ENet!");

            return ENet_Initialized;
        }

        /// <summary>
        /// Tries to deinitialize ENet if no nodes are registered.
        /// </summary>
        /// <returns>Whether or not ENet was deinitialized</returns>

        private static bool _Internal_TryDeinitializeENet()
        {
            if (!ENet_Initialized)
                return false;

            if (_nodes.Count == 0)
            {
                Library.Deinitialize();
                ENet_Initialized = false;
                return true;
            }

            return false;
        }

        internal static OrderedDictionary<Guid, NetworkNode> _nodes = new();

        /// <summary>
        /// Tries to register the specified <see cref="NetworkNode"/>. <br/>
        /// Node mustn't be null and cannot be registered already.<br/><br/>
        /// <b>NOTE:</b> If the C++ ENet library hasn't been initialized already, it will<br/>
        /// initialize it with default callbacks. See <see cref="InitializeENet()"/>
        /// </summary>
        /// <param name="node">The node to register</param>
        /// <returns>Whether or not the node was registered</returns>
        internal static bool _Internal_TryRegisterNode([NotNull] NetworkNode node)
        {

            // Absolutely no duplicates
            if (node == null || _nodes.ContainsKey(node.UUID))
                return false;

            bool tryInitEnet = _nodes.Count == 0;
            _nodes.Add(node.UUID, node);

            if (tryInitEnet)
            {
                // Try to initialize ENet with no special callbacks
                if (InitializeENet())
                {
                    node.Notify.Debug("Initialized C++ ENet library!");

                    // Let's ensure we unregister nodes when the process ends.
                    AppDomain.CurrentDomain.ProcessExit += (object sender, EventArgs e) =>
                    {
                        Iterator<KeyValuePair<Guid, NetworkNode>> iterator = _nodes.Iterator();

                        while (iterator.HasNext())
                        {
                            var pair = iterator.Next();
                            NetworkNode node = pair.Value;

                            if (_Internal_TryUnregisterNode(node))
                                node.TryStop(true);
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

        internal static bool _Internal_TryUnregisterNode(NetworkNode node)
        {
            bool removed = node != null && _nodes.Remove(node.UUID);

            if (removed)
                _Internal_TryDeinitializeENet();

            return removed;
        }

    }

}
