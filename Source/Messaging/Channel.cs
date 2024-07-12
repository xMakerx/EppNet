/////////////////////////////////////////////
/// Filename: Channel.cs
/// Date: July 11, 2024
/// Author: Maverick Liberty
//////////////////////////////////////////////

using EppNet.Logging;

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

        public float TotalDataReceivedKb
        { 
            private set
            {

                lock (_threadLock)
                    _totalDataReceivedKb = value;
            }

            get => _totalDataReceivedKb;
        }

        public float TotalDataSentKb
        {
            private set
            {
                lock (_threadLock)
                    _totalDataSentKb = value;
            }

            get => _totalDataSentKb;
        }


        private object _threadLock;

        // Statistics
        protected int _datagramsReceived;
        protected int _datagramsSent;
        protected float _totalDataReceivedKb;
        protected float _totalDataSentKb;

        public Channel(byte id)
        {
            this.Id = id;
            this.Flags = ChannelFlags.None;

            this._threadLock = new();
            this._datagramsReceived = 0;
            this._datagramsSent = 0;
            this._totalDataReceivedKb = 0f;
            this._totalDataSentKb = 0f;
        }

        public Channel(byte id, ChannelFlags flags) : this(id)
        {
            this.Flags = flags;
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
                _totalDataReceivedKb = 0f;
                _totalDataSentKb = 0f;

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
        public static bool operator !=(Channel left, Channel right) => !left.Equals(right);
    }

}
