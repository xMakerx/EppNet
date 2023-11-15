/////////////////////////////////////////////
/// Filename: UpdateSerializer.cs
/// Date: September 28, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////
using EppNet.Processes.Events;

using Disruptor;

namespace EppNet.Processes
{

    public class PrepareUpdateConsumer : IEventHandler<SerializeUpdateEvent>
    {
        public void OnEvent(SerializeUpdateEvent data, long sequence, bool endOfBatch)
        {
            throw new System.NotImplementedException();
        }
    }

}
