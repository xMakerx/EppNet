﻿//////////////////////////////////////////////
/// Filename: DatagramRegister.cs
/// Date: September 13, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Data.Datagrams;

namespace EppNet.Registers
{
    public class DatagramRegister : Register<byte, Datagram>
    {

        public static readonly DatagramRegister Instance = new DatagramRegister();
        public static DatagramRegister Get() => Instance;

        public DatagramRegister()
        {
            Add<PingDatagram>(0x1);
            Add<DisconnectDatagram>(0x2);
            Add<ObjectUpdateDatagram>(0x3);
        }

        public override bool IsValidKey(byte key)
        {
            if (key < 1)
                throw new System.ArgumentException($"{GetType().Name} requires keys greater than 1!");

            return base.IsValidKey(key);
        }
    }

}
