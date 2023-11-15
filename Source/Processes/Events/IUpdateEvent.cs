///////////////////////////////////////////////////////
/// Filename: IUpdateEvent.cs
/// Date: September 28, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;

namespace EppNet.Processes.Events
{

    public interface IUpdateEvent
    {

        public void SetUpdate(Update update);

        public Update GetUpdate();

    }

}
