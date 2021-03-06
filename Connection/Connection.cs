﻿using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Numerics;


namespace Connection
{

    public class ConnectionDeadException : Exception
    {
        public ConnectionDeadException() { }
        public ConnectionDeadException(string message) : base(message) { }
    }

    public class NewMessageEventArgs : EventArgs
    {
        public MessagePayload Message;

        public NewMessageEventArgs(MessagePayload message)
        {
            Message = message;
        }
    }

    public class NetworkConfiguration
    {
        public UInt16 DefaultPort { get; }
        public UInt32 Magic { get; }
        public String DNSSeed { get; }
        public UInt32 MinimumChainLength { get; }
        public UInt32 MaximumChainLength { get; }
        public BigInteger MinimumHeaderHash { get; }
        public BigInteger MinimumChainOrigin { get; }
        public BigInteger MaximumChainOrigin { get; }

        public NetworkConfiguration(
            UInt16 defaultPort,
            UInt32 magic,
            String dnsSeed,
            UInt32 minimumChainLength,
            UInt32 maximumChainLength,
            BigInteger minimumHeaderHash,
            BigInteger minimumChainOrigin,
            BigInteger maximumChainOrigin
        )
        {
            DefaultPort = defaultPort;
            Magic = magic;
            DNSSeed = dnsSeed;
            MinimumChainLength = minimumChainLength;
            MaximumChainLength = maximumChainLength;
            MinimumHeaderHash = minimumHeaderHash;
            MinimumChainOrigin = minimumChainLength;
            MaximumChainOrigin = maximumChainOrigin;
        }
    }

    public class Connection
    {
        public event EventHandler<NewMessageEventArgs> NewMessage;

        public IPAddress From { get; }
        public IPAddress To { get; }
        public UInt16 Port { get; }
        public UInt64 Services { get; }
        public Int32 ProtocolVersion { get; }
        public UInt32 StartHeight { get; }
        public NetworkConfiguration NetworkConfig { get; }
        public Boolean Alive;
        public ConnectionDeadException DeathException;

        public List<MessagePayload> SentMessages { get; }
        public List<MessagePayload> ReceivedMessages { get; }

        TcpClient Client;
        Stream Stream;
        Object AliveLock = new Object();
        Object SendLock = new Object();
        Object ReceiveLock = new Object();
        CancellationTokenSource CancelReceivingMessages;

        public Connection(
            IPAddress from,
            IPAddress to,
            UInt16 port,
            NetworkConfiguration networkConfig,
            TcpClient client
        )
        {
            From = from;
            To = to;
            Port = port;
            NetworkConfig = networkConfig;
            SentMessages = new List<MessagePayload>();
            ReceivedMessages = new List<MessagePayload>();
            Client = client;
            Stream = Client.GetStream();
            Alive = true;

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
            ReceiveVerAckMessage();
        }

        ~Connection()
        {
            if (CancelReceivingMessages != null)
            {
                CancelReceivingMessages.Cancel();
                CancelReceivingMessages.Dispose();
            }
            Stream.Close();
            Client.Close();
        }

        public void SendMessage(MessagePayload message)
        {
            lock (SendLock)
            {
                lock (AliveLock)
                {
                    if (DeathException != null)
                    {
                        throw DeathException;
                    }
                }

                try
                {
                    Byte[] data = message.ToBytes();
                    Stream.Write(data, 0, data.Length);
                    SentMessages.Add(message);
                }
                catch
                {
                    lock(AliveLock)
                    {
                        if(DeathException == null)
                        {
                            Alive = false;
                            DeathException = new ConnectionDeadException("SendMessage threw exception.");
                        }
                    }
                    throw DeathException;
                }
            }
        }

        MessagePayload ReceiveMessage()
        {
            lock (ReceiveLock)
            {
                lock (AliveLock)
                {
                    if (DeathException != null)
                    {
                        throw DeathException;
                    }
                }

                try
                {
                    var framer = new MessageFramer(NetworkConfig.Magic);
                    var message = framer.NextMessage(Stream);
                    ReceivedMessages.Add(message);
                    return message;
                }
                catch
                {
                    lock (AliveLock)
                    {
                        if (DeathException == null)
                        {
                            Alive = false;
                            DeathException = new ConnectionDeadException("ReceiveMessage threw exception.");
                        }
                    }
                    throw DeathException;
                }
            }
        }

        public void StartReceivingMessages()
        {
            CancelReceivingMessages = new CancellationTokenSource();
            Task.Run(
                () => {
                    lock(ReceiveLock)
                    {
                        while(true)
                        {
                            try
                            {
                                var message = ReceiveMessage();
                                NewMessage?.Invoke(this, new NewMessageEventArgs(message));
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                },
                CancelReceivingMessages.Token
            );
        }

        public void SendGetDataMessage(InvPayload invPayload)
        {
            var message = new MessagePayload(NetworkConfig.Magic, "getdata", invPayload);
            SendMessage(message);
        }

        void SendVersionMessage()
        {
            var message = new MessagePayload(NetworkConfig.Magic, "version", GetVersionPayload());
            SendMessage(message);
        }

        void SendVerAckMessage()
        {
            var message = new MessagePayload(NetworkConfig.Magic, "verack", new VerAckPayload());
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

        VerAckPayload ReceiveVerAckMessage()
        {
            var message = ReceiveMessage();
            if (message.Command != "verack")
            {
                throw new Exception("Expected verack message.");
            }
            return (VerAckPayload)message.CommandPayload;
        }

        public VersionPayload GetVersionPayload()
        {
            var to = new IPAddressPayload(DateTime.UtcNow, 1, To, Port);
            var from = new IPAddressPayload(DateTime.UtcNow, 0, From, Port);
            var userAgent = new StringPayload("/WinPrime-0.1.0/");
            return new VersionPayload(70001, 1, DateTime.UtcNow, to, from, 0, userAgent, 0, true);
        }
    }
}
