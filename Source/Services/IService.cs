///////////////////////////////////////////////////////
/// Filename: IService.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Services
{

    public interface IService
    {

        public event Action<ServiceStateChangedEvent> OnStateChanged;


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
