namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.IO.Compression;
using Extracts;
using Microsoft.EntityFrameworkCore;

public interface IZipArchiveWriter<in TContext> where TContext : DbContext
{
    Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
        CancellationToken cancellationToken);
}
