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

        public static void WritePaddedString(this BinaryWriter writer, string value, DbaseFieldWriteProperties properties)
        {
            if (value != null && value.Length > properties.Length)
            {
                throw new ArgumentException($"The value length for {properties.Name} is longer than the writable length.");
            }

            if (string.IsNullOrEmpty(value))
            {
                writer.Write(new string(properties.Padding, properties.Length).ToCharArray());
            }
            else
            {
                var padding = new string(properties.Padding, properties.Length - value.Length);
                writer.Write(
                    (
                        properties.Pad == DbaseFieldPadding.Left
                        ? string.Concat(padding, value)
                        : string.Concat(value, padding)
                    ).ToCharArray()
                );
            }
        }
    }
}
