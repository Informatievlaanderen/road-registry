namespace RoadRegistry.Editor.Schema.Extensions;

using System.IO;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;

public static class ShapeContentExtensions
{
    public static T FromBytes<T>(byte[] bytes, RecyclableMemoryStreamManager manager, Encoding encoding)
        where T : ShapeContent
    {
        using (var input = manager.GetStream(bytes))
        using (var reader = new BinaryReader(input, encoding))
        {
            return (T)ShapeContent.Read(reader);
        }
    }

    public static byte[] ToBytes(this ShapeContent content, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        using (var output = manager.GetStream())
        using (var writer = new BinaryWriter(output, encoding))
        {
            content.Write(writer);
            writer.Flush();
            return output.ToArray();
        }
    }

    public static T Clone<T>(this T content, RecyclableMemoryStreamManager manager, Encoding encoding)
        where T: ShapeContent
    {
        var bytes = ToBytes(content, manager, encoding);
        var clone = FromBytes<T>(bytes, manager, encoding);
        return clone;
    }
}
