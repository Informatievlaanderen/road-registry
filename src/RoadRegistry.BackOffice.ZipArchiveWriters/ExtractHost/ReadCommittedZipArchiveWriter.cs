namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.Data;
using System.IO.Compression;
using Extracts;
using Microsoft.EntityFrameworkCore;

public class ReadCommittedZipArchiveWriter<TContext> : IZipArchiveWriter<TContext> where TContext : DbContext
{
    private readonly IZipArchiveWriter<TContext> _writer;

    public ReadCommittedZipArchiveWriter(IZipArchiveWriter<TContext> writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task WriteAsync(ZipArchive archive, RoadNetworkExtractAssemblyRequest request, TContext context,
        CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (context == null) throw new ArgumentNullException(nameof(context));

        await using (await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken))
        {
            await _writer.WriteAsync(archive, request, context, cancellationToken);
        }
    }
}
