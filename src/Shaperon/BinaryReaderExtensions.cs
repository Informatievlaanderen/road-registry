using System;
using System.IO;

namespace Shaperon
{
    internal static class BinaryReaderExtensions
    {
        public static string ReadRightPaddedString(this BinaryReader reader, int length, char padding)
        {
            var characters = reader.ReadChars(length);
            var index = characters.Length - 1;
            while(index >= 0 && characters[index].Equals(padding))
            {
                index--;
            }
            return index < 0 ? string.Empty : new string(characters, 0, index + 1);
        }

        public static string ReadLeftPaddedString(this BinaryReader reader, int length, char padding)
        {
            var characters = reader.ReadChars(length);
            var index = 0;
            while(index < characters.Length && characters[index].Equals(padding))
            {
                index++;
            }
            return index == characters.Length ? string.Empty : new string(characters, index, characters.Length - index);
        }

        public static void WriteRightPaddedString(this BinaryWriter writer, string value, int length, char padding)
        {
            if(value != null && value.Length > length)
            {
                throw new ArgumentException("The value length is longer than the writable length.");
            }
            if(string.IsNullOrEmpty(value))
            {
                writer.Write(new string(padding, length).ToCharArray());
            }
            else
            {
                writer.Write(value.Length != length
                    ? string.Concat(value, new string(padding, length - value.Length)).ToCharArray()
                    : value.ToCharArray());
            }
        }

        public static void WriteLeftPaddedString(this BinaryWriter writer, string value, int length, char padding)
        {
            if(value != null && value.Length > length)
            {
                throw new ArgumentException("The value length is longer than the writable length.");
            }
            if(string.IsNullOrEmpty(value))
            {
                writer.Write(new string(padding, length).ToCharArray());
            }
            else
            {
                writer.Write(value.Length != length
                    ? string.Concat(new string(padding, length - value.Length), value).ToCharArray()
                    : value.ToCharArray());
            }
        }
    }
}
