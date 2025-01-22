namespace RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

using System.Data;
using System.IO.Compression;
using Extracts;

public class ReadCommittedZipArchiveWriter : IZipArchiveWriter
{
    private readonly IZipArchiveWriter _writer;

    public ReadCommittedZipArchiveWriter(IZipArchiveWriter writer)
    {
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task WriteAsync(
        ZipArchive archive,
        RoadNetworkExtractAssemblyRequest request,
        IZipArchiveDataProvider zipArchiveDataProvider,
        CancellationToken cancellationToken)
    {
        if (archive == null) throw new ArgumentNullException(nameof(archive));
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (zipArchiveDataProvider == null) throw new ArgumentNullException(nameof(zipArchiveDataProvider));

        // todo-rik niet 100% overtuigd van deze manier
        await using (await zipArchiveDataProvider.BeginTransaction(IsolationLevel.ReadCommitted, cancellationToken))
        {
            await _writer.WriteAsync(archive, request, zipArchiveDataProvider, cancellationToken);
        }
    }
}
