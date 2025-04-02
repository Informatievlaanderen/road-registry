namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost.V1;

using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Microsoft.IO;

internal static class ShapeContentFactory
{
    public static ShapeContent FromBytes(byte[] data, RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        using (var input = manager.GetStream(Guid.NewGuid(), nameof(ZipArchiveWriters), data, 0, data.Length))
        using (var reader = new BinaryReader(input, encoding))
        {
            return ShapeContent.Read(reader);
        }
    }
}
