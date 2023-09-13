﻿//////////////////////////////////////////////
/// Filename: DatagramRegister.cs
/// Date: September 13, 2022
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Data;

namespace EppNet.Registers
{

    public class DatagramRegister : Register<byte, IDatagram>
    {

        public static readonly DatagramRegister Instance = new DatagramRegister();
        public static DatagramRegister Get() => Instance;

        public DatagramRegister() : base()
        {
            Add<PingDatagram>(0x1);
        }

        public override bool IsValidKey(byte key)
        {
            if (key < 1)
                throw new System.ArgumentException($"{GetType().Name} requires keys greater than 1!");

            return base.IsValidKey(key);
        }
    }

}