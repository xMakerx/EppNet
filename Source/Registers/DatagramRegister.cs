//////////////////////////////////////////////
/// Filename: DatagramRegister.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data.Datagrams;
using EppNet.Utilities;

using System;
using System.Buffers.Binary;
using System.Numerics;

namespace EppNet.Registers
{
    public sealed class DatagramRegister : Register<byte, Datagram>
    {

        public static readonly DatagramRegister Instance = new();
        public static DatagramRegister Get() => Instance;

        private DatagramRegister()
        {
            TryRegister<PingDatagram>();
            TryRegister<DisconnectDatagram>();
            TryRegister<ObjectUpdateDatagram>();
        }

        public override CompilationResult Compile()
        {
            CompilationResult result = base.Compile();

            if (result.Successful)
            {
                Datagram.HeaderByteLength = (int)MathF.Ceiling(Registrations / 255f);

                byte check = (byte) (Registrations - 1);
                Datagram.AvailableHeaderBits = BitOperations.LeadingZeroCount(check);

                byte b = 0;

                for (int i = 0; i < Datagram.AvailableHeaderBits; i++)
                    b = b.EnableBit(i);

                Datagram.MaxHeaderDataDecimalValue = b;
            }

            return result;
        }

        public override bool TryGetNew<T>(out T instance)
        {
            instance = default;

            if (_type2Keys.TryGetValue(typeof(T), out byte key))
            {
                IRegistration registration = _lookupTable[key];
                instance = (T)registration.NewInstance();
                instance.Index = key;
                return true;
            }

            return false;
        }

    }

}
