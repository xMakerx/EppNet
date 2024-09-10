///////////////////////////////////////////////////////
/// Filename: SnapshotServiceBase.cs
/// Date: September 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;
using EppNet.Services;

namespace EppNet.Snapshots
{
    
    /// <summary>
    /// A lot of the logic for the snapshot service can be
    /// shared between the client and the server nodes. This reduces
    /// code copying.
    /// </summary>
    public abstract class SnapshotServiceBase : Service
    {

        /// <summary>
        /// This number is multiplied by the <see cref="SnapshotsPerSecond"/>
        /// to create the size of the buffer
        /// </summary>
        public const float DefaultBufferMultiplier = 1.5f;

        /// <summary>
        /// How many snapshots to take per second
        /// </summary>

        public int SnapshotsPerSecond { get; }

        /// <summary>
        /// While synchronized, this is the maximum amount of
        /// snapshots we can keep in the buffer at one time.
        /// </summary>
        public int SnapshotBufferSize { get; }

        /// <summary>
        /// How often we should be receiving snapshots
        /// </summary>
        public float SnapshotInterval { get; }

        protected SnapshotServiceBase(ServiceManager svcMgr, int snapshotsPerSecond)
            : base(svcMgr, sortOrder: -999)
        {
            this.SnapshotsPerSecond = snapshotsPerSecond;
            this.SnapshotInterval = 1f / SnapshotsPerSecond;
        }

        public void OnCheckForDesync(Connection connection)
        {

        }

    }

}
