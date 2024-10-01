///////////////////////////////////////////////////////
/// Filename: DesyncEvent.cs
/// Date: September 4, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;

namespace EppNet.Snapshots
{

    /// <summary>
    /// Stores information about a desynchronization event<br/>
    /// (when we stop receiving a consistent flow of snapshots to maintain the simulation)
    /// </summary>
    public class DesyncEvent
    {

        /// <summary>
        /// When the desynchronization event begin <br/>
        /// More like when we first realized we haven't received a snapshot
        /// in a while.
        /// </summary>
        public TimeSpan EventBegin { internal set; get; }

        public TimeSpan? EventEnd
        {
            internal set
            {
                if (value != null)
                {
                    Duration = value.Value - EventBegin;
                    _eventEnd = value.Value;
                }
            }

            get => _eventEnd;
        }

        /// <summary>
        /// How long this event lasted. This is 0 until the
        /// <see cref="EventEnd"/> property is set.
        /// </summary>
        public TimeSpan Duration { private set; get; }

        /// <summary>
        /// We need a certain amount of snapshots before we're
        /// considered synchronized again.
        /// </summary>
        public int SnapshotsReceived { internal set; get; }

        protected TimeSpan? _eventEnd;

        public DesyncEvent(TimeSpan eventBegin)
        {
            this.EventBegin = eventBegin;
            this.Duration = TimeSpan.Zero;
            this.SnapshotsReceived = 0;
            this._eventEnd = null;
        }

        /// <summary>
        /// If the event hasn't ended yet, it's still happening.
        /// </summary>
        /// <returns></returns>
        public bool IsActive()
            => !EventEnd.HasValue;

    }

}