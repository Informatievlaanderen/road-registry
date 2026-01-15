namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using System.Data;
using System.IO.Compression;
using System.Text;
using Marten;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.ZipArchiveWriters;
using RoadRegistry.Infrastructure.MartenDb;
using ScopedRoadNetwork;

public class RoadNetworkExtractArchiveAssemblerForDomainV2 : IRoadNetworkExtractArchiveAssembler
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly IZipArchiveWriterFactory _writerFactory;
    private readonly IDocumentStore _store;
    private readonly IRoadNetworkRepository _roadNetworkRepository;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly ILogger _logger;

    public RoadNetworkExtractArchiveAssemblerForDomainV2(
        RecyclableMemoryStreamManager manager,
        IZipArchiveWriterFactory writerFactory,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        Func<EditorContext> editorContextFactory,
        ILoggerFactory loggerFactory)
    {
        _manager = manager.ThrowIfNull();
        _writerFactory = writerFactory.ThrowIfNull();
        _store = store.ThrowIfNull();
        _roadNetworkRepository = roadNetworkRepository.ThrowIfNull();
        _editorContextFactory = editorContextFactory.ThrowIfNull();
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _store.WaitForNonStaleProjection(WellKnownProjectionStateNames.ExtractsRoadNetworkChangesProjection, _logger, cancellationToken);

        await using var session = _store.LightweightSession(IsolationLevel.Snapshot);

        await using var editorContext = _editorContextFactory();

        var stream = _manager.GetStream();
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
        var writer = _writerFactory.Create(request.ZipArchiveWriterVersion);
        await writer.WriteAsync(archive, request, new ZipArchiveDataProvider(session, _roadNetworkRepository, editorContext), cancellationToken);

        return stream;
    }
}
