using System;
using System.IO;

namespace Shaperon
{
    public class NullShapeContent : IShapeContent
    {
        public static readonly IShapeContent Instance = new NullShapeContent();

        private NullShapeContent()
        {
            ContentLength = new WordLength(2);
        }

        public ShapeType ShapeType => ShapeType.NullShape;

        public WordLength ContentLength { get; }

        public static IShapeContent Read(BinaryReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var typeOfShape = reader.ReadInt32LittleEndian();
            if(!Enum.IsDefined(typeof(ShapeType), typeOfShape))
                throw new ShapeFileContentException("The Shape Type field does not contain a known type of shape.");
            if(((ShapeType)typeOfShape) != ShapeType.NullShape)
                throw new ShapeFileContentException("The Shape Type field does not indicate a Null shape.");

            return Instance;
        }

        public void Write(BinaryWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            writer.WriteInt32LittleEndian((int)ShapeType); // Shape Type
        }
    }
}
