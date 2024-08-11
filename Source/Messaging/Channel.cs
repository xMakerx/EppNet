/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using ENet;

using EppNet.Data.Datagrams;
using EppNet.Logging;
using EppNet.Utilities;

using System;
using System.Collections.Generic;
using System.Threading;

namespace EppNet.Messaging
{

    /// <summary>
    /// This enum represents E++Net reserved channels
    /// </summary>

    public enum Channels : byte
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

        public Action<IDatagram> DatagramReceived;

        public ILoggable Notify { get => this; }

        public readonly byte Id;
        public ChannelFlags Flags { protected set; get; }

        // Statistics
        public int DatagramsReceived { get => _datagramsReceived; }
        public int DatagramsSent { get => _datagramsSent; }

        public long TotalBytesReceived { get => _totalBytesReceived; }

        public long TotalBytesSent { get => _totalBytesSent; }


        private readonly Queue<IDatagram> _buffer;
        private readonly object _bufferLock;

        protected int _datagramsReceived;
        protected int _datagramsSent;
        protected long _totalBytesReceived;
        protected long _totalBytesSent;

        public Channel(ChannelService service, byte id)
        {
            this.Service = service;
            this.Id = id;
            this.Flags = ChannelFlags.None;
            this._buffer = (Flags & ChannelFlags.ProcessImmediately) == 0 ? new() : null;
            this._bufferLock = _buffer != null ? new() : null;
        }

        public Channel(ChannelService service, byte id, ChannelFlags flags) : this(service, id)
        {
            this.Flags = flags;
        }

        public void ReceiveOrQueue(IDatagram datagram)
        {
            if (datagram == null)
                // Nothing to do.
                return;

            if (Flags.HasFlag(ChannelFlags.ProcessImmediately))
                Receive(datagram);
            else if (datagram.Collectible)
            {
                lock (_bufferLock)
                    _buffer.Enqueue(datagram);
            }
        }

        public void Receive(IDatagram datagram)
        {
            Interlocked.Increment(ref _datagramsReceived);
            Interlocked.Add(ref _totalBytesReceived, datagram.Size);
            DatagramReceived?.GlobalInvoke(datagram);
        }

        public void ReceiveQueue()
        {

            IDatagram[] datagrams = new IDatagram[_buffer.Count];

            lock (_bufferLock)
            {
                _buffer.CopyTo(datagrams, 0);
                _buffer.Clear();
            }

            for (int i = 0; i < datagrams.Length; i++)
                Receive(datagrams[i]);
        }

        public void Clear()
        {
            if (_buffer == null)
                // Nothing to clear
                return;

            lock (_bufferLock)
                _buffer.Clear();
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
                    Interlocked.Increment(ref _datagramsReceived);
                    Interlocked.Add(ref _totalBytesSent, bytes.LongLength);
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
            Interlocked.Exchange(ref _datagramsReceived, 0);
            Interlocked.Exchange(ref _datagramsSent, 0);
            Interlocked.Exchange(ref _totalBytesReceived, 0);
            Interlocked.Exchange(ref _totalBytesSent, 0);
        }

        public void Dispose()
        {
            Clear();
            ResetStatistics();

            DatagramReceived = null;
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

        public static explicit operator byte(Channel channel) => channel.Id;
    }

}
