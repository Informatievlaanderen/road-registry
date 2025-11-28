namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Editor.Projections;
using RoadRegistry.Editor.Schema;
using System.Diagnostics;
using Editor.Schema.Extensions;
using RoadSegment.ValueObjects;
using ModifyRoadSegment = BackOffice.Uploads.ModifyRoadSegment;
using Reason = BackOffice.Reason;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class CorrectRoadSegmentVersionsRequestHandler : IRequestHandler<CorrectRoadSegmentVersionsRequest, CorrectRoadSegmentVersionsResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly Func<EditorContext> _editorContextFactory;
    private readonly RecyclableMemoryStreamManager _manager;
    private readonly FileEncoding _fileEncoding;
    private readonly ILogger<CorrectRoadSegmentVersionsRequestHandler> _logger;

    public CorrectRoadSegmentVersionsRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        Func<EditorContext> editorContextFactory,
        RecyclableMemoryStreamManager manager,
        FileEncoding fileEncoding,
        ILogger<CorrectRoadSegmentVersionsRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _editorContextFactory = editorContextFactory;
        _manager = manager;
        _fileEncoding = fileEncoding;
        _logger = logger;
    }

    public async Task<CorrectRoadSegmentVersionsResponse> Handle(CorrectRoadSegmentVersionsRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(OrganizationId.DigitaalVlaanderen)
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Corrigeer wegsegmenten versies"));

        var roadSegmentsToCorrect = request.RoadSegments?.ToList();
        if (roadSegmentsToCorrect is null)
        {
            var invalidRoadSegmentIds = await GetRoadSegmentIdsWithInvalidVersions(cancellationToken);
            roadSegmentsToCorrect = invalidRoadSegmentIds
                .Select(id => new CorrectRoadSegmentVersion(id, null, null))
                .ToList();
        }

        if (!roadSegmentsToCorrect.Any())
        {
            return new CorrectRoadSegmentVersionsResponse(0);
        }

        var recordNumber = RecordNumber.Initial;
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        using (var editorContext = _editorContextFactory())
        {
            foreach (var correction in roadSegmentsToCorrect)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var editorRoadSegment = await editorContext.RoadSegments
                    .IgnoreQueryFilters()
                    .SingleOrDefaultAsync(x => x.Id == correction.Id, cancellationToken);
                if (editorRoadSegment is null)
                {
                    _logger.LogError($"No roadsegment found in editor table for ID {correction.Id}");
                    continue;
                }

                if (editorRoadSegment.IsRemoved)
                {
                    continue;
                }

                var roadSegmentGeometryDrawMethod = editorRoadSegment.MethodId;

                var roadSegmentId = new RoadSegmentId(correction.Id);
                var stream = RoadNetworkStreamNameProvider.Get(roadSegmentId, RoadSegmentGeometryDrawMethod.ByIdentifier[roadSegmentGeometryDrawMethod]);
                var network = await _roadRegistryContext.RoadNetworks.Get(stream, cancellationToken);
                var roadSegment = network.FindRoadSegment(roadSegmentId);

                var modifyRoadSegment = new ModifyRoadSegment(
                        recordNumber,
                        roadSegment.Id,
                        roadSegment.AttributeHash.GeometryDrawMethod,
                        roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? new RoadNodeId(0) : roadSegment.Start,
                        roadSegment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined ? new RoadNodeId(0) : roadSegment.End,
                        roadSegment.AttributeHash.OrganizationId,
                        roadSegment.AttributeHash.Morphology,
                        roadSegment.AttributeHash.Status,
                        roadSegment.AttributeHash.Category,
                        roadSegment.AttributeHash.AccessRestriction,
                        roadSegment.AttributeHash.LeftStreetNameId,
                        roadSegment.AttributeHash.RightStreetNameId
                    ).WithGeometry(roadSegment.Geometry);

                if (correction.Version is not null)
                {
                    modifyRoadSegment = modifyRoadSegment.WithVersion(new RoadSegmentVersion(correction.Version.Value));
                }
                if (correction.GeometryVersion is not null)
                {
                    modifyRoadSegment = modifyRoadSegment.WithGeometryVersion(new GeometryVersion(correction.GeometryVersion.Value));
                }

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

        return new CorrectRoadSegmentVersionsResponse(translatedChanges.Count);
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

        _logger.LogInformation("Read started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);
        var roadSegments = await context.RoadSegments
            .Where(x => x.Version == 0 || x.GeometryVersion == 0)
            .OrderBy(x => x.Id)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        _logger.LogInformation("Read finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        sw.Restart();

        _logger.LogInformation("Add DbaseRecord temp collection started for {EntityName} from EditorContext (Page {PageIndex}, Size {PageSize})", nameof(context.RoadSegments), pageIndex, pageSize);

        roadSegmentIds.AddRange(roadSegments.Select(x => x.Id));
        _logger.LogInformation("Add DbaseRecord temp collection finished for {EntityName} from EditorContext in {StopwatchElapsedMilliseconds}ms", nameof(context.RoadSegments), sw.ElapsedMilliseconds);

        return roadSegments.Any();
    }
}
