using System;
using System.IO;

namespace Shaperon.IO
{
    public class ShapeRecord
    {
        //Rationale: 100 byte file header means first record appears at offset 50 (16-bit word) of the mainfile.
        public static readonly WordOffset InitialOffset = new WordOffset(50);

        public ShapeRecord(ShapeRecordHeader header, IShape shape)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public ShapeRecordHeader Header { get; }
        public IShape Shape { get; }

        //public WordLength RecordLength { get; }

        public static ShapeRecord Create(RecordNumber recordNumber, IShape shape)
        {
            return new ShapeRecord(new ShapeRecordHeader(recordNumber, shape.ContentLength), shape);
        }

        public static ShapeRecord Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var header = ShapeRecordHeader.Read(reader);
            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeFileException("The Shape Type field does not contain a known type of shape.");
            var shape = NullShape.Instance;
            switch((ShapeType)typeOfShape)
            {
                case ShapeType.NullShape:
                    break;
                case ShapeType.Point:
                    shape = PointShape.Read(reader, header);
                    break;
                case ShapeType.PolyLineM:
                    shape = PolyLineMShape.Read(reader, header);
                    break;
                default:
                    throw new ShapeFileException($"The Shape Type {typeOfShape} is currently not suppported.");
            }
            return new ShapeRecord(header, shape);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            Header.Write(writer);
            Shape.Write(writer);
        }

        public ShapeIndexRecord AtOffset(WordOffset offset)
        {
            return new ShapeIndexRecord(offset, Header.ContentLength);
        }
    }
}