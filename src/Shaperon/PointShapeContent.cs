using System;
using System.IO;
using Wkx;

namespace Shaperon
{
    public class PointShapeContent : ShapeContent
    {
        public PointShapeContent(Point shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ShapeType = ShapeType.Point;
            ContentLength = new WordLength(10);
        }

        public Point Shape { get; }

        internal static ShapeContent ReadFromRecord(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var x = reader.ReadDoubleLittleEndian();
            var y = reader.ReadDoubleLittleEndian();
            return new PointShapeContent(new Point(x, y));
        }

        public static ShapeContent Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeRecordContentException("The Shape Type field does not contain a known type of shape.");
            if(((ShapeType)typeOfShape) == ShapeType.NullShape)
                return NullShapeContent.Instance;
            if(((ShapeType)typeOfShape) != ShapeType.Point)
                throw new ShapeRecordContentException("The Shape Type field does not indicate a Point shape.");

            return new PointShapeContent(
                new Point(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()));
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type
            writer.WriteDoubleLittleEndian(Shape.X.Value); // X Coordinate
            writer.WriteDoubleLittleEndian(Shape.Y.Value); // Y Coordinate
        }

        public override string ToString() => $"Point[{Shape.X.Value};{Shape.Y.Value}]";
    }
}
