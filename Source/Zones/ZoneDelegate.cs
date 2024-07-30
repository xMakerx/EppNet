///////////////////////////////////////////////////////
/// Filename: ZoneDelegate.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;

namespace EppNet.Zones
{

    public class ZoneDelegate : ObjectAgent
    {

        public readonly IZone Zone;
        public ZoneDelegate ParentZone { internal set; get; }

        public ZoneDelegate(ObjectService service, ObjectRegistration reg, IZone userObject, long id) : base(service, reg, userObject, id)
        {
            this.Zone = userObject;
            this.ParentZone = null;
            this._children = null;
        }

    }

}
