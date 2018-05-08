using System;
using System.IO;

namespace Shaperon.IO
{
    public static class BinaryReaderExtensions
    {
        public static string ReadRightPaddedString(this BinaryReader reader, int length, char padding)
        {
            var characters = reader.ReadChars(length);
            var index = characters.Length - 1;
            while(index >= 0 && characters[index].Equals(padding)) 
            { 
                index--; 
            }
            if (index < 0)
            {
                return string.Empty;
            }
            return new string(characters, 0, index + 1);
        }

        public static string ReadLeftPaddedString(this BinaryReader reader, int length, char padding)
        {
            var characters = reader.ReadChars(length);
            var index = 0;
            while(index < characters.Length && characters[index].Equals(padding)) 
            { 
                index++; 
            }
            if(index == characters.Length)
            {
                return string.Empty;
            }
            return new string(characters, index, characters.Length - index);
        }

        public static void WriteRightPaddedString(this BinaryWriter writer, string value, int length, char padding)
        {
            if(value != null && value.Length > length) 
            {
                throw new ArgumentException("The value length is longer than the writable length.");
            }
            if(value == null || value.Length == 0)
            {
                writer.Write(new string(padding, length).ToCharArray());
            }
            else
            {
                if(value.Length != length)
                {
                    writer.Write(String.Concat(value, new string(padding, length - value.Length)).ToCharArray());
                } else {
                    writer.Write(value.ToCharArray());
                }
            }
        }

        public static void WriteLeftPaddedString(this BinaryWriter writer, string value, int length, char padding)
        {
            if(value != null && value.Length > length) 
            {
                throw new ArgumentException("The value length is longer than the writable length.");
            }
            if(value == null || value.Length == 0)
            {
                writer.Write(new string(padding, length).ToCharArray());
            }
            else
            {
                if(value.Length != length)
                {
                    writer.Write(String.Concat(new string(padding, length - value.Length), value).ToCharArray());
                } else {
                    writer.Write(value.ToCharArray());
                }
            }
        }
    }
}