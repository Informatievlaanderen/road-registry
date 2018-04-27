using System;
using System.IO;

namespace Shaperon.IO
{
    public class ShapeRecordHeader
    {
        public ShapeRecordHeader(RecordNumber recordNumber, WordLength contentLength)
        {
            RecordNumber = recordNumber;
            ContentLength = contentLength;
        }

        public RecordNumber RecordNumber { get; }
        public WordLength ContentLength { get; }

        public static ShapeRecordHeader Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            var recordNumber = reader.ReadInt32BigEndian();
            var contentLength = reader.ReadInt32BigEndian();
            return new ShapeRecordHeader(new RecordNumber(recordNumber), new WordLength(contentLength));
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32BigEndian(RecordNumber.ToInt32());
            writer.WriteInt32BigEndian(ContentLength);
        }
    }
}