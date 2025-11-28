namespace RoadRegistry.BackOffice.Handlers.RoadSegments;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Core;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.BackOffice.Uploads;
using AddRoadSegment = BackOffice.Uploads.AddRoadSegment;
using Reason = ValueObjects.Reason;
using RemoveOutlinedRoadSegmentFromRoadNetwork = BackOffice.Uploads.RemoveOutlinedRoadSegmentFromRoadNetwork;
using RoadSegmentLaneAttribute = BackOffice.Uploads.RoadSegmentLaneAttribute;
using RoadSegmentSurfaceAttribute = BackOffice.Uploads.RoadSegmentSurfaceAttribute;
using RoadSegmentWidthAttribute = BackOffice.Uploads.RoadSegmentWidthAttribute;

public sealed class MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequestHandler : IRequestHandler<MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest, MigrateOutlinedRoadSegmentsOutOfRoadNetworkResponse>
{
    private readonly IRoadNetworkCommandQueue _roadNetworkCommandQueue;
    private readonly IRoadRegistryContext _roadRegistryContext;
    private readonly ILogger<MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequestHandler> _logger;

    public MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequestHandler(
        IRoadNetworkCommandQueue roadNetworkCommandQueue,
        IRoadRegistryContext roadRegistryContext,
        ILogger<MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequestHandler> logger)
    {
        _roadNetworkCommandQueue = roadNetworkCommandQueue;
        _roadRegistryContext = roadRegistryContext;
        _logger = logger;
    }

    public async Task<MigrateOutlinedRoadSegmentsOutOfRoadNetworkResponse> Handle(MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest request, CancellationToken cancellationToken)
    {
        var translatedChanges = TranslatedChanges.Empty
            .WithOrganization(OrganizationId.DigitaalVlaanderen)
            .WithOperatorName(OperatorName.Unknown)
            .WithReason(new Reason("Migreer ingeschetste wegsegmenten uit het wegennetwerk"));

        var network = await _roadRegistryContext.RoadNetworks.Get(cancellationToken);
        var roadSegments = network.FindRoadSegments(x => x.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined);

        if (!roadSegments.Any())
        {
            return new MigrateOutlinedRoadSegmentsOutOfRoadNetworkResponse(0);
        }

        var recordNumber = RecordNumber.Initial;
        var attributeIdProvider = new NextAttributeIdProvider(AttributeId.Initial);

        foreach (var roadSegment in roadSegments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var removeRoadSegment = new RemoveOutlinedRoadSegmentFromRoadNetwork(
                recordNumber,
                roadSegment.Id
            );

            var addRoadSegment = new AddRoadSegment(
                recordNumber,
                roadSegment.Id,
                roadSegment.Id,
                new RoadNodeId(0),
                new RoadNodeId(0),
                roadSegment.AttributeHash.OrganizationId,
                roadSegment.AttributeHash.GeometryDrawMethod,
                roadSegment.AttributeHash.Morphology,
                roadSegment.AttributeHash.Status,
                roadSegment.AttributeHash.Category,
                roadSegment.AttributeHash.AccessRestriction,
                roadSegment.AttributeHash.LeftStreetNameId,
                roadSegment.AttributeHash.RightStreetNameId
            )
            {
                PermanentId = roadSegment.Id
            }.WithGeometry(roadSegment.Geometry);

            if (roadSegment.Lanes.Any())
            {
                foreach (var lane in roadSegment.Lanes)
                {
                    addRoadSegment = addRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeIdProvider.Next(), lane.Count, lane.Direction, lane.From, lane.To));
                }
            }
            else
            {
                addRoadSegment = addRoadSegment.WithLane(new RoadSegmentLaneAttribute(attributeIdProvider.Next(), RoadSegmentLaneCount.Unknown, RoadSegmentLaneDirection.Unknown, new RoadSegmentPosition(0), RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)));
            }

            if (roadSegment.Surfaces.Any())
            {
                foreach (var surface in roadSegment.Surfaces)
                {
                    addRoadSegment = addRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), surface.Type, surface.From, surface.To));
                }
            }
            else
            {
                addRoadSegment = addRoadSegment.WithSurface(new RoadSegmentSurfaceAttribute(attributeIdProvider.Next(), RoadSegmentSurfaceType.Unknown, new RoadSegmentPosition(0), RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)));
            }

            if (roadSegment.Widths.Any())
            {
                foreach (var width in roadSegment.Widths)
                {
                    addRoadSegment = addRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeIdProvider.Next(), width.Width, width.From, width.To));
                }
            }
            else
            {
                addRoadSegment = addRoadSegment.WithWidth(new RoadSegmentWidthAttribute(attributeIdProvider.Next(), RoadSegmentWidth.Unknown, new RoadSegmentPosition(0), RoadSegmentPosition.FromDouble(roadSegment.Geometry.Length)));
            }

            translatedChanges = translatedChanges.AppendChange(removeRoadSegment);
            translatedChanges = translatedChanges.AppendChange(addRoadSegment);

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

        return new MigrateOutlinedRoadSegmentsOutOfRoadNetworkResponse(roadSegments.Count);
    }
}
