﻿///////////////////////////////////////////////////////
/// Filename: Resolver.cs
/// Date: September 17, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

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

    public readonly ref struct HeaderData
    {
        public readonly byte Header;
        public readonly int TypeIndex;
        public readonly bool Signed;
        public readonly bool Absolute;
        public readonly int Data;

        public HeaderData(byte header, int typeIndex, bool signed, bool absolute, int data)
        {
            this.Header = header;
            this.TypeIndex = typeIndex;
            this.Signed = signed;
            this.Absolute = absolute;
            this.Data = data;
        }
    }

    public abstract class Resolver<T> : IResolver<T>
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

            if (header == IResolver.NullArrayHeader || header == IResolver.EmptyArrayHeader)
            {
                output = header == IResolver.NullArrayHeader ? null : default;
                return ReadResult.Success;
            }

            read = IResolver._Internal_ReadHeaderAndGetLength(payload, header, out int length);

            if (!read.IsSuccess())
                return read;

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
        public ReadResult Read<TCollection>(BytePayload payload, out TCollection output) where TCollection : class, ICollection<T>, new()
        {
            output = default;
            ReadResult read = ByteResolver.Instance.Read(payload, out byte header);

            if (!read.IsSuccess())
                return ReadResult.Failed;

            if (header == IResolver.NullArrayHeader || header == IResolver.EmptyArrayHeader)
            {
                output = header == IResolver.NullArrayHeader ? default : new();
                return ReadResult.Success;
            }

            read = IResolver._Internal_ReadHeaderAndGetLength(payload, header, out int length);

            if (!read.IsSuccess())
                return read;

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

            IResolver._Internal_WriteHeaderAndLength(payload, input.Length);
            bool written = true;

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
        public virtual bool Write<TCollection>(BytePayload payload, TCollection input) where TCollection : class, ICollection<T>
        {
            payload.EnsureReadyToWrite();

            if (input == null || input.Count == 0)
            {
                ByteResolver.Instance.Write(payload, input == null ?
                    IResolver.NullArrayHeader :
                    IResolver.EmptyArrayHeader);
                return true;
            }

            IResolver._Internal_WriteHeaderAndLength(payload, input.Count);
            bool written = true;

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

    public static class ResolverExtensions
    {

        public static ReadResult ReadAsInt<T>(this Resolver<T> resolver, BytePayload payload, out int length) where T : IComparable<T>, IConvertible
        {
            ReadResult result = resolver.Read(payload, out T output);
            length = Convert.ToInt32(output);
            return result;
        }

        public static ReadResult ReadAs<T, TOutput>(this Resolver<T> resolver, BytePayload payload, out TOutput output)
            where T : unmanaged, IComparable<T>, IConvertible
            where TOutput : unmanaged, IComparable<TOutput>, IConvertible
        {
            ReadResult result = resolver.Read(payload, out T typedOutput);
            output = NumberExtensions.CreateChecked<T, TOutput>(typedOutput);
            return result;
        }
    }

    public interface IResolver<T> : IResolver
    {
        public ReadResult Read(BytePayload payload, out T output);
        public bool Write(BytePayload payload, T input);
    }

    public interface IResolver
    {

        public const byte NullArrayHeader = 0;
        public const byte EmptyArrayHeader = 16;

        public ReadResult Read(BytePayload payload, out object output);
        public bool Write(BytePayload payload, object input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static internal void _Internal_WriteHeaderAndLength(BytePayload payload, int length)
        {
            // Type indices
            // 0 -> byte
            // 1 -> ushort
            // 2 -> uint
            int typeIndex = 2;

            if (byte.MinValue <= length && length <= byte.MaxValue)
                typeIndex = 0;

            else if (ushort.MinValue <= length && length <= ushort.MaxValue)
                typeIndex = 1;

            byte header = (byte)typeIndex;
            payload.Stream.WriteByte(header);

            _ = typeIndex switch
            {
                0 => ByteResolver.Instance.Write(payload, length),
                1 => UShortResolver.Instance.Write(payload, length),
                _ => UInt32Resolver.Instance.Write(payload, length),
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static internal ReadResult _Internal_ReadHeaderAndGetLength(BytePayload payload, byte header, out int length)
        {
            // Type indices
            // 0 -> byte
            // 1 -> ushort
            // 2 -> uint

            return ((header >> 6) & 0b11) switch
            {
                0 => ByteResolver.Instance.ReadAsInt(payload, out length),
                1 => UShortResolver.Instance.ReadAsInt(payload, out length),
                _ => UInt32Resolver.Instance.ReadAsInt(payload, out length)
            };
        }

    }

}
