namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers.Extracts;

using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Editor.Schema;
using Marten;
using Microsoft.IO;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.ZipArchiveWriters.DomainV2;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork;

public class DomainV2RoadNetworkExtractArchiveAssembler : IRoadNetworkExtractArchiveAssembler
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IZipArchiveWriterFactory _writerFactory;
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly Func<EditorContext> _editorContextFactory;

    public DomainV2RoadNetworkExtractArchiveAssembler(
        RecyclableMemoryStreamManager manager,
        IZipArchiveWriterFactory writerFactory,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        Func<EditorContext> editorContextFactory)
    {
        _manager = manager.ThrowIfNull();
        _writerFactory = writerFactory.ThrowIfNull();
        _store = store.ThrowIfNull();
        _roadNetworkRepository = roadNetworkRepository.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
    }

    public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var stream = _manager.GetStream();

        //TODO-pr wacht op extract projectie

        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);
        await using var editorContext = _editorContextFactory();

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
        var writer = _writerFactory.Create(request.ZipArchiveWriterVersion);
        await writer.WriteAsync(archive, request, new ZipArchiveDataProvider(session, _roadNetworkRepository, editorContext), cancellationToken);

        return stream;
    }
}
