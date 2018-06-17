using System;
using System.IO;

namespace PrimeNetwork
{
    class MessageFramer
    {
        public MessagePayload NextMessage(Stream stream)
        {
            var message = new MemoryStream();

            // Read the Magic Bytes.
            Int32 nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                throw new Exception("Stream closed.");
            }
            if (nextByte != 0xE4)
            {
                throw new Exception("Was expecting first magic byte.");
            }
            message.WriteByte((Byte)nextByte);

            nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                throw new Exception("Stream closed.");
            }
            if (nextByte != 0xE7)
            {
                throw new Exception("Was expecting second magic byte.");
            }
            message.WriteByte((Byte)nextByte);

            nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                throw new Exception("Stream closed.");
            }
            if (nextByte != 0xE5)
            {
                throw new Exception("Was expecting third magic byte.");
            }
            message.WriteByte((Byte)nextByte);

            nextByte = stream.ReadByte();
            if (nextByte == -1)
            {
                throw new Exception("Stream closed.");
            }
            if (nextByte != 0xE7)
            {
                throw new Exception("Was expecting fourth magic byte.");
            }
            message.WriteByte((Byte)nextByte);

            // Read the Command.
            for (int i = 0; i < 12; i++)
            {
                nextByte = stream.ReadByte();
                if (nextByte == -1)
                {
                    throw new Exception("Stream closed.");
                }
                message.WriteByte((Byte)nextByte);
            }

            // Read the Length and CheckSum.
            var reader = new BinaryReader(stream);
            var length = reader.ReadUInt32();
            message.Write(BitConverter.GetBytes(length), 0, 4);
            var checkSum = reader.ReadUInt32();
            message.Write(BitConverter.GetBytes(checkSum), 0, 4);

            // Read the Payload.
            for (int i = 0; i < length; i++)
            {
                nextByte = stream.ReadByte();
                if (nextByte == -1)
                {
                    throw new Exception("Stream closed.");
                }
                message.WriteByte((Byte)nextByte);
            }

            return new MessagePayload(message.ToArray());
        }
    }
}
