///////////////////////////////////////////////////////
/// Filename: MessageDirector.cs
/// Date: September 12, 2023
/// Author: Maverick Liberty
///////////////////////////////////////////////////////

using ENet;

using EppNet.Connections;
using EppNet.Registers;

using System;

namespace EppNet.Data
{

    public class MessageDirector
    {

        public event Action<Datagram> OnDatagramReceived;

        protected readonly DatagramRegister _dgRegister;

        public MessageDirector()
        {
            this._dgRegister = DatagramRegister.Get();
        }

        public virtual void OnPacketReceived(Connection sender, Packet packet, byte channelID)
        {
            byte[] received_data = new byte[packet.Length];
            packet.CopyTo(received_data);

            byte header = received_data[0];

            IRegistrationBase registration = _dgRegister.Get(header);

            if (registration == null)
                throw new ArgumentException($"Received unknown Datagram of ID {header}. Did you forget to register it?");

            bool datagram_valid = false;
            Datagram datagram;

            try
            {
                datagram = (Datagram)registration.NewInstance(received_data);
                datagram.ChannelID = channelID;
                datagram.Sender = sender;
                datagram.Read();

                datagram_valid = true;
            }
            catch (Exception e)
            {

            }
            finally
            {
                // Let's dispose of the native ENet packet.
                packet.Dispose();
            }


            // Direct the datagram to where it needs to be!
            OnDatagramReceived?.Invoke(datagram);

        }

    }

}
