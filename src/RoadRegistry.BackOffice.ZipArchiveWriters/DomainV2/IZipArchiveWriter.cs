namespace RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2;

using System.IO.Compression;
using Extracts;

public interface IZipArchiveWriter
{
    Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken);
}
