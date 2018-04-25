using System;
using System.IO;
using Shaperon.IO;

namespace Shaperon
{
    public class PointShape : IShape
    {
        public PointShape(double x, double y)
        {
            X = x;
            Y = y;
            ContentLength = new WordLength(10);
        }

        public ShapeType ShapeType => ShapeType.Point;

        public double X { get; }
        public double Y { get; }

        public WordLength ContentLength { get; }

        public static IShape Read(BinaryReader reader, ShapeFileRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var x = reader.ReadDoubleLittleEndian();
            var y = reader.ReadDoubleLittleEndian();
            return new PointShape(x, y);
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type
            writer.WriteDoubleLittleEndian(X); // X Coordinate
            writer.WriteDoubleLittleEndian(Y); // Y Coordinate
        }

        public override string ToString() => $"Point[{X};{Y}]";
    }
}