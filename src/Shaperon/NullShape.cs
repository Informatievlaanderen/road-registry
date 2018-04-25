using System;
using System.IO;
using Shaperon.IO;

namespace Shaperon
{
    public class NullShape : IShape
    {
        public static readonly IShape Instance = new NullShape();

        private NullShape() 
        { 
            ContentWordLength = new WordLength(2);
        }

        public ShapeType ShapeType => ShapeType.NullShape;

        public WordLength ContentWordLength { get; }

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