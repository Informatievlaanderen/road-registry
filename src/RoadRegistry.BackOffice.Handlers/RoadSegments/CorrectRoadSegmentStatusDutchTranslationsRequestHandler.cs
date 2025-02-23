namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using System.Diagnostics;
using Abstractions.RoadSegments;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using Editor.Schema;
using FluentValidation;
using Framework;
using MediatR;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using Reason = Reason;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CorrectRoadSegmentStatusDutchTranslationsRequestHandler
    : IRequestHandler<CorrectRoadSegmentStatusDutchTranslationsRequest, CorrectRoadSegmentStatusDutchTranslationsResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly ILogger<CorrectRoadSegmentStatusDutchTranslationsRequestHandler> _logger;

    public CorrectRoadSegmentStatusDutchTranslationsRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadSegmentStatusDutchTranslationsRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _editorContextFactory = editorContextFactory;
        _logger = logger;
    }

    public async Task<CorrectRoadSegmentStatusDutchTranslationsResponse> Handle(CorrectRoadSegmentStatusDutchTranslationsRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(OrganizationId.DigitaalVlaanderen)
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Corrigeer wegsegmenten status"));

        var roadSegmentIdsWithRequestedStatus = await GetRoadSegmentIdsWithRequestedStatus(request.Identifier, cancellationToken);
        if (!roadSegmentIdsWithRequestedStatus.Any())
        {
            return new CorrectRoadSegmentStatusDutchTranslationsResponse(0);
        }

        var network = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);

        var roadSegments = network.FindRoadSegments(roadSegmentIdsWithRequestedStatus.Select(x => new RoadSegmentId(x)));

        var recordNumber = RecordNumber.Initial;
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        foreach (var roadSegment in roadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var modifyRoadSegment = new ModifyRoadSegment(
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
            ).WithGeometry(roadSegment.Geometry);

            foreach (var lane in roadSegment.Lanes)
            {
                modifyRoadSegment = modifyRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, lane.From, lane.To));
            }
            foreach (var surface in roadSegment.Surfaces)
            {
                modifyRoadSegment = modifyRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, surface.From, surface.To));
            }
            foreach (var width in roadSegment.Widths)
            {
                modifyRoadSegment = modifyRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, width.From, width.To));
            }

            translatedChanges = translatedChanges.AppendChange(modifyRoadSegment);

            recordNumber = recordNumber.Next();
        }

        var requestedChanges = translatedChanges.Select(change =>
        {
            var requestedChange = new RequestedChange();
            change.TranslateTo(requestedChange);
            return requestedChange;
        }).ToList();

        var changeRoadNetwork = new ChangeRoadNetwork
        {
            RequestId = ChangeRequestId.FromUploadId(new UploadId(Guid.NewGuid())),
            Changes = requestedChanges.ToArray(),
            Reason = translatedChanges.Reason,
            Operator = translatedChanges.Operator,
            OrganizationId = translatedChanges.Organization
        };
        await new ChangeRoadNetworkValidator().ValidateAndThrowAsync(changeRoadNetwork, cancellationToken);

        await _roadNetworkCommandQueue.WriteAsync(new Command(changeRoadNetwork), cancellationToken);

        return new CorrectRoadSegmentStatusDutchTranslationsResponse(roadSegments.Count);
    }

    private async Task<List<int>> GetRoadSegmentIdsWithRequestedStatus(int statusIdentifier, CancellationToken cancellationToken)
    {
        await using var context = _editorContextFactory();

        var roadSegmentIds = new List<int>();
        const int pageSize = 5000;
        var pageIndex = 0;

        while (await FillRoadSegmentIdsWithRequestedStatus(context, statusIdentifier, pageIndex++, pageSize, roadSegmentIds, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
        }

        return roadSegmentIds;
    }

    private async Task<bool> FillRoadSegmentIdsWithRequestedStatus(EditorContext context, int statusIdentifier, int pageIndex, int pageSize, List<int> roadSegmentIds, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);
        var roadSegments = await context.RoadSegments
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments
            .Where(x => x.StatusId == statusIdentifier)
            .Select(x => x.Id));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }
}
