namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RequestExtract;

using System;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;
using RoadRegistry.Editor.Schema;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts;
using SqlStreamStore;

public class RoadNetworkExtractArchiveAssemblerForDomainV1 : IRoadNetworkExtractArchiveAssembler
{
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly Func<EditorContext> _contextFactory;
    private readonly IZipArchiveWriterFactory _writerFactory;
    private readonly IStreamStore _streamStore;
    private readonly ILogger _logger;

    public RoadNetworkExtractArchiveAssemblerForDomainV1(
        RecyclableMemoryStreamManager manager,
        Func<EditorContext> contextFactory,
        IZipArchiveWriterFactory writerFactory,
        IStreamStore streamStore,
        ILoggerFactory loggerFactory)
    {
        _manager = manager.ThrowIfNull();
        _contextFactory = contextFactory.ThrowIfNull();
        _writerFactory = writerFactory.ThrowIfNull();
        _streamStore = streamStore;
        _logger = loggerFactory.CreateLogger(GetType());
    }

    public async Task<MemoryStream> AssembleArchive(RoadNetworkExtractAssemblyRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await using var context = _contextFactory();

        await context.WaitForProjectionsToBeAtStoreHeadPosition(_streamStore, [
            WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
            WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost,
            WellKnownProjectionStateNames.RoadRegistryEditorExtractRequestProjectionHost
        ], _logger, cancellationToken);

        var stream = _manager.GetStream();
        await using var tr = await context.Database.BeginTransactionAsync(IsolationLevel.Snapshot, cancellationToken);

        using var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8);
        var writer = _writerFactory.Create(request.ZipArchiveWriterVersion);
        await writer.WriteAsync(archive, request, new ZipArchiveDataProvider(context), cancellationToken);

        return stream;
    }
}
