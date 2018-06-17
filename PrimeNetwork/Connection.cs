using System;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace PrimeNetwork
{
    public class Connection
    {
        public IPAddress From { get; }
        public IPAddress To { get; }
        public UInt16 Port { get; }
        public UInt64 Services { get; }
        public TcpClient Client { get; }
        public Stream Stream { get; }
        public Int32 ProtocolVersion { get; }
        public UInt32 StartHeight { get; }

        public Connection(IPAddress from, IPAddress to, UInt16 port, TcpClient client)
        {
            From = from;
            To = to;
            Port = port;
            Client = client;
            Stream = Client.GetStream();

            SendVersionMessage();

            var version = ReceiveVersionMessage();
            if (version.Version < 70001)
            {
                throw new Exception("Can only talk to protocol 70001 or better.");
            }
            ProtocolVersion = 70001;
            Services = version.Services;
            StartHeight = version.StartHeight;

            SendVerAckMessage();
        }

        ~Connection()
        {
            Stream.Close();
            Client.Close();
        }

        void SendMessage(MessagePayload message)
        {
            Byte[] data = message.ToBytes();
            Stream.Write(data, 0, data.Length);
        }

        MessagePayload ReceiveMessage()
        {
            var framer = new MessageFramer();
            return framer.NextMessage(Stream);           
        }

        void SendVersionMessage()
        {
            UInt32 magic = 0xE7E5E7E4;
            var message = new MessagePayload(magic, "version", GetVersionPayload());
            SendMessage(message);
        }

        void SendVerAckMessage()
        {
            UInt32 magic = 0xE7E5E7E4;
            var message = new MessagePayload(magic, "verack", new VerAckPayload());
            SendMessage(message);
        }

        VersionPayload ReceiveVersionMessage()
        {
            var message = ReceiveMessage();
            if (message.Command != "version")
            {
                throw new Exception("Expected version message.");
            }
            return (VersionPayload)message.CommandPayload;
        }

        public VersionPayload GetVersionPayload()
        {
            var to = new IPAddressPayload(DateTime.UtcNow, 1, To, Port);
            var from = new IPAddressPayload(DateTime.UtcNow, 0, From, Port);
            var userAgent = new StringPayload("/WinPrime-1.0.0/");
            return new VersionPayload(70001, 1, DateTime.UtcNow, to, from, 0, userAgent, 0, true);
        }
    }
}
