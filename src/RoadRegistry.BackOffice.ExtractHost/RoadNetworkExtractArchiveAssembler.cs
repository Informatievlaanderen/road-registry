namespace RoadRegistry.BackOffice.ExtractHost;

using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Editor.Schema;
using Extracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using ZipArchiveWriters.ExtractHost;

public class RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
{
    private readonly Func<EditorContext> _contextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IZipArchiveWriter _writer;

    public RoadNetworkExtractArchiveAssembler(
        RecyclableMemoryStreamManager manager,
        Func<EditorContext> contextFactory,
        IZipArchiveWriter writer)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
    }

    public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var stream = _manager.GetStream();
        await using var context = _contextFactory();
        
        await using var tr = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
        await _writer.WriteAsync(archive, request, new ZipArchiveDataProvider(context), cancellationToken);

        return stream;
    }
}
