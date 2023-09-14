///////////////////////////////////////////////////////
/// Filename: ISimViewer.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////
/// Simulation viewers are observers of simulation units.
/// They receive updates about units they're interested in.

using System.Collections.Generic;

namespace EppNet.Sim
{
    public interface ISimViewer
    {

        //public bool HasInterestIn(ISimUnit unit) => HasInterestIn(unit.ID);

        //public bool HasInterestIn(ISimUnit unit, int recursionDepth) => HasInterestIn(unit.ID, recursionDepth);

        public bool HasInterestIn(uint id, int recursionDepth = 0)
        {
            var dict = GetInterestDictionary();
            bool contains = dict.ContainsKey(id);

            if (!contains && recursionDepth > 0)
            {
                // Descend the "tree" by one level.
                recursionDepth--;

                foreach (ISimUnit unit in dict.Values)
                {
                    if (unit is ISimViewer viewer)
                        if (viewer.HasInterestIn(id, recursionDepth))
                            return true;
                }
            }

            return contains;
        
        }

        public bool RemoveInterest(uint id)
        {
            var dict = GetInterestDictionary();
            return dict.Remove(id);
        }

        //public bool RemoveInterest(ISimUnit unit) => RemoveInterest(unit.ID);

        /// <summary>
        /// Retrieves dictionary containing the units we're interested in.
        /// </summary>
        /// <returns></returns>
        protected IDictionary<uint, ISimUnit> GetInterestDictionary();

    }

}
