///////////////////////////////////////////////////////
/// Filename: Vector4Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class Vector4Resolver : Resolver<Vector4>
    {

        public static readonly Vector4Resolver Instance = new();

        public const byte AbsoluteHeader = 128;
        public const byte UnitXHeader = 16;
        public const byte UnitYHeader = 32;
        public const byte UnitZHeader = 64;
        public const byte UnitWHeader = 128;

        public Vector4Resolver() : base(autoAdvance: false) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(BytePayload payload, Vector4 input, bool absolute = true)
        {
            if (input == Vector4.Zero || input == Vector4.UnitX || input == Vector4.UnitY || input == Vector4.UnitZ || input == Vector4.UnitW)
            {
                if (input == Vector4.Zero)
                    payload.Stream.WriteByte(0);
                else
                    payload.Stream.WriteByte(input == Vector4.UnitX ? UnitXHeader : input == Vector4.UnitY ? UnitYHeader : input == Vector4.UnitZ ? UnitZHeader : UnitWHeader);

                return true;
            }

            Vector4 typeIndices = Vector4.Zero;

            for (int i = 0; i < 4; i++)
            {
                float value = input[i];
                int typeIndex = 3;

                if ((value % 1) != 0)
                {
                    typeIndices[i] = typeIndex;
                    continue;
                }

                if (sbyte.MinValue <= value && value <= sbyte.MaxValue)
                    typeIndex = 0;

                else if (short.MinValue <= value && value <= short.MaxValue)
                    typeIndex = 1;

                else if (int.MinValue <= value && value <= int.MaxValue)
                    typeIndex = 2;

                typeIndices[i] = typeIndex;
            }

            int useTypeIndex = (int)MathF.Max(MathF.Max(typeIndices[0], typeIndices[1]), MathF.Max(typeIndices[2], typeIndices[3]));

            byte header = absolute ? AbsoluteHeader : (byte)0;
            header = (byte)(header | (1 << useTypeIndex));

            payload.Stream.WriteByte(header);

            for (int i = 0; i < 4; i++)
            {

                switch (useTypeIndex)
                {
                    case 0:
                        payload.Stream.WriteByte((byte)(sbyte)input[i]);
                        continue;

                    case 1:
                        ShortResolver.Instance.Write(payload, (short)input[i]);
                        continue;

                    case 2:
                        Int32Resolver.Instance.Write(payload, (int)input[i]);
                        continue;

                    case 3:
                        FloatResolver.Instance.Write(payload, input[i]);
                        continue;
                }

            }

            return true;
        }

        protected override ReadResult _Internal_Read(BytePayload payload, out Vector4 output)
        {

            int result = payload.Stream.ReadByte();
            output = default;

            // -1 means we received the end of the stream
            if (result == -1)
                return ReadResult.Failed;

            byte header = (byte)result;

            switch (header)
            {
                case 0:
                    output = Vector4.Zero;
                    return ReadResult.Success;

                case UnitXHeader:
                    output = Vector4.UnitX;
                    return ReadResult.Success;

                case UnitYHeader:
                    output = Vector4.UnitY;
                    return ReadResult.Success;

                case UnitZHeader:
                    output = Vector4.UnitZ;
                    return ReadResult.Success;

                case UnitWHeader:
                    output = Vector4.UnitW;
                    return ReadResult.Success;
            }

            bool absolute = (header & (1 << 7)) != 0;
            int typeIndex = byte.TrailingZeroCount(absolute ? (byte)(header & ~(1 << 7)) : header);

            output = new();

            for (int i = 0; i < 4; i++)
            {
                switch (typeIndex)
                {
                    case 0:
                        output[i] = (sbyte)payload.Stream.ReadByte();
                        continue;

                    case 1:
                        ShortResolver.Instance.Read(payload, out short value);
                        output[i] = value;
                        continue;

                    case 2:
                        Int32Resolver.Instance.Read(payload, out int iValue);
                        output[i] = iValue;
                        continue;

                    case 3:
                        FloatResolver.Instance.Read(payload, out float fValue);
                        output[i] = fValue;
                        continue;
                }
            }

            return absolute ? ReadResult.Success : ReadResult.SuccessDelta;
        }

        protected override bool _Internal_Write(BytePayload payload, Vector4 input)
            => Write(payload, input, absolute: true);
    }

}
