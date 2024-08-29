/////////////////////////////////////////////
/// Filename: PacketDeserializer.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Processes.Events;
using EppNet.Registers;
using EppNet.Sockets;

using System;
using System.Diagnostics.CodeAnalysis;

namespace EppNet.Processes
{

    public class PacketDeserializer : IBufferEventHandler<PacketReceivedEvent>, ILoggable
    {

        public int DroppedPackets { private set; get; }
        public long DroppedBytes { private set; get; }

        public ILoggable Notify { get => this; }

        protected BaseSocket _socket;
        protected DatagramRegister _dgRegister;
        protected MultithreadedBuffer<PacketReceivedEvent> _buffer;

        public PacketDeserializer([NotNull] BaseSocket socket, int bufferSize)
        {
            this._socket = socket;

            MultithreadedBufferBuilder<PacketReceivedEvent> builder = new(socket.Node, bufferSize);
            this._buffer = builder.ThenUseHandlers(this).ThenUseHandlers(socket.ChannelService).Build();

            this._dgRegister = DatagramRegister.Get();
            this.DroppedBytes = 0;
        }

        public bool Handle(PacketReceivedEvent data)
        {
            byte[] bytes = data.Data;
            byte header = bytes[0];

            IRegistration dgRegistration = _dgRegister.Get(header);

            if (dgRegistration == null)
            {
                Notify.Error(new TemplatedMessage("Received new datagram of unknown type with header {id}! Is it registered correctly?", header));
                return false;
            }

            Datagram dg = dgRegistration.NewInstance() as Datagram;
            dg.Sender = data.Sender;
            dg.ReadFrom(data.Data);
            data.Datagram = dg;

            dg.Read();
            return true;
        }

        public void Start() => _buffer.Start();
        public void Cancel() => _buffer.Stop();

        public void HandlePacket(Peer peer, Packet packet, byte channelID, int timeoutMs = 10)
        {
            byte[] data = new byte[packet.Length];
            packet.CopyTo(data);

            Action<PacketReceivedEvent> setupAction = (PacketReceivedEvent @event) => SetupPacket(@event, peer, data, channelID);
            _buffer.CreateAndWrite(setupAction);
        }

        public void SetupPacket(PacketReceivedEvent @event, Peer peer, byte[] data, byte channelID)
        {
            if (_socket is ServerSocket srvSocket)
                @event.Initialize(srvSocket.ConnectionService.Get(peer.ID), data, channelID);
            else
                @event.Initialize(_socket.Companion, data, channelID);
        }

        public void DropPacket(byte[] data)
        {
            DroppedBytes += data.Length;
            DroppedPackets++;
            Notify.Warning("Dropped packet!");
        }
    }


}
