///////////////////////////////////////////////////////
/// Filename: VectorResolverBase.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Utilities;

using System;

namespace EppNet.Data
{

    public abstract class VectorResolverBase<T> : Resolver<T> where T : struct, IEquatable<T>
    {

        public const byte UnitXHeader = 64;
        public const byte UnitYHeader = 32;
        public const byte UnitZHeader = 16;
        public const byte UnitWHeader = 8;

        /// <summary>
        /// The default Vector type output
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

        public bool Write(BytePayload payload, T input, bool absolute = true)
        {
            if (input.Equals(Default)
                || input.Equals(UnitX)
                || input.Equals(UnitY)
                || input.Equals(UnitZ)
                || input.Equals(UnitW))
            {
                if (input.Equals(Default))
                    payload.Stream.WriteByte(0);

                else if (input.Equals(UnitX))
                    payload.Stream.WriteByte(UnitXHeader);

                else if (input.Equals(UnitY))
                    payload.Stream.WriteByte(UnitYHeader);

                else if (input.Equals(UnitZ))
                    payload.Stream.WriteByte(UnitZHeader);

                else if (input.Equals(UnitW))
                    payload.Stream.WriteByte(UnitWHeader);

                return true;
            }

            Span<float> floats = stackalloc float[NumComponents];
            floats = GetFloats(input, ref floats);
            HeaderData data = _Internal_CreateHeaderWithType(ref floats, true, absolute);
            bool written = true;

            byte header = data.Header;

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

            T? fetched = header switch
            {
                0 => Default,
                UnitXHeader => UnitX,
                UnitYHeader => UnitY,
                UnitZHeader => UnitZ,
                UnitWHeader => UnitW,
                _ => null
            };

            if (fetched.HasValue)
                return ReadResult.Success;

            HeaderData data = _Internal_GetHeaderData(header, true);
            int components = data.Data;
            ReadResult readResult = ReadResult.Success;

            output = new();

            for (int i = 0; i < NumComponents; i++)
            {

                // If this isn't an absolute update, we were only sent
                // components with a bit enabled.
                if (!data.Absolute && ((byte)components & (1 << i)) == 0)
                    continue;

                float value;
                readResult = data.TypeIndex switch
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
                readResult = data.Absolute ? ReadResult.Success : ReadResult.SuccessDelta;

            return readResult;
        }

        protected override bool _Internal_Write(BytePayload payload, T input)
            => Write(payload, input, absolute: true);

        public abstract T Put(T input, float value, int index);

        public abstract Span<float> GetFloats(T input, scoped ref Span<float> floats);

    }

}