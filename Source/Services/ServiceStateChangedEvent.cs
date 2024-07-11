///////////////////////////////////////////////////////
/// Filename: ServiceStateChangedEvent.cs
/// Date: July 9, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Services
{

    public readonly struct ServiceStateChangedEvent : IEquatable<ServiceStateChangedEvent>
    {

        public readonly ServiceState State;
        public readonly ServiceState OldState;

        /// <summary>
        /// Stores a service's state transformation from one to another.
        /// </summary>

        public ServiceStateChangedEvent(ServiceState newState, ServiceState oldState)
        {
            this.State = newState;
            this.OldState = oldState;
        }

        public static bool operator ==(ServiceStateChangedEvent left, ServiceStateChangedEvent right) => left.Equals(right);
        public static bool operator !=(ServiceStateChangedEvent left, ServiceStateChangedEvent right) => !left.Equals(right);

        public bool Equals(ServiceStateChangedEvent other) => State == other.State && OldState == other.OldState;

        public override bool Equals(object obj) => obj is ServiceStateChangedEvent evt && Equals(evt);

        public override int GetHashCode() => State.GetHashCode();
    }

}
