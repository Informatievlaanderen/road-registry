using System;
using System.IO;
using Shaperon.IO;

namespace Shaperon
{
    public class PointShapeContent : IShapeContent
    {
        public PointShapeContent(Point point)
        {
            Point = point;
            ContentLength = new WordLength(10);
        }

        public ShapeType ShapeType => ShapeType.Point;

        public Point Point { get; }

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
            writer.WriteDoubleLittleEndian(Point.X); // X Coordinate
            writer.WriteDoubleLittleEndian(Point.Y); // Y Coordinate
        }

        public override string ToString() => $"Point[{Point.X};{Point.Y}]";
    }
}