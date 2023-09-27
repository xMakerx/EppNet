///////////////////////////////////////////////////////
/// Filename: ObjectUpdateDatagram.cs
/// Date: September 25, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;

using System.Collections.Generic;

namespace EppNet.Data.Datagrams
{

    public class ObjectUpdateDatagram : Datagram
    {

        public ObjectDelegate[] Objects { protected set; get; }

        /// <summary>
        /// Stores a key-value pair of <see cref="ObjectDelegate"/>s with incoming or outgoing updates.
        /// </summary>
        public Dictionary<ObjectDelegate, List<Update>> Transients { protected set; get; }

        public bool SnapshotUpdates { protected set; get; }

        public ObjectUpdateDatagram()
        {
            this.Header = 0x3;
            this.ChannelID = 0x1;
            this.Objects = null;
            this.Transients = new Dictionary<ObjectDelegate, List<Update>>();
            this.SnapshotUpdates = false;
        }

        public ObjectUpdateDatagram(bool snapshotUpdates, params ObjectDelegate[] objectsToUpdate) : this()
        {
            this.SnapshotUpdates = snapshotUpdates;
            this.Objects = objectsToUpdate;

            if (SnapshotUpdates)
                this.ChannelID = 0x2;
        }

        public override void Write()
        {
            base.Write();

            // Let's only send the objects that have pending updates.

            foreach (ObjectDelegate obj in Objects)
            {
                List<Update> outgoing = SnapshotUpdates ? 
                    obj.OutgoingSnapshotUpdates.FlushQueue()
                    : obj.OutgoingReliableUpdates.FlushQueue();

                if (outgoing.Count > 0)
                    Transients.Add(obj, outgoing);
            }

            // Let's record how many objects are in this datagram.
            WriteByte((byte)Transients.Count);

            foreach (KeyValuePair<ObjectDelegate, List<Update>> kvp in Transients)
            {
                // Write the ID of the object.
                WriteULong((ulong) kvp.Key.ID);

                // Let's record the number of updates incoming.
                WriteByte((byte)kvp.Value.Count);

                foreach (Update update in kvp.Value)
                    update.WriteTo(this);
            }

        }

        public override void Read()
        {
            base.Read();

            byte numObjects = ReadByte();

            for (int i = 0; i < numObjects; i++)
            {
                ulong id = (ulong) ReadULong();
                ObjectDelegate objDelegate = ObjectManager.Get().GetObject((long)id);

                if (objDelegate == null)
                {
                    // FIXME: Have to handle when we don't have the object created yet.
                    continue;
                }

                byte numUpdates = ReadByte();
                List<Update> updates = new List<Update>();

                for (int j = 0; j < numUpdates; j++)
                    updates.Add(Update.From(objDelegate, this));

                Transients.Add(objDelegate, updates);
            }
        }

        public override void Dispose()
        {
            this.Objects = null;
            this.Transients.Clear();
            base.Dispose();
        }

    }

}
