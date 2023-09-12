///////////////////////////////////////////////////////
/// Filename: ISimBase.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

namespace EppNet.Simulation
{

    public interface ISimBase
    {
        /// <summary>
        /// Submits a request to synchronize clocks
        /// </summary>
        
        public void RequestSync();
        public SimClock GetClock();

    }

}
