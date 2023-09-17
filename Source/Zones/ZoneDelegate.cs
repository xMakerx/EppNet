///////////////////////////////////////////////////////
/// Filename: ZoneDelegate.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;
using EppNet.Sim;

using System.Collections.Generic;

namespace EppNet.Zones
{

    public class ZoneDelegate : ObjectDelegate
    {

        public readonly IZone Zone;
        public IZone ParentZone { internal set; get; }
        protected internal HashSet<ZoneDelegate> _children;

        public ZoneDelegate(ISimUnit userObject) : base(userObject)
        {
            this.Zone = (IZone)userObject;
            this._children = (Zone.GetCellSize().Length() > 0) ? new HashSet<ZoneDelegate>() : null;
        }

        public bool AddChildZone(ZoneDelegate zone)
        {
            // Ensure our cell size is greater than 0
            if (!(_children != null && zone != null && zone != this))
                return false;

            bool added = _children.Add(zone);
            

            return _children.Add(zone);
        }

    }

}
