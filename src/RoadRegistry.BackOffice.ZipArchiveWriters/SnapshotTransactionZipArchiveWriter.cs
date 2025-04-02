namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.Data;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

public class SnapshotTransactionZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly IZipArchiveWriter<TContext> _writer;

    public SnapshotTransactionZipArchiveWriter(IZipArchiveWriter<TContext> writer)
    {
        _writer = writer.ThrowIfNull();
    }

    public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(archive);
        ArgumentNullException.ThrowIfNull(context);

        await using (await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken))
        {
            await _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
