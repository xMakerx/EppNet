///////////////////////////////////////////////////////
/// Filename: Resolver.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EppNet.Data
{

    public enum ReadResult
    {
        /// <summary>
        /// Failed to read from the stream
        /// </summary>
        Failed,

        /// <summary>
        /// Successfully read from the stream<br/>
        /// In cases of vectors, this means an absolute value was returned
        /// </summary>
        Success,

        /// <summary>
        /// Successfully read from the stream<br/>
        /// In cases of vectors, this means a delta value was returned.
        /// </summary>
        SuccessDelta
    }

    public static class ReadResultExtensions
    {

        public static bool IsSuccess(this ReadResult result) 
            => result == ReadResult.Success || result == ReadResult.SuccessDelta;
    }

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

        public Resolver(int size, bool autoAdvance = true)
        {
            this.Output = typeof(T);
            this.Size = size;
            this.AutoAdvance = autoAdvance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadResult Read(BytePayload payload, out T output)
            => _Internal_Read(payload, out output);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadResult Read(BytePayload payload, out object output)
        {
            ReadResult read = _Internal_Read(payload, out T result);
            output = result;

            return read;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadResult Read(BytePayload payload, out T[] output)
        {
            output = default;

            ReadResult read = ByteResolver.Instance.Read(payload, out byte header);

            if (!read.IsSuccess())
                return ReadResult.Failed;

            if (header == IResolver.NullArrayHeader)
            {
                output = null;
                return ReadResult.Success;
            }

            if (header == IResolver.EmptyArrayHeader)
            {
                output = new T[0];
                return ReadResult.Success;
            }

            int typeIndex = byte.TrailingZeroCount(header);
            int length;

            switch (typeIndex)
            {
                case 0:
                    ByteResolver.Instance.Read(payload, out byte bLength);
                    length = bLength;
                    break;

                case 1:
                    UShortResolver.Instance.Read(payload, out ushort uLength);
                    length = uLength;
                    break;

                case 2:
                    UInt32Resolver.Instance.Read(payload, out uint iLength);
                    length = (int) iLength;
                    break;

                default:
                    return ReadResult.Failed;
            }

            output = new T[length];

            for (int i = 0; i < length; i++)
            {
                read = _Internal_Read(payload, out T element);

                if (read == ReadResult.Failed)
                    break;

                output[i] = element;
            }

            return read;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadResult Read<TCollection>(BytePayload payload, out TCollection output) where TCollection : ICollection<T>, new()
        {
            output = default;
            ReadResult read = ByteResolver.Instance.Read(payload, out byte header);

            if (!read.IsSuccess())
                return ReadResult.Failed;

            if (header == IResolver.NullArrayHeader)
                return ReadResult.Success;

            if (header == IResolver.EmptyArrayHeader)
            {
                output = new();
                return ReadResult.Success;
            }

            int typeIndex = byte.TrailingZeroCount(header);
            int length;

            switch (typeIndex)
            {
                case 0:
                    ByteResolver.Instance.Read(payload, out byte bLength);
                    length = bLength;
                    break;

                case 1:
                    UShortResolver.Instance.Read(payload, out ushort uLength);
                    length = uLength;
                    break;

                case 2:
                    UInt32Resolver.Instance.Read(payload, out uint iLength);
                    length = (int)iLength;
                    break;

                default:
                    return ReadResult.Failed;
            }

            output = new();

            for (int i = 0; i < length; i++)
            {
                read = _Internal_Read(payload, out T element);

                if (!read.IsSuccess())
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

            if (input == null)
            {
                ByteResolver.Instance.Write(payload, IResolver.NullArrayHeader);
                return true;
            }
            else if (input.Length == 0)
            {
                ByteResolver.Instance.Write(payload, IResolver.EmptyArrayHeader);
                return true;
            }

            int typeIndex = 2;

            if (byte.MinValue <= input.Length && input.Length <= byte.MaxValue)
                typeIndex = 0;

            else if (ushort.MinValue <= input.Length && input.Length <= ushort.MaxValue)
                typeIndex = 1;

            byte header = 0;
            header = (byte)(header | (1 << typeIndex));
            bool written = ByteResolver.Instance.Write(payload, header);

            // Let's write the length
            switch (typeIndex)
            {
                case 0:
                    ByteResolver.Instance.Write(payload, input.Length);
                    break;

                case 1:
                    UShortResolver.Instance.Write(payload, input.Length);
                    break;

                case 2:
                    UInt32Resolver.Instance.Write(payload, input.Length);
                    break;
            }

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

            if (input == null)
            {
                ByteResolver.Instance.Write(payload, IResolver.NullArrayHeader);
                return true;
            }

            if (input.Count == 0)
            {
                ByteResolver.Instance.Write(payload, IResolver.EmptyArrayHeader);
                return true;
            }

            int typeIndex = 2;

            if (byte.MinValue <= input.Count && input.Count <= byte.MaxValue)
                typeIndex = 0;

            else if (ushort.MinValue <= input.Count && input.Count <= ushort.MaxValue)
                typeIndex = 1;

            byte header = 0;
            header = (byte)(header | (1 << typeIndex));
            bool written = ByteResolver.Instance.Write(payload, header);

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
        protected abstract ReadResult _Internal_Read(BytePayload payload, out T output);
    }

    public interface IResolver
    {

        public const byte NullArrayHeader = 0;
        public const byte EmptyArrayHeader = 16;

        public ReadResult Read(BytePayload payload, out object output);
        public bool Write(BytePayload payload, object input);
    }

}
