using System;
using System.IO;

namespace Shaperon.IO
{
    public static class BinaryReaderExtensions
    {
        public static string ReadRightPaddedString(this BinaryReader reader, int length, char paddingCharacter)
        {
            var characters = reader.ReadChars(length);
            var index = Array.FindIndex(
                characters, 
                character => character.Equals(paddingCharacter));
            return (index < 0) ? string.Empty : new string(characters, 0, index + 1);
        }
    }
}