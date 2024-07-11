///////////////////////////////////////////////////////
/// Filename: Service.cs
/// Date: April 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Services
{

    public enum ServiceState
    {
        Offline         = 0,
        Starting        = 1,
        Online          = 2,
        ShuttingDown    = 3
    }

    public abstract class Service : IService
    {

        public event Action<ServiceStateChangedEvent> OnStateChanged;

        public ServiceState Status
        {
            protected set
            {
                if (value != _status)
                {
                    OnStateChanged?.Invoke(new(value, _status));
                    _status = value;
                }
            }

            get => _status;
        }

        protected readonly ServiceManager _serviceMgr;

        protected ServiceState _status = ServiceState.Offline;

        /// <summary>
        /// Next tick an update needs to be performed.
        /// </summary>
        protected bool _isDirty;

        protected Service(ServiceManager svcMgr)
        {
            this._status = ServiceState.Offline;
            this._serviceMgr = svcMgr;

            if (_serviceMgr == null)
                throw new ArgumentNullException("svcMgr", "Must pass a valid ServiceManager instance to a service!");
        }

        /// <summary>
        /// Called every tick
        /// </summary>

        internal virtual void Update()
        {
            // If 
        }

        public virtual void Start()
        {
            this.Status = ServiceState.Starting;
        }


        public virtual void Shutdown()
        {

            this.Status = ServiceState.ShuttingDown;
        }

        /// <summary>
        /// Marks this service as dirty and needing cleaning next update.
        /// </summary>

        public void MarkDirty()
        {
            this._isDirty = true;
        }

        public ServiceState GetStatus() => _status;

    }

}
