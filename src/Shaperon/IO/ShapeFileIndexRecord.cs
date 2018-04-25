using System;
using System.IO;

namespace Shaperon.IO
{
    public class ShapeIndexFileRecord
    {
        public ShapeIndexFileRecord(Offset offset, WordLength contentLength)
        {
            Offset = offset;
            ContentLength = contentLength;
        }

        public Offset Offset { get; }
        public WordLength ContentLength { get; }

        public static ShapeIndexFileRecord Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var offset = new Offset(reader.ReadInt32BigEndian());
            var contentLength = new WordLength(reader.ReadInt32BigEndian());
            return new ShapeIndexFileRecord(offset, contentLength);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32BigEndian(Offset.ToInt32());
            writer.WriteInt32BigEndian(ContentLength.ToInt32());
        }
    }
}