using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Protocol
{
    public class MessagePayload : Payload
    {
        public UInt32 Magic { get; }
        public String Command { get; }
        public Payload CommandPayload { get; }

        public MessagePayload(UInt32 magic, String command, Payload commandPayload)
        {
            if (Encoding.ASCII.GetBytes(command).Length > 12)
            {
                throw new ArgumentException("command must be 12 or fewer ascii bytes");
            }
            Magic = magic;
            Command = command;
            CommandPayload = commandPayload;
        }

        public MessagePayload(byte[] bytes)
        {
            Magic = BitConverter.ToUInt32(bytes, 0);
            var remaining = bytes.Skip(4);

            Command = Encoding.ASCII.GetString(remaining.Take(12).ToArray());
            Command = Command.TrimEnd(new char[] { '\0' });
            remaining = remaining.Skip(12);

            var length = BitConverter.ToUInt32(remaining.ToArray(), 0);
            remaining = remaining.Skip(4);

            var checkSum = remaining.Take(4).ToArray();
            remaining = remaining.Skip(4);

            var payload = remaining.Take((Int32)length).ToArray();

            if (payload.Length != length)
            {
                throw new ArgumentException("payload was not of expected length!");
            }

            SHA256 sha256 = SHA256Managed.Create();
            var computedCheckSum = sha256.ComputeHash(sha256.ComputeHash(payload)).Take(4);

            if (!checkSum.SequenceEqual(computedCheckSum.ToArray()))
            {
                throw new ArgumentException("checksum of payload does not match!");
            }

            switch (Command)
            {
                case "block":
                    CommandPayload = new BlockPayload(payload);
                    break;

                case "inv":
                    CommandPayload = new InvPayload(payload);
                    break;

                case "verack":
                    CommandPayload = new VerAckPayload(payload);
                    break;

                case "version":
                    CommandPayload = new VersionPayload(payload);
                    break;

                default:
                    CommandPayload = new UnknownPayload(payload);
                    break;
            }
        }

        public override byte[] ToBytes()
        {
            var magicBytes = BitConverter.GetBytes(Magic);

            var paddedCommandBytes = new Byte[12];
            var commandBytes = Encoding.ASCII.GetBytes(Command);
            for (Int32 i = 0; i < commandBytes.Length; i++)
            {
                paddedCommandBytes[i] = commandBytes[i];
            }

            var payloadBytes = CommandPayload.ToBytes();
            var lengthBytes = BitConverter.GetBytes((UInt32)payloadBytes.Length);

            SHA256 sha256 = SHA256Managed.Create();
            var checkSumBytes = sha256.ComputeHash(sha256.ComputeHash(payloadBytes)).Take(4);

            return magicBytes
                   .Concat(paddedCommandBytes)
                   .Concat(lengthBytes)
                   .Concat(checkSumBytes)
                   .Concat(payloadBytes)
                   .ToArray();
        }
    }
}
