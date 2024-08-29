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

        public event Action<float> OnTick;

        public int SortOrder;

        public bool Started { private set; get; }

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
            this._isDirty = false;

            if (_serviceMgr == null)
                throw new ArgumentNullException(nameof(svcMgr), "Must pass a valid ServiceManager instance to a service!");

            // Let's add our debug state change notification
            OnStateChanged += (ServiceStateChangedEvent evt) => Notify.Debug(new TemplatedMessage("Service State changed to {NewState} from {OldState}", evt.State, evt.OldState));
            this.SortOrder = sortOrder;
        }

        /// <summary>
        /// Called every tick by the <see cref="ServiceManager"/>
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>

        public virtual bool Tick(float dt)
        {
            if (!Started)
                return false;

            OnTick?.Invoke(dt);
            return true;
        }

        /// <summary>
        /// <b>NOTE: Explicit IRunnable override!! </b>Called every tick. See <see cref="Tick(float)"/>
        /// </summary>

        bool IRunnable.Tick(float dt) => Tick(dt);

        public virtual bool Start()
        {
            if (Status != ServiceState.Offline)
                return false;

            this.Status = ServiceState.Starting;
            this.Started = true;
            return true;
        }

        public virtual bool Stop()
        {
            if (Status != ServiceState.Online)
                return false;

            this.Status = ServiceState.ShuttingDown;
            this.Started = false;
            return true;
        }

        public virtual void Dispose(bool disposing)
        {
            this.Stop();
        }

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
