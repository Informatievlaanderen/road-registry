namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;
using BackOffice.Extracts.Dbase.RoadNodes;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Editor.Projections;
using Editor.Schema;
using Hosts;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using RoadRegistry.BackOffice.Core;
using System.Diagnostics;
using TicketingService.Abstractions;
using ModifyRoadNode = BackOffice.Uploads.ModifyRoadNode;

public sealed class CorrectRoadNodeVersionsSqsLambdaRequestHandler : SqsLambdaHandler<CorrectRoadNodeVersionsSqsLambdaRequest>
{
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly IChangeRoadNetworkDispatcher _changeRoadNetworkDispatcher;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;

    public CorrectRoadNodeVersionsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IChangeRoadNetworkDispatcher changeRoadNetworkDispatcher,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadNodeVersionsSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
        _changeRoadNetworkDispatcher = changeRoadNetworkDispatcher;
        _editorContextFactory = editorContextFactory;
        _manager = manager;
        _fileEncoding = fileEncoding;
    }

    protected override async Task<ETagResponse> InnerHandleAsync(CorrectRoadNodeVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _changeRoadNetworkDispatcher.DispatchAsync(request, "Corrigeer wegknoop versies", async translatedChanges =>
            {
                var roadNodeIdsWithGeometryVersionZero = await GetRoadNodeIdsWithInvalidVersions(cancellationToken);
                
                if (roadNodeIdsWithGeometryVersionZero.Any())
                {
                    var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);
                    
                    var roadNodes = network.FindRoadNodes(roadNodeIdsWithGeometryVersionZero.Select(x => new RoadNodeId(x)));

                    var recordNumber = RecordNumber.Initial;

                    foreach (var roadNode in roadNodes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        translatedChanges = translatedChanges.AppendChange(new ModifyRoadNode(
                            recordNumber,
                            roadNode.Id,
                            roadNode.Type
                        ).WithGeometry(roadNode.Geometry));

                        recordNumber = recordNumber.Next();
                    }
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        return new ETagResponse(string.Empty, string.Empty);
    }

    private async Task<List<int>> GetRoadNodeIdsWithInvalidVersions(CancellationToken cancellationToken)
    {
        await using var context = _editorContextFactory();

        var roadNodeIds = new List<int>();
        const int pageSize = 5000;
        var pageIndex = 0;

        while (await FillInvalidRoadNodeIds(context, pageIndex++, pageSize, roadNodeIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return roadNodeIds;
    }

    private async Task<bool> FillInvalidRoadNodeIds(EditorContext context, int pageIndex, int pageSize, List<int> roadNodeIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        Logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadNodes), pageIndex, pageSize);
        var roadNodes = await context.RoadNodes
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        Logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadNodes), sw.ElapsedMilliseconds);

        sw.Restart();

        Logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadNodes), pageIndex, pageSize);

        roadNodeIds.AddRange(roadNodes
            .Select(x => new RoadNodeDbaseRecord().FromBytes(x.DbaseRecord, _manager, _fileEncoding))
            .Where(x => x.WK_UIDN.Value.EndsWith("_0"))
            .Select(x => x.WK_OIDN.Value));
        Logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadNodes), sw.ElapsedMilliseconds);

        return roadNodes.Any();
    }

    protected override Task ValidateIfMatchHeaderValue(CorrectRoadNodeVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
