/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: September 14, 2022
/// Author: Maverick Liberty
//////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace EppNet.Data
{

    public class Channel : IMessageHandler
    {

        #region Static members

        private static object _s_lock = new object();
        private static IDictionary<uint, Channel> _channels = new Dictionary<uint, Channel>()
        {
            // Connectivity Channel
            {0, new Channel(0, ChannelFlags.ProcessImmediately)}
        };

        public static Channel GetById(uint id)
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

        private static void _TryRegister(uint id, Channel channel)
        {
            lock (_s_lock)
            {
                if (_channels.ContainsKey(id))
                    throw new System.Exception($"Channel ID {id} has already been registered!!");

                _channels.Add(id, channel);
            }
        }

        public static bool IsRegistered(uint id) => _channels.ContainsKey(id);

        #endregion

        public enum ChannelFlags : byte
        {
            None                    = 0,

            /// <summary>
            /// Datagrams received are sent to this channel to be processed
            /// immediately after reception rather than waiting for a new simulation
            /// tick.
            /// </summary>
            ProcessImmediately      = 1 << 0,
        }

        public event Action<Datagram> OnDatagramRead;

        public readonly uint ID;
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
        protected int _datagrams_received;
        protected int _datagrams_sent;

        /// <summary>
        /// Instantiates a new channel with <see cref="ChannelFlags.None"/>
        /// </summary>
        /// <param name="id"></param>

        private Channel(uint id) : this(id, ChannelFlags.None) { }

        private Channel(uint id, ChannelFlags flags)
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
        /// This is 
        /// </summary>
        /// <param name="datagram"></param>

        public void OnDatagramReceived(Datagram datagram)
        {
            DatagramsReceived++;
            OnDatagramRead?.Invoke(datagram);
        }

    }

}
