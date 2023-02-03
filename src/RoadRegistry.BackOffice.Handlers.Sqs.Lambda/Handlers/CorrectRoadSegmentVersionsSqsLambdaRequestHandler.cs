namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Handlers;

using System.Diagnostics;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using Dbase.RoadSegments;
using Editor.Projections;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using Requests;
using TicketingService.Abstractions;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using Reason = Reason;

public sealed class CorrectRoadSegmentVersionsSqsLambdaRequestHandler : SqsLambdaHandler<CorrectRoadSegmentVersionsSqsLambdaRequest>
{
    private readonly IRoadNetworkCommandQueue _commandQueue;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;

    public CorrectRoadSegmentVersionsSqsLambdaRequestHandler(
        IConfiguration configuration,
        ICustomRetryPolicy retryPolicy,
        ITicketing ticketing,
        IIdempotentCommandHandler idempotentCommandHandler,
        IRoadRegistryContext roadRegistryContext,
        IRoadNetworkCommandQueue commandQueue,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        ILogger<CorrectRoadSegmentVersionsSqsLambdaRequestHandler> logger)
        : base(
            configuration,
            retryPolicy,
            ticketing,
            idempotentCommandHandler,
            roadRegistryContext,
            logger)
    {
        _commandQueue = commandQueue;
        _editorContextFactory = editorContextFactory;
        _manager = manager;
    }

    private async Task<List<int>> GetRoadSegmentIdsWithInvalidVersions()
    {
        await using var context = _editorContextFactory();

        var roadSegmentIds = new List<int>();
        const int pageSize = 1000;
        var pageIndex = 0;

        while (await FillInvalidRoadSegmentIds(context, pageIndex++, pageSize, roadSegmentIds))
        {
        }

        return roadSegmentIds;
    }

    private async Task<bool> FillInvalidRoadSegmentIds(EditorContext context, int pageIndex, int pageSize, List<int> roadSegmentIds)
    {
        var sw = Stopwatch.StartNew();

        Logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);
        var roadSegments = await context.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync();
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

    protected override async Task<ETagResponse> InnerHandleAsync(CorrectRoadSegmentVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        var command = await ToCommand(request, cancellationToken);

        var commandId = command.CreateCommandId();
        await _commandQueue.Write(new Command(command).WithMessageId(commandId), cancellationToken);

        try
        {
            await IdempotentCommandHandler.Dispatch(
                commandId,
                command,
                request.Metadata,
                cancellationToken);
        }
        catch (IdempotencyException)
        {
            // Idempotent: Do Nothing return last etag
        }

        return new ETagResponse(null, null);
    }

    private async Task<IHasCommandProvenance> ToCommand(CorrectRoadSegmentVersionsSqsLambdaRequest lambdaRequest, CancellationToken cancellationToken)
    {
        var roadSegmentIdsWithGeometryVersionZero = await GetRoadSegmentIdsWithInvalidVersions();
        
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(new OrganizationId(lambdaRequest.Provenance.Organisation.ToString()))
            .WithOperatorName(new OperatorName(lambdaRequest.Provenance.Operator))
            .WithReason(new Reason("Corrigeer wegsegmenten versies"));

        if (roadSegmentIdsWithGeometryVersionZero.Any())
        {
            var network = await RoadRegistryContext.RoadNetworks.Get(cancellationToken);

            var roadSegments = network.FindRoadSegments(roadSegmentIdsWithGeometryVersionZero.Select(x => new RoadSegmentId(x)));

            var recordNumber = RecordNumber.Initial;

            foreach (var roadSegment in roadSegments)
            {
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

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();
        var messageId = Guid.NewGuid();

        return new ChangeRoadNetwork(lambdaRequest.Provenance)
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(messageId)),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
    }

    protected override Task ValidateIfMatchHeaderValue(CorrectRoadSegmentVersionsSqsLambdaRequest request, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
