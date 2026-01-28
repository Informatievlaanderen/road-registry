namespace RoadRegistry.Extracts.ZipArchiveWriters;

using System.IO.Compression;
using RoadRegistry.Extracts;

public interface IZipArchiveWriter
{
    Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        ZipArchiveWriteContext context,
        CancellationToken cancellationToken);
}
