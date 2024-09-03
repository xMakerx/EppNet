///////////////////////////////////////////////////////
/// Filename: SnapshotBase.cs
/// Date: September 3, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Snapshots
{
    
    public class Snapshot : SnapshotBase
    {

        internal Snapshot(SnapshotService service, long globalSequenceNumber, SequenceNumber seqNumber)
            : base(service.Node, globalSequenceNumber, seqNumber)
        {

        }

        public override void RecordCurrent()
        {
            throw new System.NotImplementedException();
        }
    }

}
