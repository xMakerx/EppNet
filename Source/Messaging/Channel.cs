/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Logging;

using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

namespace EppNet.Messaging
{

    /// <summary>
    /// This enum represents E++Net reserved channels
    /// </summary>

    public enum Channels
    {

        /// <summary>
        /// Handles messages related to connectivity and synchronization<br/>
        /// Examples: Ping datagrams, joins, exits
        /// </summary>
        Connectivity = 0x0,

        /// <summary>
        /// Handles messages that are guaranteed to arrive
        /// </summary>
        Reliable     = 0x1,

        /// <summary>
        /// Handles messages that aren't guaranteed to arrive<br/>
        /// Example: Snapshot messages
        /// </summary>
        Unreliable   = 0x2
    }

    public class Channel : ILoggable, IDisposable, IEquatable<Channel>
    {

        public readonly ChannelService Service;

        public ILoggable Notify { get => this; }

        public readonly byte Id;
        public ChannelFlags Flags { protected set; get; }

        public int DatagramsReceived
        {
            private set
            {
                lock (_threadLock)
                    _datagramsReceived = value;
            }

            get => _datagramsReceived;
        }

        public int DatagramsSent
        {
            private set
            {
                lock (_threadLock)
                    _datagramsSent = value;
            }

            get => _datagramsSent;
        }

        public long TotalBytesReceived
        { 
            private set
            {

                lock (_threadLock)
                    _totalBytesReceived = value;
            }

            get => _totalBytesReceived;
        }

        public long TotalBytesSent
        {
            private set
            {
                lock (_threadLock)
                    _totalBytesSent = value;
            }

            get => _totalBytesSent;
        }


        private object _threadLock;

        // Statistics
        protected int _datagramsReceived;
        protected int _datagramsSent;
        protected long _totalBytesReceived;
        protected long _totalBytesSent;

        public Channel(ChannelService service, byte id)
        {
            this.Service = service;
            this.Id = id;
            this.Flags = ChannelFlags.None;

            this._threadLock = new();
            this._datagramsReceived = 0;
            this._datagramsSent = 0;
            this._totalBytesReceived = 0L;
            this._totalBytesSent = 0L;
        }

        public Channel(ChannelService service, byte id, ChannelFlags flags) : this(service, id)
        {
            this.Flags = flags;
        }

        public bool SendTo(Peer peer, byte[] bytes, PacketFlags flags)
        {
            // Create an ENet packet
            Packet packet = new();

            try
            {
                packet.Create(bytes, flags);

                // Send the packet to our ENet peer
                if (peer.Send(Id, ref packet))
                {
                    DatagramsSent++;
                    _totalBytesSent += bytes.LongLength;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Notify.Error($"Failed to send Datagram to Peer {peer.ID}. Error {ex.Message}\n{ex.StackTrace}");
            }
            finally
            {
                // No matter what, dispose of this packet.
                packet.Dispose();
            }

            // Failed to send the datagram.
            return false;
        }

        /// <summary>
        /// Checks if a flag is enabled
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool HasFlag(ChannelFlags flag) => Flags.HasFlag(flag);

        public void ResetStatistics()
        {
            lock (_threadLock)
            {
                _datagramsReceived = 0;
                _datagramsSent = 0;
                _totalBytesReceived = 0L;
                _totalBytesSent = 0L;

                Notify.Debug("Statistics were reset!");
            }
        }

        public void Dispose()
        {
            ResetStatistics();
        }

        public bool Equals(Channel other) => other.Id == Id;

        public override bool Equals(object obj)
        {
            if (obj is Channels channelsEnum)
                return Id == (byte)channelsEnum;

            if (obj is Channel other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode() => Id;

        public static bool operator ==(Channel left, Channel right) => left.Equals(right);

        public static bool operator ==(Channel left, Channels right) => left.Equals(right);

        public static bool operator !=(Channel left, Channel right) => !left.Equals(right);

        public static bool operator !=(Channel left, Channels right) => !left.Equals(right);
    }

}
