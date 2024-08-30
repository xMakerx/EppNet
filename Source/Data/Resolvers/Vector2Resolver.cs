///////////////////////////////////////////////////////
/// Filename: Vector2Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;

namespace EppNet.Data
{

    public class Vector2Resolver : Resolver<Vector2>
    {

        public static readonly Vector2Resolver Instance = new();

        public const byte AbsoluteHeader = 128;
        public const byte UnitXHeader = 16;
        public const byte UnitYHeader = 32;

        public Vector2Resolver() : base(autoAdvance: false) { }

        public bool Write(BytePayload payload, Vector2 input, bool absolute = true)
        {
            if (input == Vector2.Zero || input == Vector2.UnitX || input == Vector2.UnitY)
            {
                if (input == Vector2.Zero)
                    payload.Stream.WriteByte(0);
                else
                    payload.Stream.WriteByte(input == Vector2.UnitX ? UnitXHeader : UnitYHeader);

                payload.Stream.Advance(1);
                return true;
            }

            Vector2 typeIndices = Vector2.Zero;

            for (int i = 0; i < 3; i++)
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

            int useTypeIndex = (int) Math.Max(typeIndices.X, typeIndices.Y);

            byte header = absolute ? AbsoluteHeader : (byte)0;
            header = (byte)(header | (1 << useTypeIndex));

            payload.Stream.WriteByte(header);
            payload.Stream.Advance(1);

            for (int i = 0; i < 2; i++)
            {

                switch (useTypeIndex)
                {
                    case 0:
                        payload.Stream.WriteByte((byte)(sbyte)input[i]);
                        payload.Stream.Advance(1);
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

        protected override ReadResult _Internal_Read(BytePayload payload, out Vector2 output)
        {

            int result = payload.Stream.ReadByte();
            output = default;

            // -1 means we received the end of the stream
            if (result == -1)
                return ReadResult.Failed;

            payload.Stream.Advance(1);

            byte header = (byte)result;

            switch (header)
            {
                case 0:
                    output = Vector2.Zero;
                    return ReadResult.Success;

                case UnitXHeader:
                    output = Vector2.UnitX;
                    return ReadResult.Success;

                case UnitYHeader:
                    output = Vector2.UnitY;
                    return ReadResult.Success;
            }

            bool absolute = (header & (1 << 7)) != 0;
            int typeIndex = byte.TrailingZeroCount(absolute ? (byte)(header & ~(1 << 7)) : header);

            output = new();

            for (int i = 0; i < 2; i++)
            {
                switch (typeIndex)
                {
                    case 0:
                        output[i] = (sbyte)payload.Stream.ReadByte();
                        payload.Stream.Advance(1);
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

        protected override bool _Internal_Write(BytePayload payload, Vector2 input)
            => Write(payload, input, absolute: true);

    }

}
