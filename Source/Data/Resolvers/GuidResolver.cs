///////////////////////////////////////////////////////
/// Filename: GuidResolver.cs
/// Date: August 28, 2024
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Attributes;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    [NetworkTypeResolver]
    public class GuidResolver : Resolver<Guid>
    {

        public static GuidResolver Instance = new();

        public GuidResolver() : base(16) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ReadResult _Internal_Read(BytePayload payload, out Guid output)
        {
            Span<byte> buffer = stackalloc byte[Size];
            int read = payload.Stream.Read(buffer);

            output = (read == buffer.Length) ? new(buffer) : default;
            return read == buffer.Length ? ReadResult.Success : ReadResult.Failed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool _Internal_Write(BytePayload payload, Guid input)
            => input.TryWriteBytes(payload.Stream.GetSpan(Size));

    }

    public static class GuidResolverExtensions
    {

        /// <summary>
        /// Writes 16 unsigned 8-bit integers to the stream denoting the Guid
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Guid input)
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified input to the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>
        /// <param name="input"></param>
        public static void Write(this BytePayload payload, Guid[] input)
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>
        /// <param name="input"></param>
        public static void Write<TCollection>(this BytePayload payload, TCollection input) where TCollection : class, ICollection<Guid>
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>
        /// <param name="input"></param>
        public static void WriteArray(this BytePayload payload, Guid[] input)
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes 16 unsigned 8-bit integers to the stream denoting the Guid
        /// </summary>
        /// <param name="input"></param>
        public static void WriteGuid(this BytePayload payload, Guid input)
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Writes the specified collection input to the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>
        /// <param name="input"></param>
        public static void WriteGuidArray(this BytePayload payload, Guid[] input)
            => GuidResolver.Instance.Write(payload, input);

        /// <summary>
        /// Reads a Guid collection from the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>

        public static TCollection Read<TCollection>(this BytePayload payload) where TCollection : class, ICollection<Guid>, new()
        {
            GuidResolver.Instance.Read(payload, out TCollection output);
            return output;
        }

        /// <summary>
        /// Reads 16 unsigned 8-bit integers from the stream denoting a Guid
        /// </summary>
        public static Guid ReadGuid(this BytePayload payload)
        {
            GuidResolver.Instance.Read(payload, out Guid output);
            return output;
        }

        /// <summary>
        /// Reads a Guid array from the stream<br/>
        /// See <see cref="Write(BytePayload, Guid)"/> for more info on how each Guid is written
        /// </summary>
        public static Guid[] ReadGuidArray(this BytePayload payload)
        {
            GuidResolver.Instance.Read(payload, out Guid[] output);
            return output;
        }

    }

}
