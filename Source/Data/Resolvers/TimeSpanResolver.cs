///////////////////////////////////////////////////////
/// Filename: TimeSpanResolver.cs
/// Date: September 30, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Buffers.Binary;

namespace EppNet.Data
{

    public class TimeSpanResolver : Resolver<TimeSpan>
    {
        public static readonly TimeSpanResolver Instance = new();

        public TimeSpanResolver() : base(sizeof(long)) { }

        protected override ReadResult _Internal_Read(BytePayload payload, out TimeSpan output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);
            long ticks = 0;

            ReadResult result = BinaryPrimitives.TryReadInt64LittleEndian(buffer, out ticks)
                ? ReadResult.Success : ReadResult.Failed;

            output = TimeSpan.FromTicks(ticks);
            return result;
        }

        protected override bool _Internal_Write(BytePayload payload, TimeSpan input)
            => BinaryPrimitives.TryWriteInt64LittleEndian(payload.Stream.GetSpan(Size), input.Ticks);
    }

    public static class TimeSpanResolverExtensions
    {

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> to the wire.<br/>
        /// Internally, this writes the ticks as a long
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>

        public static void Write(this BytePayload payload, TimeSpan input)
            => TimeSpanResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="TimeSpan"/> to the wire.<br/>
        /// See <see cref="Write(BytePayload, TimeSpan)"/> for more information.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, TimeSpan[] input)
            => TimeSpanResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes an array of <see cref="TimeSpan"/> to the wire.<br/>
        /// See <see cref="Write(BytePayload, TimeSpan)"/> for more information.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, TimeSpan[] input)
            => TimeSpanResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a <see cref="TimeSpan"/> from the wire.<br/>
        /// </summary>
        /// <param name="payload"></param>

        public static TimeSpan Read(this BytePayload payload)
        {
            TimeSpanResolver.Instance.Read(payload, out TimeSpan result);
            return result;
        }

        /// <summary>
        /// Reads a <see cref="TimeSpan"/> array from the wire.<br/>
        /// </summary>
        /// <param name="payload"></param>

        public static TimeSpan[] ReadArray(this BytePayload payload)
        {
            TimeSpanResolver.Instance.Read(payload, out TimeSpan[] result);
            return result;
        }
    }

}
