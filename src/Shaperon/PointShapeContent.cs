using System;
using System.IO;
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

        internal static IShapeContent ReadFromRecord(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var x = reader.ReadDoubleLittleEndian();
            var y = reader.ReadDoubleLittleEndian();
            return new PointShapeContent(new Point(x, y));
        }

        public static IShapeContent Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeFileContentException("The Shape Type field does not contain a known type of shape.");
            if(((ShapeType)typeOfShape) != ShapeType.Point)
                throw new ShapeFileContentException("The Shape Type field does not indicate a Point shape.");

            return new PointShapeContent(
                new Point(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()));
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

        public override string ToString() => $"Point[{Shape.X.Value};{Shape.Y.Value}]";
    }
}
