///////////////////////////////////////////////////////
/// Filename: Service.cs
/// Date: April 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Core
{

    public enum ServiceState
    {
        Offline         = 0,
        Starting        = 1,
        Online          = 2,
        ShuttingDown    = 3
    }

    public abstract class Service
    {

        /// <summary>
        /// Fired when the status type changes
        /// Old state, new state
        /// </summary>
        public event Action<ServiceState, ServiceState> OnStateChanged;

        public ServiceState Status
        {
            protected set
            {
                if (value != _status)
                {
                    OnStateChanged?.Invoke(_status, value);
                    _status = value;
                }
            }

            get => _status;
        }

        protected ServiceState _status = ServiceState.Offline;

        public virtual void Shutdown()
        {

            this.Status = ServiceState.ShuttingDown;
        }

    }

}
