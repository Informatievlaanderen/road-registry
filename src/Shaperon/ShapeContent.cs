using System.IO;
using System.Text;

namespace Shaperon
{
    public abstract class ShapeContent
    {
        protected ShapeContent() {}

        public ShapeType ShapeType { get; protected set; }

        public WordLength ContentLength { get; protected set; }

        public abstract void Write(BinaryWriter writer);

        public ShapeRecord RecordAs(RecordNumber number)
        {
            return new ShapeRecord(new ShapeRecordHeader(number, ContentLength), this);
        }

        public byte[] ToBytes()
        {
            using(var output = new MemoryStream())
            using(var writer = new BinaryWriter(output))
            {
                Write(writer);
                writer.Flush();
                return output.ToArray();
            }
        }

        public byte[] ToBytes(Encoding encoding)
        {
            using(var output = new MemoryStream())
            using(var writer = new BinaryWriter(output, encoding))
            {
                Write(writer);
                writer.Flush();
                return output.ToArray();
            }
        }
    }
}
