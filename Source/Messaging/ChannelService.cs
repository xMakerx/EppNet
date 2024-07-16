/////////////////////////////////////////////
/// Filename: ChannelService.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Logging;
using EppNet.Services;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Messaging
{

    public class ChannelService : Service
    {

        protected Dictionary<byte, EppNet.Messaging.Channel> _channels;
        protected object _threadLock;

        public ChannelService(ServiceManager svcMgr) : base(svcMgr)
        {
            this._channels = new();
            this._threadLock = new();
        }

        public bool TryAddChannel(byte id) => TryAddChannel(id, out Channel _);

        public bool TryAddChannel(byte id, out Channel newChannel, ChannelFlags flags = ChannelFlags.None)
        {
            newChannel = null;
            
            if (id < (byte) Enum.GetValues<Channels>().Length)
            {
                // This is a reserved channel id
                string message = "Channel ID \"{channelId}\" is reserved!";
                ChannelAlreadyExistsException exp = new(id, String.Format(message, id));

                Notify.Error(new TemplatedMessage(message, id), exp);
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

            Channel channel = null;
            lock (_threadLock)
            {
                _channels.TryGetValue(id, out channel);

                if (channel == null)
                    Notify.Debug(new TemplatedMessage("Failed to obtain Channel \"{channelId}\"!", id));
            }

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

            lock (_threadLock)
            {
                if (_channels.ContainsKey(id))
                {
                    // The channel already exists
                    string message = "Channel \"{channelId}\" already exists!";
                    ChannelAlreadyExistsException exp = new(id, String.Format(message, id));

                    Notify.Error(new TemplatedMessage(message, id), exp);
                    _serviceMgr.Node.HandleException(exp);

                    return false;
                }

                newChannel = new(id, flags);
                _channels[id] = newChannel;

                // Let's send out this debug message just in case.
                Notify.Debug(new TemplatedMessage("Created new Channel \"{channelId}\" with Flags {flags}", id, flags.ToListString()));
            }

            return true;
        }

    }

}