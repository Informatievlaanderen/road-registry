using System;
using System.IO;
using Shaperon.IO;

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