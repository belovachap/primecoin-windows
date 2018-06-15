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
        public NetworkStream Stream { get; }
        public Int32 ProtocolVersion { get; }

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
            Stream = Client.GetStream();

            SendVersionMessage();
            //var verAck = ReceiveVerAckMessage();
            //var version = ReceiveVersionMessage();
        }

        ~Connection()
        {
            Stream.Close();
            Client.Close();
        }

        void SendMessage(String command, Payload payload)
        {

        }

        void SendMessage(MessagePayload message)
        {
            Byte[] data = message.ToBytes();
            Stream.Write(data, 0, data.Length);
        }

        MessagePayload ReceiveMessage()
        {
            var buffer = new Byte[1024];
            Int32 bytesRead = Stream.Read(buffer, 0, buffer.Length);
            return new MessagePayload(buffer);
        }

        void SendVersionMessage()
        {
            var payload = GetVersionPayload();
            SendMessage("version", payload);
        }

        //VerAckPayload ReceiveVerAckMessage()
        //{
        //    var message = new MessagePayload();
        //    return (VerAckPayload)message.CommandPayload;
        //}

        //VersionPayload ReceiveVersionMessage()
        //{
        //    return new VersionPayload();
        //}

        public VersionPayload GetVersionPayload()
        {
            var to = new IPAddressPayload(DateTime.UtcNow, 1, To, Port);
            var from = new IPAddressPayload(DateTime.UtcNow, 0, From, Port);
            var userAgent = new StringPayload("/WinPrime-1.0.0/");
            return new VersionPayload(70001, 1, DateTime.UtcNow, to, from, 0, userAgent, 0, true);
        }
    }
}
