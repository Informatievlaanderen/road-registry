namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Editor.Projections;
using Editor.Schema;
using Framework;
using Hosts;
using Infrastructure;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using RoadRegistry.BackOffice.Core;
using System.Diagnostics;
using BackOffice.Extracts.Dbase.RoadSegments;
using Hosts.Infrastructure.Extensions;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;

public sealed class CorrectRoadSegmentVersionsSqsLambdaRequestHandler : SqsLambdaHandler<CorrectRoadSegmentVersionsSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly DistributedStreamStoreLock _distributedStreamStoreLock;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;

    public CorrectRoadSegmentVersionsSqsLambdaRequestHandler(
        SqsLambdaHandlerOptions options,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
        DistributedStreamStoreLockOptions distributedStreamStoreLockOptions,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        ILogger<CorrectRoadSegmentVersionsSqsLambdaRequestHandler> logger)
        : base(
            options,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _commandQueue = commandQueue;
        _distributedStreamStoreLock = new DistributedStreamStoreLock(distributedStreamStoreLockOptions, RoadNetworks.Stream, Logger);
        _editorContextFactory = editorContextFactory;
        _manager = manager;
    }

    protected override async Task<ETagResponse> InnerHandleAsync(CorrectRoadSegmentVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        await _distributedStreamStoreLock.RetryRunUntilLockAcquiredAsync(async () =>
        {
            await _commandQueue.DispatchChangeRoadNetwork(IdempotentCommandHandler, request, "Corrigeer wegsegmenten versies", async translatedChanges =>
            {
                var roadSegmentIdsWithGeometryVersionZero = await GetRoadSegmentIdsWithInvalidVersions(cancellationToken);
                
                if (roadSegmentIdsWithGeometryVersionZero.Any())
                {
                    var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

                    var roadSegments = network.FindRoadSegments(roadSegmentIdsWithGeometryVersionZero.Select(x => new RoadSegmentId(x)));

                    var recordNumber = RecordNumber.Initial;

                    foreach (var roadSegment in roadSegments)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        translatedChanges = translatedChanges.AppendChange(new ModifyRoadSegment(
                            recordNumber,
                            roadSegment.Id,
                            roadSegment.Start,
                            roadSegment.End,
                            roadSegment.AttributeHash.OrganizationId,
                            roadSegment.AttributeHash.GeometryDrawMethod,
                            roadSegment.AttributeHash.Morphology,
                            roadSegment.AttributeHash.Status,
                            roadSegment.AttributeHash.Category,
                            roadSegment.AttributeHash.AccessRestriction,
                            roadSegment.AttributeHash.LeftStreetNameId,
                            roadSegment.AttributeHash.RightStreetNameId
                        ).WithGeometry(roadSegment.Geometry));

                        recordNumber = recordNumber.Next();
                    }
                }

                return translatedChanges;
            }, cancellationToken);
        }, cancellationToken);

        return new ETagResponse(string.Empty, string.Empty);
    }

    private async Task<List<int>> GetRoadSegmentIdsWithInvalidVersions(CancellationToken cancellationToken)
    {
        await using var context = _editorContextFactory();

        var roadSegmentIds = new List<int>();
        const int pageSize = 5000;
        var pageIndex = 0;

        while (await FillInvalidRoadSegmentIds(context, pageIndex++, pageSize, roadSegmentIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return roadSegmentIds;
    }

    private async Task<bool> FillInvalidRoadSegmentIds(EditorContext context, int pageIndex, int pageSize, List<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        Logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);
        var roadSegments = await context.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        Logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        Logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments
            .Select(x => new RoadSegmentDbaseRecord().FromBytes(x.DbaseRecord, _manager, WellKnownEncodings.WindowsAnsi))
            .Where(x => x.WS_GIDN.Value.EndsWith("_0") || x.WS_UIDN.Value.EndsWith("_0"))
            .Select(x => x.WS_OIDN.Value));
        Logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }

    protected override Task ValidateIfMatchHeaderValue(CorrectRoadSegmentVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
