///////////////////////////////////////////////////////
/// Filename: SnapshotService.cs
/// Date: September 2, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Services;

namespace EppNet.Snapshots
{

    public sealed class SnapshotService : Service
    {
        public SnapshotService(ServiceManager svcMgr, int sortOrder = 0) : base(svcMgr, sortOrder)
        {
        }
    }

}