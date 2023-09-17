///////////////////////////////////////////////////////
/// Filename: ObjectManager.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Sim;

using System.Collections.Generic;

namespace EppNet.Objects
{

    public class ObjectManager
    {

        protected static ObjectManager _instance;
        public static ObjectManager Get() => _instance;

        protected readonly Simulation _sim;
        protected Dictionary<long, ObjectDelegate> _objects;

        public ObjectManager()
        {
            if (_instance != null)
                return;

            ObjectManager._instance = this;
            this._sim = Simulation.Get();
            this._objects = new Dictionary<long, ObjectDelegate>();
        }

        public ObjectDelegate GetObject(long id)
        {
            _objects.TryGetValue(id, out ObjectDelegate result);
            return result;
        }

    }

}
