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
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using ZipArchiveWriters.ExtractHost;

public class RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
{
    private readonly Func<EditorContext> _contextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IZipArchiveWriterFactory _writerFactory;

    public RoadNetworkExtractArchiveAssembler(
        RecyclableMemoryStreamManager manager,
        Func<EditorContext> contextFactory,
        IZipArchiveWriterFactory writerFactory)
    {
        _manager = manager.ThrowIfNull();
        _contextFactory = contextFactory.ThrowIfNull();
        _writerFactory = writerFactory.ThrowIfNull();
    }

    public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stream = _manager.GetStream();
        await using var context = _contextFactory();

        await using var tr = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
        var writer = _writerFactory.Create(request.ZipArchiveWriterVersion);
        await writer.WriteAsync(archive, request, new ZipArchiveDataProvider(context), cancellationToken);

        return stream;
    }
}
