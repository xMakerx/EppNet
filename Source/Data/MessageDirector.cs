///////////////////////////////////////////////////////
/// Filename: MessageDirector.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using Disruptor;

using ENet;

using EppNet.Connections;
using EppNet.Registers;
using EppNet.Sim;
using EppNet.Data.Datagrams;
using EppNet.Utilities;

using System;
using System.Collections.Generic;

namespace EppNet.Data
{

    /// <summary>
    /// Handles the reception of datagrams and determines whether or not
    /// they should be passed to their respective channels now or enqueued.
    /// </summary>

    public class MessageDirector : IEventHandler<SimulationTickEvent>
    {

        /// <summary>
        /// Invoked when a new <see cref="Datagram"/> is received.
        /// Passes the <see cref="Datagram"/> and whether or not it was enqueued.<br/>
        /// Don't use this to bypass how datagrams are meant to be handled. Should be looked at as a means to
        /// debug problems.
        /// </summary>
        public event Action<Datagram, bool> OnDatagramReceived;

        protected readonly DatagramRegister _datagram_register;

        protected object _lock;
        protected List<Datagram> _datagram_queue;

        public MessageDirector()
        {
            this._datagram_register = DatagramRegister.Get();

            this._lock = new object();
            this._datagram_queue = new List<Datagram>();
        }

        public void OnEvent(SimulationTickEvent tick_event, long sequence, bool end_of_batch)
        {
            Console.WriteLine($"MessageDirector: Processing SimulationTickEvent with Sequence ID {sequence}");
            DispatchAll();
        }

        /// <summary>
        /// Enqueues a datagram for dispatch.
        /// </summary>
        /// <param name="datagram"></param>
        /// <returns></returns>

        public bool Enqueue(Datagram datagram)
        {
            if (datagram == null)
                return false;

            lock (_lock)
            {
                // Obtains a lock so we're the only ones accessing the queue container.
                _datagram_queue.Add(datagram);
            }

            return true;
        }

        public virtual void Dispatch(Datagram datagram)
        {
            if (datagram == null)
                return;

            byte channel_id = datagram.ChannelID;
            Channel channel = Channel.GetById(channel_id);

            channel.OnDatagramReceived(datagram);
        }

        /// <summary>
        /// Dispatches all enqueued messages to their respective recipients. 
        /// </summary>
        public virtual void DispatchAll()
        {
            List<Datagram> datagrams = new List<Datagram>();

            lock (_lock)
            {
                datagrams.AddRange(_datagram_queue);
                _datagram_queue.Clear();
            }

            foreach (Datagram datagram in datagrams)
                Dispatch(datagram);
        }

        public virtual void OnPacketReceived(Connection sender, Packet packet, byte channelID)
        {
            byte[] received_data = new byte[packet.Length];
            packet.CopyTo(received_data);

            byte header = received_data[0];

            IRegistration registration = _datagram_register.Get(header);

            if (registration == null)
                throw new ArgumentException($"Received unknown Datagram of ID {header}. Did you forget to register it?");

            Datagram datagram;

            try
            {
                datagram = (Datagram)registration.NewInstance(received_data);
                datagram.ChannelID = channelID;
                datagram.Sender = sender;
                datagram.Read();

                Channel channel = Channel.GetById(channelID);
                bool queue = !channel.Flags.IsFlagSet(ChannelFlags.ProcessImmediately);

                try
                {
                    if (queue)
                        Enqueue(datagram);
                    else
                        Dispatch(datagram);

                    try
                    {
                        OnDatagramReceived?.Invoke(datagram, queue);
                    }
                    catch (Exception) { }
                }
                catch (Exception) { }
            }
            catch (Exception)
            {
                // The data received was malformed.
            }
            finally
            {
                // Let's dispose of the native ENet packet.
                packet.Dispose();
            }

        }

    }

}
