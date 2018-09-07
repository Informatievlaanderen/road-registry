namespace Shaperon
{
    using System;
    using System.IO;
    using System.Text;

    public class PointShapeContent : ShapeContent
    {
        public PointShapeContent(PointM shape)
        {
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
            ShapeType = ShapeType.Point;
            ContentLength = new WordLength(10);
        }

        public PointM Shape { get; }

        internal static ShapeContent ReadFromRecord(BinaryReader reader, ShapeRecordHeader header)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var x = reader.ReadDoubleLittleEndian();
            var y = reader.ReadDoubleLittleEndian();
            return new PointShapeContent(new PointM(x, y));
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
                new PointM(
                    reader.ReadDoubleLittleEndian(),
                    reader.ReadDoubleLittleEndian()));
        }

        public static ShapeContent FromBytes(byte[] bytes)
        {
            using(var input = new MemoryStream(bytes))
            using(var reader = new BinaryReader(input))
            {
                return Read(reader);
            }
        }

        public static ShapeContent FromBytes(byte[] bytes, Encoding encoding)
        {
            using(var input = new MemoryStream(bytes))
            using(var reader = new BinaryReader(input, encoding))
            {
                return Read(reader);
            }
        }

        public override void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type
            writer.WriteDoubleLittleEndian(Shape.X); // X Coordinate
            writer.WriteDoubleLittleEndian(Shape.Y); // Y Coordinate
        }
    }
}
