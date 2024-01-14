//////////////////////////////////////////////
/// <summary>
/// Filename: IRingBufferEvent.cs
/// Date: January 14, 2024
/// Author: Maverick Liberty
/// </summary>
//////////////////////////////////////////////

using System;

namespace EppNet.Processes.Events
{

    public abstract class RingBufferEvent : IDisposable
    {

        public long SequenceID { protected set; get; }

        public bool Disposed { private set; get; }

        /// <summary>
        /// Calls <see cref="Cleanup"/> and sets the sequence id
        /// </summary>
        /// <param name="sequenceId"></param>

        internal void _Internal_Preinitialize(long sequenceId)
        {
            this.Cleanup();
            this.SequenceID = sequenceId;
            this.Disposed = false;
        }

        /// <summary>
        /// Overwrite this to clean up unmanaged resources
        /// </summary>

        public virtual void Dispose() { }

        public void Cleanup()
        {
            if (Disposed)
                return;

            // Dispose of any heavy resources here.
            this.Dispose();
            Disposed = true;
        }

    }

}
