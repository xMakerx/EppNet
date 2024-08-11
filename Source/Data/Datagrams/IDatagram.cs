///////////////////////////////////////////////////////
/// Filename: IDatagram.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using EppNet.Connections;

using System;

namespace EppNet.Data.Datagrams
{

    public interface IDatagram : IDisposable
    {

        public long Size { get; }
        public bool Written { get; }

        /// <summary>
        /// Collectible datagrams are datagrams that are collected within a channel so something can be
        /// executed with it later on. Set this flag to false if you are executing some code from within the Read function
        /// </summary>
        public bool Collectible { get; }

        public void Read();

        public void Write();

        public byte[] Pack();

        public void WriteHeader();
        public byte GetHeader();
        public byte GetChannelID();
        public Connection GetSender();

    }

}
