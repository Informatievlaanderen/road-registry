namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

public interface IZipArchiveWriter<in TContext> where TContext : DbContext
{
    Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken);
}
