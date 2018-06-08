using System;
using System.Net;
using System.Net.Sockets;

namespace PrimeNetwork
{
    public class Connection
    {
        IPAddress From;
        IPAddress To;
        UInt16 Port;
        UInt64 Services;
        TcpClient Client;

        public Connection(
            IPAddress from,
            IPAddress to,
            UInt16 port,
            TcpClient client
        )
        {
            From = from;
            To = to;
            Port = port;
            Client = client;

            SendVersionMessage();
            var verAck = ReceiveVerAckMessage();
            var version = ReceiveVersionMessage();
        }

        void SendVersionMessage()
        {
            var message = new MessagePayload();
        }

        VerAckPayload ReceiveVerAckMessage()
        {
            var message = new MessagePayload();
            return (VerAckPayload)message.CommandPayload;
        }

        VersionPayload ReceiveVersionMessage()
        {
            return new VersionPayload();
        }
    }
}
