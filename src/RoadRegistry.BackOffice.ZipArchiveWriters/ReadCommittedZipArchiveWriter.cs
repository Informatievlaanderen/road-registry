namespace RoadRegistry.BackOffice.ZipArchiveWriters;

using System.Data;
using System.IO.Compression;
using Microsoft.EntityFrameworkCore;

public class ReadCommittedZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    public ReadCommittedZipArchiveWriter(IZipArchiveWriter<TContext> writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    private readonly IZipArchiveWriter<TContext> _writer;

    public async Task WriteAsync(ZipArchive archive, TContext context, CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (context == null) throw new ArgumentNullException(nameof(context));

        await using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
        {
            await _writer.WriteAsync(archive, context, cancellationToken);
        }
    }
}
