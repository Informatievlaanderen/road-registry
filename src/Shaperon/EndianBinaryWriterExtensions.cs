namespace Shaperon
{
    using System;
    using System.IO;

    public static class EndianBinaryWriterExtensions
    {
        private const string NoDescription = "<unknown written value>";

        public static void WriteInt32BigEndian(this BinaryWriter writer, int value, string description = NoDescription)
        {
            var bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteDoubleBigEndian(this BinaryWriter writer, double value, string description = NoDescription)
        {
            var bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteInt32LittleEndian(this BinaryWriter writer, int value, string description = NoDescription)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteDoubleLittleEndian(this BinaryWriter writer, double value, string description = NoDescription)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }
    }
}
