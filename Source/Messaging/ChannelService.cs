/////////////////////////////////////////////
/// Filename: ChannelService.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Services;
using EppNet.Utilities;

using System;
using System.Collections.Concurrent;

namespace EppNet.Messaging
{

    public class ChannelService : Service
    {

        protected ConcurrentDictionary<byte, Channel> _channels;

        public ChannelService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._channels = new();
        }

        public bool TrySendTo(Peer peer, IDatagram datagram, PacketFlags flags)
        {

            if (!datagram.Written)
                datagram.Write();

            Channel channel = GetChannelById(datagram.GetChannelID());
            return channel?.SendTo(peer, datagram.Pack(), flags) == true;
        }

        public bool TrySendDataTo(Peer peer, byte channelId, byte[] bytes, PacketFlags flags)
        {
            Channel channel = GetChannelById(channelId);
            return channel?.SendTo(peer, bytes, flags) == true;
        }

        public bool TryAddChannel(byte id) => TryAddChannel(id, out Channel _);

        public bool TryAddChannel(byte id, out Channel newChannel, ChannelFlags flags = ChannelFlags.None)
        {
            newChannel = null;
            
            if (id < (byte) Enum.GetValues<Channels>().Length)
            {
                // This is a reserved channel id
                string message = $"Channel ID \"{id}\" is reserved!";
                ChannelAlreadyExistsException exp = new(id, message);

                Notify.Error(new TemplatedMessage(message), exp);
                _serviceMgr.Node.HandleException(exp);
                return false;
            }

            return _Internal_TryAddChannel(id, out newChannel, flags);
        }

        /// <summary>
        /// Tries to obtain a <see cref="EppNet.Messaging.Channel"/> by its ID.
        /// </summary>
        /// <returns>A valid <see cref="Channel"/> instance or NULL</returns>

        public Channel GetChannelById(byte id)
        {
            _channels.TryGetValue(id, out Channel channel);

            if (channel == null)
                Notify.Debug(new TemplatedMessage("Failed to obtain Channel \"{channelId}\"!", id));

            return channel;
        }

        public override bool Start()
        {

            bool started = base.Start();

            if (started)
            {
                // Let's add our channels
                _Internal_TryAddChannel((byte)Channels.Connectivity, out var _, ChannelFlags.ProcessImmediately);
                _Internal_TryAddChannel((byte)Channels.Reliable, out var _);
                _Internal_TryAddChannel((byte)Channels.Unreliable, out var _);
            }

            return started;
        }

        private bool _Internal_TryAddChannel(byte id, out Channel newChannel, ChannelFlags flags = ChannelFlags.None)
        {
            newChannel = null;

            if (_channels.ContainsKey(id))
            {
                // The channel already exists
                string message = $"Channel \"{id}\" already exists!";
                ChannelAlreadyExistsException exp = new(id, message);

                Notify.Error(new TemplatedMessage(message), exp);
                _serviceMgr.Node.HandleException(exp);

                return false;
            }

            newChannel = new(this, id, flags);
            _channels[id] = newChannel;

            // Let's send out this debug message just in case.
            Notify.Debug(new TemplatedMessage("Created new Channel \"{channelId}\" with Flags {flags}", id, flags.ToListString()));

            return true;
        }

    }

}