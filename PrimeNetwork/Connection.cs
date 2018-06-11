using System;
using System.Net;
using System.Net.Sockets;

namespace PrimeNetwork
{
    public class Connection
    {
        public IPAddress From { get; }
        public IPAddress To { get; }
        public UInt16 Port { get; }
        public UInt64 Services { get; }
        public TcpClient Client { get; }

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
