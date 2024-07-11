//////////////////////////////////////////////
/// Filename: MessageTemplate.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

namespace EppNet.Logging
{

    /// <summary>
    /// Intended to be passed to an <see cref="ILoggable"/> for templated message output
    /// </summary>
    public readonly struct TemplatedMessage
    {

        public readonly string Message;
        public readonly object[] Objects;

        public TemplatedMessage(string message, params object[] objects)
        {
            this.Message = message;
            this.Objects = objects;
        }

    }

}
