///////////////////////////////////////////////////////
/// Filename: ZoneDelegate.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;

using System.Collections.Generic;

namespace EppNet.Zones
{

    public class ZoneDelegate : ObjectAgent
    {

        public readonly IZone Zone;
        public ZoneDelegate ParentZone { internal set; get; }
        protected internal HashSet<ZoneDelegate> _children;

        public ZoneDelegate(ObjectService service, ObjectRegistration reg, IZone userObject, long id) : base(service, reg, userObject, id)
        {
            this.Zone = userObject;
            this.ParentZone = null;
            this._children = null;
        }

        public bool AddChildZone(ZoneDelegate zone)
        {
            // Ensure we were passed a valid delegate
            if (!(zone != null && zone != this && zone.ParentZone != this))
                return false;

            if (_children == null)
                _children = new HashSet<ZoneDelegate>();

            // Update the current parent of the child to
            // know about the change in the tree.
            zone.ParentZone?.RemoveChildZone(zone);

            bool added = _children.Add(zone);

            // Update the parent of the zone to us!
            zone.ParentZone = this;

            return added;
        }

        public bool RemoveChildZone(ZoneDelegate zone)
        {
            // Ensure we were passed a valid delegate,
            // and we have a set of delegates.
            if (!IsOtherValid(zone) || _children == null)
                return false;

            bool removed = _children.Remove(zone);
            if (removed)
                zone.ParentZone = null;

            return removed;
        }

    }

}
