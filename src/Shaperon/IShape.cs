using System.IO;

namespace Shaperon
{
    public interface IShape
    {
        ShapeType ShapeType { get; }

        WordLength ContentWordLength { get; }

        void Write(BinaryWriter writer);
    }
}