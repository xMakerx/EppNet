///////////////////////////////////////////////////////
/// Filename: ISimUnit.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// Simulation units are components that propagate updates
/// on the network

using EppNet.Objects;

namespace EppNet.Sim
{

    public interface ISimUnit
    {

        public void AnnounceGenerate();
        public void Generate();

        public bool SendUpdate(string methodName, params object[] args);

        /// <summary>
        /// Recalculates what zone this unit is in.
        /// </summary>
        public void RecalculateZoneBounds() { }

        public bool RequestDelete();

        public bool IsDeleteRequested()
        {
            ObjectDelegate d = GetObjectDelegate();
            return (d != null && d.TicksUntilDeletion > -1);
        }

        internal void SetObjectDelegate(ObjectDelegate oDelegate);
        public ObjectDelegate GetObjectDelegate();

        internal void SetID(long id);
        public long GetID();

    }

}
