///////////////////////////////////////////////////////
/// Filename: ISimUnit.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// Simulation units are components that propagate updates
/// on the network

using EppNet.Node;
using EppNet.Objects;

namespace EppNet.Sim
{

    public interface ISimUnit
    {

        public void AnnounceGenerate() { }
        public void OnGenerate() { }

        public bool SendUpdate(string memberName, params object[] args)
        {
            ObjectAgent myDelegate = GetAgent();
            ObjectRegistration reg = myDelegate.Metadata;
            ObjectMemberDefinition mDef = reg.GetMemberByName(memberName);

            if (mDef == null)
                throw new System.ArgumentNullException($"[ISimUnit#SendUpdate()] for Object of Type {reg.GetRegisteredType().Name} with ID {GetID()} does not have an update called {memberName}!");

            // Let's validate the arguments.
            if (args.Length != mDef.ParameterTypes.Length)
                throw new System.ArgumentNullException($"[ISimUnit#SendUpdate()] Update {memberName} was provided {args.Length} arguments; expected {mDef.ParameterTypes.Length}");

            myDelegate.EnqueueOutgoing(Update.For(myDelegate, mDef, args));
            return true;
        }

        /// <summary>
        /// Recalculates what zone this unit is in.
        /// </summary>
        public void RecalculateZoneBounds() { }

        /// <summary>
        /// Called by the <see cref="ObjectManagerService"/> upon successful creation
        /// </summary>
        public void OnCreate(ObjectAgent agent) { }

        /// <summary>
        /// Requests the <see cref="ObjectManagerService"/> to delete this
        /// TODO: ISimUnit: Implement command functionality to call this code at the end of a tick
        /// </summary>
        /// <returns>Whether or not the request for deletion was successful</returns>
        public bool RequestDelete()
        {
            ObjectAgent agent = GetAgent();

            if (agent == null)
                return false;

            return agent.ObjectManager.TryRequestDelete(agent.ID);
        }

        /// <summary>
        /// Called by the <see cref="ObjectManagerService"/> upon deletion.<br/>
        /// Guaranteed to be called once.
        /// </summary>

        public void OnDelete() { }

        /// <summary>
        /// Called by the <see cref="ObjectManagerService"/> when deletion is requested successfully.<br/>
        /// Guaranteed to be called once.
        /// </summary>

        public void OnDeleteRequested() { }

        /// <summary>
        /// Checks if this unit is delete requested.
        /// </summary>
        /// <returns>True or false</returns>

        public bool IsDeleteRequested() => GetAgent()?.TicksUntilDeletion > -1;

        /// <summary>
        /// Fetches the <see cref="ObjectAgent"/> associated with this unit.
        /// </summary>
        /// <returns>A valid ObjectAgent instance or NULL</returns>

        public virtual ObjectAgent GetAgent()
        {
            foreach (NetworkNode node in NetworkNodeManager._nodes.Values)
            {
                ObjectManagerService manager = node.Services.GetService<ObjectManagerService>();

                if (manager != null)
                {
                    ObjectAgent agent = manager.GetAgentFor(this);

                    if (agent != null)
                        return agent;
                }
            }

            return null;
        }

        /// <summary>
        /// Fetches the ID associated with this unit.
        /// </summary>
        /// <returns>A valid ID if properly managed or -1</returns>

        public virtual long GetID()
        {
            ObjectAgent agent = GetAgent();

            if (agent != null)
                return agent.ID;

            return -1;
        }

    }

}
