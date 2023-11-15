///////////////////////////////////////////////////////
/// Filename: SerializeUpdateEvent.cs
/// Date: September 28, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Objects;

namespace EppNet.Processes.Events
{

    public class SerializeUpdateEvent : IUpdateEvent
    {

        /// <summary>
        /// Sequence ID within the constructor
        /// </summary>
        public int ID { private set; get; }
        public ObjectDelegate ObjectDelegate { private set; get; }
        public string MemberName { private set; get; }
        public object[] Arguments { private set; get; }

        public Update Update { private set; get; }

        public void Initialize(int sequenceId, ObjectDelegate objDelegate, string memberName,
            object[] arguments)
        {
            ID = sequenceId;
            ObjectDelegate = objDelegate;
            MemberName = memberName;
            Arguments = arguments;
        }

        public void SetUpdate(Update update)
        {
            this.Update = update;
        }

        public Update GetUpdate() => Update;

    }

}
