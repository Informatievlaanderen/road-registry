using System;
using System.IO;

namespace Shaperon
{
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
