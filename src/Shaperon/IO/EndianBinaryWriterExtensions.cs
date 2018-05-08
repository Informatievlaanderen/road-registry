using System;
using System.IO;

namespace Shaperon.IO
{
    public static class EndianBinaryWriterExtensions
    {
        public static void WriteInt32BigEndian(this BinaryWriter writer, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) 
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteDoubleBigEndian(this BinaryWriter writer, double value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(BitConverter.IsLittleEndian) 
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteInt32LittleEndian(this BinaryWriter writer, int value)
        {
            var bytes = BitConverter.GetBytes(value);
            if(!BitConverter.IsLittleEndian) 
            {
                Array.Reverse(bytes);
            }
            writer.Write(bytes);
        }

        public static void WriteDoubleLittleEndian(this BinaryWriter writer, double value)
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
