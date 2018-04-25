using System.IO;

namespace Shaperon
{
    public interface IShape
    {
        ShapeType ShapeType { get; }

        WordLength ContentLength { get; }

        void Write(BinaryWriter writer);
    }
}