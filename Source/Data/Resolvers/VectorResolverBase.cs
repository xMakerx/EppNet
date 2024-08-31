///////////////////////////////////////////////////////
/// Filename: VectorResolverBase.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public abstract class VectorResolverBase<T> : Resolver<T> where T : struct, IEquatable<T>
    {

        /// <summary>
        /// Signifies that this is a UnitX transmission.
        /// </summary>
        public const byte UnitXHeader = 64 + 0;

        /// <summary>
        /// Signifies that this is a UnitY transmission.
        /// </summary>
        public const byte UnitYHeader = 64 + 1;

        /// <summary>
        /// Signifies that this is a UnitZ transmission.
        /// </summary>
        public const byte UnitZHeader = 64 + 2;

        /// <summary>
        /// Signifies that this is a UnitW transmission.
        /// </summary>
        public const byte UnitWHeader = 64 + 3;

        /// <summary>
        /// The default Vector type output (i.e. zero for each component)
        /// </summary>
        public T Default { protected set; get; }

        /// <summary>
        /// How many components are in this vector<br/>
        /// Vector2 => 2, <br/>
        /// Vector3 => 3, <br/>
        /// Vector4 => 4, etc.
        /// </summary>
        public int NumComponents { protected set; get; }

        public T UnitX { protected set; get; }
        public T UnitY { protected set; get; }
        public T UnitZ { protected set; get; }
        public T UnitW { protected set; get; }

        protected VectorResolverBase(bool autoAdvance = true) : base(autoAdvance) { }

        protected VectorResolverBase(int size, bool autoAdvance = true) : base(size, autoAdvance) { }

        protected VectorResolverBase(int size) : base(size) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual bool WriteArray(BytePayload payload, T[] input, bool absolute = true)
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

            _Internal_WriteHeaderAndLength(payload, input.Length);
            bool written = true;

            for (int i = 0; i < input.Length; i++)
            {
                if (!written)
                    break;

                written = Write(payload, input[i], absolute);

                if (written && AutoAdvance)
                    payload.Advance(Size);
            }

            return written;
        }

        public bool Write(BytePayload payload, T input, bool absolute = true)
        {
            byte header = 0;
            if (input.Equals(Default)
                || input.Equals(UnitX)
                || input.Equals(UnitY)
                || input.Equals(UnitZ)
                || input.Equals(UnitW))
            {
                if (input.Equals(Default))
                    header = 0;

                else if (input.Equals(UnitX))
                    header = UnitXHeader;

                else if (input.Equals(UnitY))
                    header = UnitYHeader;

                else if (input.Equals(UnitZ))
                    header = UnitZHeader;

                else if (input.Equals(UnitW))
                    header = UnitWHeader;

                header |= (byte)(absolute ? 128 : 0);
                payload.Stream.WriteByte(header);
                return true;
            }

            Span<float> floats = stackalloc float[NumComponents];
            floats = GetFloats(input, ref floats);
            HeaderData data = _Internal_CreateHeaderWithType(ref floats, true, absolute);
            bool written = true;

            header = data.Header;

            // Let's finalize the header
            if (!absolute)
            {
                byte components = 0;

                for (int i = 0; i < floats.Length; i++)
                {
                    if (floats[i] != 0)
                        components |= (byte) (components | (1 << i));
                }

                byte shifted = (byte) ((components & 0b111111) << 2);
                header |= shifted;
            }

            ByteResolver.Instance.Write(payload, header);

            for (int i = 0; i < floats.Length; i++)
            {
                float value = floats[i];

                if (!absolute && value == 0)
                    continue;

                written = data.TypeIndex switch
                {
                    0 => SByteResolver.Instance.Write(payload, (sbyte)value),
                    1 => ShortResolver.Instance.Write(payload, (short)value),
                    2 => Int32Resolver.Instance.Write(payload, (int)value),
                    _ => FloatResolver.Instance.Write(payload, value)
                };

                if (!written)
                    break;
            }

            return written;
        }

        protected override ReadResult _Internal_Read(BytePayload payload, out T output)
        {
            int result = payload.Stream.ReadByte();
            output = Default;

            if (result == -1)
                return ReadResult.Failed;

            byte header = (byte)result;

            bool absolute = (header & 0b1) == 1;
            int typeIndex = header & 0b11;

            int components = (header >> 2) & 0b1111;

            // Check if we received a special value. Negate the first bit
            T? fetched = (header & 0b01111111) switch
            {
                0 => Default,
                UnitXHeader => UnitX,
                UnitYHeader => UnitY,
                UnitZHeader => UnitZ,
                UnitWHeader => UnitW,
                _ => null
            };

            if (fetched.HasValue)
                return absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            ReadResult readResult = ReadResult.Success;

            output = new();

            for (int i = 0; i < NumComponents; i++)
            {

                // If this isn't an absolute update, we were only sent
                // components with a bit enabled.
                if (!absolute && ((byte)components & (1 << i)) == 0)
                    continue;

                float value;
                readResult = typeIndex switch
                {
                    0 => SByteResolver.Instance.ReadAs(payload, out value),
                    1 => ShortResolver.Instance.ReadAs(payload, out value),
                    2 => Int32Resolver.Instance.ReadAs(payload, out value),
                    _ => FloatResolver.Instance.ReadAs(payload, out value)
                };

                if (!readResult.IsSuccess())
                    return readResult;

                output = Put(output, value, i);
            }

            if (readResult.IsSuccess())
                readResult = absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            return readResult;
        }

        protected override bool _Internal_Write(BytePayload payload, T input)
            => Write(payload, input, absolute: true);

        public abstract T Put(T input, float value, int index);

        public abstract Span<float> GetFloats(T input, scoped ref Span<float> floats);

    }

}