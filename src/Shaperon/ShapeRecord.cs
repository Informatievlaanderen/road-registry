using System;
using System.IO;

namespace Shaperon
{
    public class ShapeRecord
    {
        //Rationale: 100 byte file header means first record appears at offset 50 (16-bit word) of the mainfile.
        public static readonly WordOffset InitialOffset = new WordOffset(50);

        public ShapeRecord(ShapeRecordHeader header, IShapeContent content)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public ShapeRecordHeader Header { get; }
        public IShapeContent Content { get; }

        public static ShapeRecord Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var header = ShapeRecordHeader.Read(reader);
            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeRecordContentException("The Shape Type field does not contain a known type of shape.");
            var content = NullShapeContent.Instance;
            switch((ShapeType)typeOfShape)
            {
                case ShapeType.NullShape:
                    break;
                case ShapeType.Point:
                    content = PointShapeContent.ReadFromRecord(reader, header);
                    break;
                case ShapeType.PolyLineM:
                    content = PolyLineMShapeContent.ReadFromRecord(reader, header);
                    break;
                default:
                    throw new ShapeRecordContentException($"The Shape Type {typeOfShape} is currently not suppported.");
            }
            return new ShapeRecord(header, content);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Header.Write(writer);
            Content.Write(writer);
        }

        public byte[] ToBytes()
        {
            using(var output = new MemoryStream())
            {
                using(var writer = new BinaryWriter(output))
                {
                    Write(writer);
                    writer.Flush();
                }
                return output.ToArray();
            }
        }

        public ShapeIndexRecord AtOffset(WordOffset offset)
        {
            return new ShapeIndexRecord(offset, Header.ContentLength);
        }
    }
}
