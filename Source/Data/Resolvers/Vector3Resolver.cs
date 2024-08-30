///////////////////////////////////////////////////////
/// Filename: Vector3Resolver.cs
/// Date: August 30, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace EppNet.Data
{

    public class Vector3Resolver : Resolver<Vector3>
    {

        public static readonly Vector3Resolver Instance = new();

        static Vector3Resolver()
            => BytePayload.AddResolver(typeof(Vector3), Instance);

        public const byte AbsoluteHeader = 128;
        public const byte UnitXHeader = 16;
        public const byte UnitYHeader = 32;
        public const byte UnitZHeader = 64;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Write(BytePayload payload, Vector3 input, bool absolute = true)
        {

            if (input == Vector3.Zero || input == Vector3.UnitX || input == Vector3.UnitY || input == Vector3.UnitZ)
            {
                if (input == Vector3.Zero)
                    payload.Stream.WriteByte(0);
                else
                    payload.Stream.WriteByte(input == Vector3.UnitX ? UnitXHeader : input == Vector3.UnitY ? UnitYHeader : UnitZHeader);

                payload.Stream.Advance(1);
                return true;
            }

            Vector3 typeIndices = Vector3.Zero;

            for (int i = 0; i < 3; i++)
            {
                float value = input[i];
                int typeIndex;

                if (sbyte.MinValue <= value && value <= sbyte.MaxValue)
                    typeIndex = 0;

                else if (short.MinValue <= value && value <= short.MaxValue)
                    typeIndex = 1;

                else if (int.MinValue <= value && value <= int.MaxValue)
                    typeIndex = 2;
                else
                    typeIndex = 3;

                typeIndices[i] = typeIndex;
            }

            int useTypeIndex = (int)MathF.Max(MathF.Max(typeIndices[0], typeIndices[1]), typeIndices[2]);

            byte header = absolute ? AbsoluteHeader : (byte)0;
            header = (byte)(header | (1 << useTypeIndex));

            payload.Stream.WriteByte(header);
            payload.Stream.Advance(1);

            for (int i = 0; i < 3; i++)
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

        protected override ReadResult _Internal_Read(BytePayload payload, out Vector3 output)
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
                    output = Vector3.Zero;
                    return ReadResult.Success;

                case UnitXHeader:
                    output = Vector3.UnitX;
                    return ReadResult.Success;

                case UnitYHeader:
                    output = Vector3.UnitY;
                    return ReadResult.Success;

                case UnitZHeader:
                    output = Vector3.UnitZ;
                    return ReadResult.Success;
            }

            bool absolute = (header & (1 << 7)) != 0;
            int typeIndex = byte.TrailingZeroCount(absolute ? (byte)(header & ~(1 << 7)) : header);

            output = new();

            for (int i = 0; i < 3; i++)
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

        protected override bool _Internal_Write(BytePayload payload, Vector3 input)
            => Write(payload, input, absolute: true);
    }

}
