namespace RoadRegistry.Extracts.ZipArchiveWriters;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;

internal static class DbaseRecordExtensions
{
    public static void FromBytes(this DbaseRecord record, byte[] data, RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        using (var input = manager.GetStream(Guid.NewGuid(), nameof(ZipArchiveWriters), data, 0, data.Length))
        using (var reader = new BinaryReader(input, encoding))
        {
            record.Read(reader);
        }
    }
    public static short ToDbaseShortValue(this bool value)
    {
        return value ? (short)1 : (short)0;
    }
}
