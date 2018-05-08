using System;
using System.IO;
using Shaperon.IO;
using Wkx;

namespace Shaperon
{
    public class PointShapeContent : IShapeContent
    {
        public PointShapeContent(Point shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ContentLength = new WordLength(10);
        }

        public ShapeType ShapeType => ShapeType.Point;

        public Point Shape { get; }

        public WordLength ContentLength { get; }

        public static IShapeContent Read(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var x = reader.ReadDoubleLittleEndian();
            var y = reader.ReadDoubleLittleEndian();
            return new PointShapeContent(new Point(x, y));
        }

        public void Write(BinaryWriter writer)
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