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

        public void AnnounceGenerate() { }
        public void OnGenerate() { }

        public bool SendUpdate(string memberName, params object[] args)
        {
            ObjectDelegate myDelegate = GetObjectDelegate();
            ObjectRegistration reg = myDelegate.Metadata;
            ObjectMemberDefinition mDef = reg.GetMemberByName(memberName);

            if (mDef == null)
                throw new System.ArgumentNullException($"[ISimUnit#SendUpdate()] for Object of Type {reg.GetRegisteredType().Name} with ID {GetID()} does not have an update called {memberName}!");

            // Let's validate the arguments.
            if (args.Length != mDef.ParameterTypes.Length)
                throw new System.ArgumentNullException($"[ISimUnit#SendUpdate()] Update {memberName} was provided {args.Length} arguments; expected {mDef.ParameterTypes.Length}");

            myDelegate._enqueuedUpdates.Enqueue(Update.For(myDelegate, mDef, args));
            return true;
        }

        /// <summary>
        /// Recalculates what zone this unit is in.
        /// </summary>
        public void RecalculateZoneBounds() { }

        public bool RequestDelete()
        {
            ObjectDelegate objDelegate = GetObjectDelegate();

            if (objDelegate.TicksUntilDeletion == -1)
            {
                objDelegate.TicksUntilDeletion = 0;
                return true;
            }

            return false;
        }

        public void OnDelete() { }

        public bool IsDeleteRequested()
        {
            ObjectDelegate d = GetObjectDelegate();
            return (d != null && d.TicksUntilDeletion > -1);
        }

        public ObjectDelegate GetObjectDelegate() => ObjectManager.Get().GetDelegateFor(this);

        public long GetID()
        {
            ObjectDelegate d = GetObjectDelegate();
            return (d != null ? d.ID : -1);
        }

    }

}
