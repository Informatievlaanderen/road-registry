namespace Shaperon
{
    using System;
    using System.IO;
    using System.Text;

    public class NullShapeContent : ShapeContent
    {
        public static readonly ShapeContent Instance = new NullShapeContent();

        private NullShapeContent()
        {
            ShapeType = ShapeType.NullShape;
            ContentLength = new WordLength(2);
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
            if(((ShapeType)typeOfShape) != ShapeType.NullShape)
                throw new ShapeRecordContentException("The Shape Type field does not indicate a Null shape.");

            return Instance;
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
        }
    }
}
