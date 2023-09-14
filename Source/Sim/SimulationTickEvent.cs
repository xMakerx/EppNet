/////////////////////////////////////////////
/// Filename: SimulationTickEvent.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
//////////////////////////////////////////////

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
        public ulong Time { private set; get; }

        /// <summary>
        /// These events are reused so we use this to initialize our
        /// properties.
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="time"></param>

        public void Initialize(int sequenceId, ulong time)
        {
            this.ID = sequenceId;
            this.Time = time;
        }

    }

}
