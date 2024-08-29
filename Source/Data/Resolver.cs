///////////////////////////////////////////////////////
/// Filename: Resolver.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using Microsoft.Diagnostics.Tracing.Parsers;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EppNet.Data
{

    public abstract class Resolver<T> : IResolver
    {

        public readonly Type Output;

        /// <summary>
        /// The size of the serialized object in bytes
        /// </summary>
        public readonly int Size;

        public readonly bool AutoAdvance;

        public Resolver(bool autoAdvance = true)
        {
            this.Output = typeof(T);
            this.Size = Marshal.SizeOf(Output);
            this.AutoAdvance = autoAdvance;
        }

        public Resolver(int size)
        {
            this.Output = typeof(T);
            this.Size = size;
            this.AutoAdvance = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read(BytePayload payload, out T output)
            => _Internal_Read(payload, out output);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read(BytePayload payload, out object output)
        {
            bool read = _Internal_Read(payload, out T result);
            output = result;

            return read;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read(BytePayload payload, out T[] output)
        {
            output = default;
            bool read = UInt32Resolver.Instance.Read(payload, out uint length);

            if (!read)
                return false;

            output = new T[length];

            for (int i = 0; i < length; i++)
            {
                read = _Internal_Read(payload, out T element);

                if (!read)
                    break;

                output[i] = element;
            }

            return read;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Read<TCollection>(BytePayload payload, out TCollection output) where TCollection : ICollection<T>, new()
        {
            output = default;
            bool read = UInt32Resolver.Instance.Read(payload, out uint length);

            if (!read)
                return false;

            output = new();

            for (int i = 0; i < length; i++)
            {
                read = _Internal_Read(payload, out T element);

                if (!read)
                    break;

                output.Add(element);
            }

            return read;
        }

        /// <summary>
        /// Writes the specified input to the payload.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="input"></param>
        /// <returns></returns>

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(BytePayload payload, T input)
        {
            payload.EnsureReadyToWrite();
            bool written = _Internal_Write(payload, input);

            if (written && AutoAdvance)
                payload.Advance(Size);

            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool Write(BytePayload payload, T[] input)
        {
            payload.EnsureReadyToWrite();

            bool written = UInt32Resolver.Instance.Write(payload, input.Length);

            for (int i = 0; i < input.Length; i++)
            {
                if (!written)
                    break;

                written = _Internal_Write(payload, input[i]);

                if (written && AutoAdvance)
                    payload.Advance(Size);
            }

            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool Write<TCollection>(BytePayload payload, TCollection input) where TCollection : ICollection<T>
        {
            payload.EnsureReadyToWrite();

            bool written = UInt32Resolver.Instance.Write(payload, input.Count);
            IEnumerator<T> inputEnum = input.GetEnumerator();

            while (written && inputEnum.MoveNext())
            {
                T element = inputEnum.Current;
                written = _Internal_Write(payload, element);

                if (written && AutoAdvance)
                    payload.Advance(Size);
            }

            return written;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(BytePayload payload, object input)
        {
            if (input is T typedInput)
                return Write(payload, typedInput);

            if (input is T[] typedArray)
                return Write(payload, typedArray);

            if (input.GetType().IsGenericType)
            {
                Type genType = input.GetType().GetGenericTypeDefinition();

                if (genType == typeof(List<>) || genType == typeof(HashSet<>) || genType == typeof(SortedSet<>) || genType == typeof(LinkedList<>))
                {
                    // We support all of these
                    ICollection<T> myColl = input as ICollection<T>;
                    return Write(payload, myColl);
                }
            }

            throw new NotSupportedException($"{input.GetType().Name} is not supported!");
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
