namespace RoadRegistry.BackOffice.Core;

using System.Linq;

public partial class ImmutableRoadNetworkView
{
    private ImmutableRoadNetworkView With(AddRoadSegment command)
    {
        var attributeHash = new AttributeHash(
            command.AccessRestriction,
            command.Category,
            command.Morphology,
            command.Status,
            command.LeftSideStreetNameId,
            command.RightSideStreetNameId,
            command.MaintenanceAuthorityId,
            command.GeometryDrawMethod
        );

        var version = RoadSegmentVersion.Initial;
        var geometryVersion = GeometryVersion.Initial;

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
            _segments.Add(command.Id,
                new RoadSegment(command.Id, version, command.Geometry, geometryVersion, command.StartNodeId, command.EndNodeId,
                    attributeHash,
                    command.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(lane.From, lane.To, lane.Count, lane.Direction, lane.AsOfGeometryVersion)).ToArray(),
                    command.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(surface.From, surface.To, surface.Type, surface.AsOfGeometryVersion)).ToArray(),
                    command.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(width.From, width.To, width.Width, width.AsOfGeometryVersion)).ToArray(),
                    command.GetHash())
            ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id))),
            SegmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)))
        );
    }

    private ImmutableRoadNetworkView With(ModifyRoadSegment command)
    {
        var view = this;

        if (command.ConvertedFromOutlined && !_segments.ContainsKey(command.Id))
        {
            view = With(new AddRoadSegment(
                command.Id,
                command.Id,
                command.OriginalId ?? command.Id,
                command.Id,
                command.StartNodeId!.Value,
                command.TemporaryStartNodeId,
                command.EndNodeId!.Value,
                command.TemporaryEndNodeId,
                command.Geometry,
                command.MaintenanceAuthorityId!.Value,
                command.GeometryDrawMethod,
                command.Morphology,
                command.Status,
                command.Category,
                command.AccessRestriction,
                command.LeftSideStreetNameId,
                command.RightSideStreetNameId,
                command.Lanes,
                command.Widths,
                command.Surfaces
            ));
        }

        var segmentBefore = view._segments[command.Id];

        var attributeHash = new AttributeHash(
            command.AccessRestriction ?? segmentBefore.AttributeHash.AccessRestriction,
            command.Category ?? segmentBefore.AttributeHash.Category,
            command.Morphology ?? segmentBefore.AttributeHash.Morphology,
            command.Status ?? segmentBefore.AttributeHash.Status,
            command.LeftSideStreetNameId ?? segmentBefore.AttributeHash.LeftStreetNameId,
            command.RightSideStreetNameId ?? segmentBefore.AttributeHash.RightStreetNameId,
            command.MaintenanceAuthorityId ?? segmentBefore.AttributeHash.OrganizationId,
            command.GeometryDrawMethod
        );

        return new ImmutableRoadNetworkView(
            view._nodes
                .TryReplaceIf(segmentBefore.Start, node => command.StartNodeId is not null && node.Id != command.StartNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplaceIf(segmentBefore.End, node => command.EndNodeId is not null && node.Id != command.EndNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
            view._segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithGeometry(command.Geometry)
                    .WithGeometryVersion(command.GeometryVersion)
                    .WithStartAndEndAndAttributeHash(command.StartNodeId, command.EndNodeId, attributeHash)
                    .WithLanes(command.Lanes?.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        lane.From,
                        lane.To,
                        lane.Count,
                        lane.Direction,
                        lane.AsOfGeometryVersion)).ToArray())
                    .WithSurfaces(command.Surfaces?.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        surface.From,
                        surface.To,
                        surface.Type,
                        surface.AsOfGeometryVersion)).ToArray())
                    .WithWidths(command.Widths?.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        width.From,
                        width.To,
                        width.Width,
                        width.AsOfGeometryVersion)).ToArray())
                    .WithLastEventHash(command.GetHash())
                ),
            view._gradeSeparatedJunctions,
            view.SegmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes?.Select(lane => new AttributeId(lane.Id))),
            view.SegmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths?.Select(width => new AttributeId(width.Id))),
            view.SegmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces?.Select(surface => new AttributeId(surface.Id)))
        );
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegment command)
    {
        return WithRemovedRoadSegment(command.Id);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToEuropeanRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .PartOfEuropeanRoad(new RoadSegmentEuropeanRoadAttribute(
                        command.AttributeId, command.Number
                    ))
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromEuropeanRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .NotPartOfEuropeanRoad(command.AttributeId)
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToNationalRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .PartOfNationalRoad(new RoadSegmentNationalRoadAttribute(
                        command.AttributeId, command.Number
                    ))
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromNationalRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .NotPartOfNationalRoad(command.AttributeId)
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToNumberedRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .PartOfNumberedRoad(new RoadSegmentNumberedRoadAttribute(
                        command.AttributeId,
                        command.Direction,
                        command.Number,
                        command.Ordinal
                    ))
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromNumberedRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.SegmentId, segment => segment
                    .NotPartOfNumberedRoad(command.AttributeId)
                    .WithVersion(command.SegmentVersion)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }
}
