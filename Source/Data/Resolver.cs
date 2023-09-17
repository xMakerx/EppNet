///////////////////////////////////////////////////////
/// Filename: Resolver.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.InteropServices;

namespace EppNet.Data
{

    public abstract class Resolver<T> : IResolver
    {

        public readonly Type Output;
        public readonly int Size;

        public Resolver()
        {
            this.Output = typeof(T);
            this.Size = Marshal.SizeOf(Output);
        }

        public bool Read(BytePayload payload, out T output)
        {
            bool read = _Internal_Read(payload, out output);
            return read;
        }

        public bool Read(BytePayload payload, out object output)
        {
            bool read = _Internal_Read(payload, out T result);
            output = result;

            return read;
        }

        /// <summary>
        /// Writes the specified input to the payload.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public bool Write(BytePayload payload, T input)
        {
            payload.EnsureReadyToWrite();
            bool written = _Internal_Write(payload, input);
            payload.Advance(Size);
            return written;
        }

        public bool Write(BytePayload payload, object input)
        {
            payload.EnsureReadyToWrite();
            bool written = _Internal_Write(payload, (T)input);
            payload.Advance(Size);
            return written;
        }

        protected abstract bool _Internal_Write(BytePayload payload, T input);
        protected abstract bool _Internal_Read(BytePayload payload, out T output);

    }

    public interface IResolver
    {

        public bool Read(BytePayload payload, out object output);
        public bool Write(BytePayload payload, object input);

    }

}
