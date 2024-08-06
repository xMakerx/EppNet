///////////////////////////////////////////////////////
/// Filename: Service.cs
/// Date: April 16, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

using EppNet.Logging;
using EppNet.Node;

namespace EppNet.Services
{

    public enum ServiceState
    {
        Offline         = 0,
        Starting        = 1,
        Online          = 2,
        ShuttingDown    = 3
    }

    public abstract class Service : IService, INodeDescendant, ILoggable, IComparable, IComparable<Service>
    {
        public ILoggable Notify { get => this; }
        public NetworkNode Node { get => _serviceMgr.Node; }

        public event Action<ServiceStateChangedEvent> OnStateChanged;

        public event Action<float> OnUpdate;

        public int SortOrder;

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

        internal readonly ServiceManager _serviceMgr;

        protected ServiceState _status = ServiceState.Offline;

        /// <summary>
        /// Next tick an update needs to be performed.
        /// </summary>
        protected bool _isDirty;

        protected Service(ServiceManager svcMgr, int sortOrder = 0)
        {
            this._status = ServiceState.Offline;
            this._serviceMgr = svcMgr;

            if (_serviceMgr == null)
                throw new ArgumentNullException(nameof(svcMgr), "Must pass a valid ServiceManager instance to a service!");

            // Let's add our debug state change notification
            OnStateChanged += (ServiceStateChangedEvent evt) => Notify.Debug(new TemplatedMessage("Service State changed to {NewState} from {OldState}", evt.State, evt.OldState));
            this.SortOrder = sortOrder;
        }

        /// <summary>
        /// Called every tick
        /// </summary>

        internal virtual void Update(float dt)
        {
            OnUpdate?.Invoke(dt);
        }

        public virtual bool Start()
        {
            if (Status != ServiceState.Offline)
                return false;

            this.Status = ServiceState.Starting;
            return true;
        }

        public virtual bool Stop()
        {
            if (Status != ServiceState.Online)
                return false;

            this.Status = ServiceState.ShuttingDown;
            return true;
        }

        public virtual void Dispose(bool disposing) { }

        /// <summary>
        /// Marks this service as dirty and needing cleaning next update.
        /// </summary>

        public void MarkDirty()
        {
            this._isDirty = true;
        }

        public ServiceState GetStatus() => _status;

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;

            if (obj is Service service)
                return SortOrder.CompareTo(service.SortOrder);

            return 0;
        }

        public int CompareTo(Service other)
        {
            if (other == null)
                return 1;

            return SortOrder.CompareTo(other.SortOrder);
        }
    }

}
