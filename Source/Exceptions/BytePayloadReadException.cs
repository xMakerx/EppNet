///////////////////////////////////////////////////////
/// Filename: BytePayloadReadException.cs
/// Date: April 14, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

using System;

namespace EppNet.Exceptions
{
    public class BytePayloadReadException : Exception
    {

        public static BytePayloadReadException NewOutOfRangeException(BytePayload payload, long position, long bytes)
        {
            return new BytePayloadReadException(payload, position, OutOfRange, 
                $"Cannot read {bytes} bytes at position {position} as it is out of range.");
        }

        public const int OutOfRange = 0x0;

        public BytePayload Payload { protected set; get; }

        /// <summary>
        /// Position we tried to read at.
        /// </summary>
        public readonly long Position;

        /// <summary>
        /// The length of the payload in bytes
        /// </summary>
        public readonly long Length;

        /// <summary>
        /// The particular read exception
        /// </summary>
        public readonly int ErrorCode;

        public BytePayloadReadException(BytePayload payload, int errorCode) : base()
        {
            this.Payload = payload;
            this.Position = Payload._stream.Position;
            this.Length = Payload.Length;
            this.ErrorCode = errorCode;
        }

        private BytePayloadReadException(BytePayload payload, long position, int errorCode, string message) : base(message)
        {
            this.Payload = payload;
            this.Position = position;
            this.Length = payload.Length;
            this.ErrorCode = errorCode;
        }

    }

}
