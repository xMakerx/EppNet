///////////////////////////////////////////////////////
/// Filename: ChildEvents.cs
/// Date: July 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Time;

namespace EppNet.Objects
{

    public readonly struct ParentChangedEvent
    {
        public readonly ObjectAgent NewParent;
        public readonly ObjectAgent OldParent;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp;

        public ParentChangedEvent(ObjectAgent newParent, ObjectAgent oldParent)
        {
            this.NewParent = newParent;
            this.OldParent = oldParent;
            this.Timestamp = Timestamp.FromMonoNow();
        }
    }

    public readonly struct ChildAddedEvent
    {

        public readonly ObjectAgent Object;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp;

        public ChildAddedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = Timestamp.FromMonoNow();
        }

    }

    public readonly struct ChildRemovedEvent
    {

        public readonly ObjectAgent Object;

        /// <summary>
        /// Timestamp is the same as <see cref="Network.MonotonicTimestamp"/>
        /// </summary>
        public readonly Timestamp Timestamp;

        public ChildRemovedEvent(ObjectAgent @object)
        {
            this.Object = @object;
            this.Timestamp = Timestamp.FromMonoNow();
        }

    }

}
