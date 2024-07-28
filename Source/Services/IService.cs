///////////////////////////////////////////////////////
/// Filename: IService.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Services
{

    /// <summary>
    /// Interface that every Service extends
    /// </summary>

    public interface IService : IRunnable
    {

        /// <summary>
        /// Fired when the <see cref="ServiceState"/> of this service changes<br/>
        /// - See <see cref="ServiceStateChangedEvent"/>
        /// </summary>
        public event Action<ServiceStateChangedEvent> OnStateChanged;

        /// <summary>
        /// Fired when Update is called
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// Marks the service as dirty and needing cleaned up
        /// </summary>
        public void MarkDirty();

        /// <summary>
        /// Gets the current <see cref="ServiceState"/>
        /// </summary>
        public ServiceState GetStatus();

    }

}
