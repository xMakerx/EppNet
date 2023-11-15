/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: September 14, 2023
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using EppNet.Data.Datagrams;

namespace EppNet.Data
{

    /// <summary>
    /// While channels are <see cref="IMessageHandler"/>s, they do not have to be registered with the
    /// <see cref="MessageDirector"/>.
    /// </summary>

    public class Channel : IMessageHandler
    {

        #region Static members

        private static object _s_lock = new object();
        private static IDictionary<byte, Channel> _channels = new Dictionary<byte, Channel>();

        static Channel()
        {
            // The connectivity channel
            //-> Ping Datagrams (Clock Sync)
            //-> Authentication
            new Channel(0x0, ChannelFlags.ProcessImmediately);

            // Reliable object updates channel
            new Channel(0x1);

            // Unreliable object updates channel (snapshots)
            new Channel(0x2);
        }

        /// <summary>
        /// Fetches the <see cref="Channel"/> object for the specified channel ID.<br/>
        /// Always returns a valid instance.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        public static Channel GetById(byte id)
        {

            Channel channel;

            lock (_s_lock)
            {
                _channels.TryGetValue(id, out channel);
            }

            if (channel == null)
            {
                // Channel does not have a definition. Let's make a default one for it.
                channel = new Channel(id);
            }

            return channel;
        }

        private static void _TryRegister(byte id, Channel channel)
        {
            lock (_s_lock)
            {
                if (_channels.ContainsKey(id))
                    throw new Exception($"Channel ID {id} has already been registered!!");

                _channels.Add(id, channel);
            }
        }

        public static bool IsRegistered(byte id) => _channels.ContainsKey(id);

        #endregion

        public event Action<Datagram> OnDatagramRead;

        public readonly byte ID;
        public ChannelFlags Flags { internal set; get; }

        /// <summary>
        /// Atomically updates the number of datagrams received.
        /// </summary>

        public int DatagramsReceived
        {

            internal set
            {
                lock (_lock)
                {
                    _datagrams_received = value;
                }
            }

            get => _datagrams_received;

        }

        /// <summary>
        /// Atomically updates the number of datagrams sent.
        /// </summary>

        public int DatagramsSent
        {

            internal set
            {
                lock (_lock)
                {
                    _datagrams_sent = value;
                }
            }

            get => _datagrams_sent;
        }

        protected object _lock;
        private int _datagrams_received;
        private int _datagrams_sent;

        /// <summary>
        /// Instantiates a new channel with <see cref="ChannelFlags.None"/>
        /// </summary>
        /// <param name="id"></param>

        private Channel(byte id) : this(id, ChannelFlags.None) { }

        private Channel(byte id, ChannelFlags flags)
        {
            // Tries to register the channel
            _TryRegister(id, this);

            this.ID = id;
            this.Flags = flags;
            this._lock = new object();
            this._datagrams_received = 0;
            this._datagrams_sent = 0;
        }

        /// <summary>
        /// A new datagram has been received. Internally invokes the <see cref="OnDatagramRead"/> event
        /// after the datagram received counter is updated.
        /// </summary>
        /// <param name="datagram"></param>

        public void OnDatagramReceived(Datagram datagram)
        {
            DatagramsReceived++;
            OnDatagramRead?.Invoke(datagram);
        }

    }

}
