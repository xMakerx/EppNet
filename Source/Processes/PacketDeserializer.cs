/////////////////////////////////////////////
/// Filename: PacketDeserializer.cs
/// Date: August 6, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using Disruptor;
using Disruptor.Dsl;

using ENet;

using EppNet.Connections;
using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Processes.Events;
using EppNet.Registers;
using EppNet.Sockets;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EppNet.Processes
{

    public class PacketDeserializer : ManagedDisruptor<PacketReceivedEvent>, IEventHandler<PacketReceivedEvent>
    {

        public int DroppedPackets { private set; get; }
        public long DroppedBytes { private set; get; }

        protected BaseSocket _socket;
        protected DatagramRegister _dgRegister;

        public PacketDeserializer([NotNull] BaseSocket socket, int bufferSize) : base(socket.Node, 
            () => new PacketReceivedEvent(), bufferSize, TaskScheduler.Default, 
            ProducerType.Single, new BlockingSpinWaitWaitStrategy())
        {
            this._socket = socket;
            this._dgRegister = DatagramRegister.Get();
            this.DroppedBytes = 0;

            this.HandleEventsWith(this).Then(socket.ChannelService);
        }

        public void OnEvent(PacketReceivedEvent data, long sequence, bool endOfBatch)
        {
            byte[] bytes = data.Data;
            byte header = bytes[0];

            IRegistration dgRegistration = _dgRegister.Get(header);

            if (dgRegistration == null)
            {
                Notify.Error(new TemplatedMessage("Received new datagram of unknown type with header {id}! Is it registered correctly?", header));
                return;
            }

            data.Datagram = dgRegistration.NewInstance() as IDatagram;
            data.Datagram.Read();
        }

        public void HandlePacket(Peer peer, Packet packet, byte channelID, int timeoutMs = 10)
        {
            byte[] data = new byte[packet.Length];
            packet.CopyTo(data);

            Action<PacketReceivedEvent> setupAction = (PacketReceivedEvent @event) => SetupPacket(@event, peer, data, channelID);
            bool fetched = this.TryGetAndPublishEvent(setupAction);

            if (!fetched)
            {
                // Let's retry for up to our timeout interval
                _ = this.TryGetAndPublishEventAsync(setupAction, () => DropPacket(data), timeoutMs);
            }
        }

        public void SetupPacket(PacketReceivedEvent @event, Peer peer, byte[] data, byte channelID)
        {
            @event.Initialize(_socket.ConnectionService.Get(peer.ID), data, channelID);
        }

        public void DropPacket(byte[] data)
        {
            DroppedBytes += data.Length;
            DroppedPackets++;
            Notify.Warning("Dropped packet!");
        }

    }


}
