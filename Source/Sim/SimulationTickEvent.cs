/////////////////////////////////////////////
/// Filename: SimulationTickEvent.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Core;

namespace EppNet.Sim
{
    /// <summary>
    /// Event pushed on every simulator tick.
    /// This is not destroyed, it's simply reused and should be highly mutable.
    /// </summary>
    public class SimulationTickEvent
    {

        /// <summary>
        /// The sequence ID within the disruptor.
        /// </summary>
        public int ID { private set; get; }
        public Timestamp Time { private set; get; }

        /// <summary>
        /// These events are reused so we use this to initialize our
        /// properties.
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="time"></param>

        public void Initialize(int sequenceId, Timestamp timestamp)
        {
            this.ID = sequenceId;
            this.Time = timestamp;
        }

    }

}
