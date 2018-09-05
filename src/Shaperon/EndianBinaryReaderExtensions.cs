namespace Shaperon
{
    using System.Linq;
    using System;
    using System.IO;

    public static class EndianBinaryReaderExtensions
    {
        private const string NoDescription = "<unknown read value>";

        public static int ReadInt32BigEndian(this BinaryReader reader, string description = NoDescription)
        {
            var bytes = reader.ReadBytes(4);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        public static double ReadDoubleBigEndian(this BinaryReader reader, string description = NoDescription)
        {
            var bytes = reader.ReadBytes(8);
            if(BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }

        public static int ReadInt32LittleEndian(this BinaryReader reader, string description = NoDescription)
        {
            var bytes = reader.ReadBytes(4);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes, 0);
        }

        public static double ReadDoubleLittleEndian(this BinaryReader reader, string description = NoDescription)
        {
            var bytes = reader.ReadBytes(8);
            if(!BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToDouble(bytes, 0);
        }
    }
}
