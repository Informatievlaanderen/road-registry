using System;
using System.IO;

namespace Shaperon.IO
{
    public class ShapeFileRecordHeader
    {
        public ShapeFileRecordHeader(RecordNumber recordNumber, WordLength contentWordLength)
        {
            RecordNumber = recordNumber;
            ContentWordLength = contentWordLength;
        }

        public RecordNumber RecordNumber { get; }
        public WordLength ContentWordLength { get; }

        public static ShapeFileRecordHeader Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }
            var recordNumber = reader.ReadInt32BigEndian();
            var contentLength = reader.ReadInt32BigEndian();
            return new ShapeFileRecordHeader(new RecordNumber(recordNumber), new WordLength(contentLength));
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32BigEndian(RecordNumber.ToInt32());
            writer.WriteInt32BigEndian(ContentWordLength);
        }
    }
}