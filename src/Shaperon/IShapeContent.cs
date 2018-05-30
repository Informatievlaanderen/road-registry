using System.IO;

namespace Shaperon
{
    public interface IShapeContent
    {
        ShapeType ShapeType { get; }

        WordLength ContentLength { get; }

        void Write(BinaryWriter writer);

        ShapeRecord RecordAs(RecordNumber number);
    }
}
