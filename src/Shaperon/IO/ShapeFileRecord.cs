using System;
using System.IO;

namespace Shaperon.IO
{
    public class ShapeFileRecord
    {
        public ShapeFileRecord(ShapeFileRecordHeader header, IShape shape)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public ShapeFileRecordHeader Header { get; }
        public IShape Shape { get; }

        //public WordLength RecordLength { get; }

        public static ShapeFileRecord Create(RecordNumber recordNumber, IShape shape)
        {
            return new ShapeFileRecord(new ShapeFileRecordHeader(recordNumber, shape.ContentLength), shape);
        }

        public static ShapeFileRecord Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var header = ShapeFileRecordHeader.Read(reader);
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
            return new ShapeFileRecord(header, shape);
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

        public ShapeIndexFileRecord AtOffset(Offset offset)
        {
            return new ShapeIndexFileRecord(offset, Header.ContentLength);
        }
    }
}