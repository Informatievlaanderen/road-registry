namespace RoadRegistry.Product.Projections;

using System.IO;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;

public static class DbaseRecordExtensions
{
    public static T FromBytes<T>(this T record, byte[] bytes, RecyclableMemoryStreamManager manager, Encoding encoding)
        where T : DbaseRecord
    {
        using (var input = manager.GetStream(bytes))
        using (var reader = new BinaryReader(input, encoding))
        {
            record.Read(reader);
        }

        return record;
    }

    public static byte[] ToBytes(this DbaseRecord record, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        using (var output = manager.GetStream())
        using (var writer = new BinaryWriter(output, encoding))
        {
            record.Write(writer);
            writer.Flush();
            return output.ToArray();
        }
    }
}
