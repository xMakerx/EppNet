///////////////////////////////////////////////////////
/// Filename: EppNet.cs
/// Date: July 29, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Node;

using System;

namespace EppNet
{

    public static class EppNet
    {

        /// <summary>
        /// Whether or not the C++ ENet library has been initialized.<br/>
        /// Don't worry about manually initializing ENet unless you're an advanced user.<br/><br/>
        /// See <see cref="NetworkNodeManager._Internal_TryRegisterNode(NetworkNode)"/> and <br/><see cref="NetworkNodeManager._Internal_TryUnregisterNode(NetworkNode)"/>
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
        /// Tries to deinitialize ENet if no nodes are registered.
        /// </summary>
        /// <returns>Whether or not ENet was deinitialized</returns>
        /// <exception cref="InvalidOperationException">A NetworkNode is registered</exception>
        public static bool DeinitializeENet()
        {
            if (!ENet_Initialized)
                return false;

            if (!NetworkNodeManager.IsEmpty())
                throw new InvalidOperationException("Cannot deinitialize the C++ ENet library if a NetworkNode is registered!");

            Library.Deinitialize();
            ENet_Initialized = false;
            return true;
        }

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

    }

}
