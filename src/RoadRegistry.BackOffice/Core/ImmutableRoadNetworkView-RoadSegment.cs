namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
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
                command.StartNodeId,
                command.EndNodeId,
                command.EndNodeId,
                command.TemporaryEndNodeId,
                command.Geometry,
                command.MaintenanceAuthorityId,
                command.MaintenanceAuthorityName,
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

        var segmentBefore = view._segments[command.Id];

        return new ImmutableRoadNetworkView(
            view._nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != command.StartNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != command.EndNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
            view._segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithGeometry(command.Geometry)
                    .WithGeometryVersion(command.GeometryVersion)
                    .WithStartAndEndAndAttributeHash(command.StartNodeId, command.EndNodeId, attributeHash)
                    .WithLanes(command.Lanes.Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                        lane.From,
                        lane.To,
                        lane.Count,
                        lane.Direction,
                        lane.AsOfGeometryVersion)).ToArray())
                    .WithSurfaces(command.Surfaces.Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                        surface.From,
                        surface.To,
                        surface.Type,
                        surface.AsOfGeometryVersion)).ToArray())
                    .WithWidths(command.Widths.Select(width => new BackOffice.RoadSegmentWidthAttribute(
                        width.From,
                        width.To,
                        width.Width,
                        width.AsOfGeometryVersion)).ToArray())
                    .WithLastEventHash(command.GetHash())
                ),
            view._gradeSeparatedJunctions,
            view.SegmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id))),
            view.SegmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id))),
            view.SegmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)))
        );
    }

    private ImmutableRoadNetworkView With(ModifyRoadSegmentAttributes command)
    {
        var segmentBefore = _segments[command.Id];

        var attributeHash = new AttributeHash(
            command.AccessRestriction ?? segmentBefore.AttributeHash.AccessRestriction,
            command.Category ?? segmentBefore.AttributeHash.Category,
            command.Morphology ?? segmentBefore.AttributeHash.Morphology,
            command.Status ?? segmentBefore.AttributeHash.Status,
            command.LeftSide is not null
                ? command.LeftSide.StreetNameId
                : segmentBefore.AttributeHash.LeftStreetNameId,
            command.RightSide is not null
                ? command.RightSide.StreetNameId
                : segmentBefore.AttributeHash.RightStreetNameId,
            command.MaintenanceAuthorityId ?? segmentBefore.AttributeHash.OrganizationId,
            command.GeometryDrawMethod
        );

        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithAttributeHash(attributeHash)
                    .WithLanes(command.Lanes?
                        .Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                            lane.From,
                            lane.To,
                            lane.Count,
                            lane.Direction,
                            lane.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Lanes)
                    .WithSurfaces(command.Surfaces?
                        .Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                            surface.From,
                            surface.To,
                            surface.Type,
                            surface.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Surfaces)
                    .WithWidths(command.Widths?
                        .Select(width => new BackOffice.RoadSegmentWidthAttribute(
                            width.From,
                            width.To,
                            width.Width,
                            width.AsOfGeometryVersion))
                        .ToArray() ?? segmentBefore.Widths)
                    .WithLastEventHash(command.GetHash())
                ),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView With(ModifyRoadSegmentGeometry command)
    {
        var segmentBefore = _segments[command.Id];

        var attributeHash = new AttributeHash(
            segmentBefore.AttributeHash.AccessRestriction,
            segmentBefore.AttributeHash.Category,
            segmentBefore.AttributeHash.Morphology,
            segmentBefore.AttributeHash.Status,
            segmentBefore.AttributeHash.LeftStreetNameId,
            segmentBefore.AttributeHash.RightStreetNameId,
            segmentBefore.AttributeHash.OrganizationId,
            command.GeometryDrawMethod
        );

        var geometry = command.Geometry;
        var geometryVersion = command.GeometryVersion;

        return new ImmutableRoadNetworkView(
            _nodes,
            _segments
                .TryReplace(command.Id, segment => segment
                    .WithVersion(command.Version)
                    .WithGeometryVersion(geometryVersion)
                    .WithGeometry(geometry)
                    .WithAttributeHash(attributeHash)
                    .WithLanes(command.Lanes
                        .Select(lane => new BackOffice.RoadSegmentLaneAttribute(
                            lane.From,
                            lane.To,
                            lane.Count,
                            lane.Direction,
                            lane.AsOfGeometryVersion))
                        .ToArray())
                    .WithSurfaces(command.Surfaces
                        .Select(surface => new BackOffice.RoadSegmentSurfaceAttribute(
                            surface.From,
                            surface.To,
                            surface.Type,
                            surface.AsOfGeometryVersion))
                        .ToArray())
                    .WithWidths(command.Widths
                        .Select(width => new BackOffice.RoadSegmentWidthAttribute(
                            width.From,
                            width.To,
                            width.Width,
                            width.AsOfGeometryVersion))
                        .ToArray())
                    .WithLastEventHash(command.GetHash())
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

    private ImmutableRoadNetworkView With(RemoveRoadSegment command)
    {
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(command.Id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(command.Id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(command.Id))
                : _nodes,
            _segments.Remove(command.Id),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegments command)
    {
        var segments = _segments;
        var nodes = _nodes;
        var junctions = _gradeSeparatedJunctions;

        foreach (var roadSegmentId in command.Ids)
        {
            segments.TryGetValue(roadSegmentId, out var segment);
            segments = segments.Remove(roadSegmentId);

            if (segment!.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                continue;
            }

            var linkedJunctions = junctions
                .Where(x => x.Value.LowerSegment == roadSegmentId || x.Value.UpperSegment == roadSegmentId)
                .Select(x => x.Value)
                .ToList();
            foreach (var junction in linkedJunctions)
            {
                junctions = junctions.Remove(junction.Id);
            }

            DisconnectAndUpdateNode(segment.Start);
            DisconnectAndUpdateNode(segment.End);
            continue;

            void DisconnectAndUpdateNode(RoadNodeId nodeId)
            {
                nodes = nodes.TryReplace(nodeId, x => x.DisconnectFrom(roadSegmentId));

                nodes.TryGetValue(nodeId, out var node);
                if (node!.Type == RoadNodeType.EndNode)
                {
                    nodes = nodes.Remove(nodeId);
                }

                if (node.Type == RoadNodeType.FakeNode)
                {
                    nodes = nodes.TryReplace(nodeId, x => x.WithType(RoadNodeType.EndNode));
                }

                if ((node.Type == RoadNodeType.RealNode || node.Type == RoadNodeType.MiniRoundabout) && node.Segments.Count == 2)
                {
                    nodes = nodes.TryReplace(nodeId, x => x.WithType(RoadNodeType.FakeNode));

                    segments.TryGetValue(node.Segments.First(), out var segmentOne);
                    segments.TryGetValue(node.Segments.Last(), out var segmentTwo);

                    //TODO-pr add tests for segment merging
                    if (SegmentAttributesAreEqual(segmentOne, segmentTwo))
                    {
                        //TODO-pr merge segments





                    }
                }

                if (node.Type == RoadNodeType.TurningLoopNode)
                {
                    nodes = nodes.TryReplace(nodeId, x => x.WithType(RoadNodeType.EndNode));
                }
            }
        }

        return new ImmutableRoadNetworkView(
            nodes,
            segments,
            junctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private static bool SegmentAttributesAreEqual(RoadSegment segment1, RoadSegment segment2)
    {
        if (segment1.AttributeHash.Category != segment2.AttributeHash.Category
            || segment1.AttributeHash.Morphology != segment2.AttributeHash.Morphology
            || segment1.AttributeHash.GeometryDrawMethod != segment2.AttributeHash.GeometryDrawMethod
            || segment1.AttributeHash.OrganizationId != segment2.AttributeHash.OrganizationId
            || segment1.AttributeHash.Status != segment2.AttributeHash.Status
            || segment1.AttributeHash.AccessRestriction != segment2.AttributeHash.AccessRestriction
            || !segment1.EuropeanRoadAttributes.Values.SequenceEqual(segment2.EuropeanRoadAttributes.Values, new EuropeanRoadAttributeEqualityComparer())
            || !segment1.NationalRoadAttributes.Values.SequenceEqual(segment2.NationalRoadAttributes.Values, new NationalRoadAttributeEqualityComparer())
            || !segment1.NumberedRoadAttributes.Values.SequenceEqual(segment2.NumberedRoadAttributes.Values, new NumberedRoadAttributeEqualityComparer())
        )
        {
            return false;
        }

        if ((segment1.Start == segment2.End || segment1.End == segment2.Start)
            && (segment1.AttributeHash.LeftStreetNameId != segment2.AttributeHash.LeftStreetNameId
                || segment1.AttributeHash.RightStreetNameId != segment2.AttributeHash.RightStreetNameId))
        {
            return false;
        }

        if ((segment1.Start == segment2.Start || segment1.End == segment2.End)
            && (segment1.AttributeHash.LeftStreetNameId != segment2.AttributeHash.RightStreetNameId
                || segment1.AttributeHash.RightStreetNameId != segment2.AttributeHash.LeftStreetNameId))
        {
            return false;
        }

        return true;
    }

    private sealed class EuropeanRoadAttributeEqualityComparer : IEqualityComparer<RoadSegmentEuropeanRoadAttribute>
    {
        public bool Equals(RoadSegmentEuropeanRoadAttribute left, RoadSegmentEuropeanRoadAttribute right)
        {
            return left.Number.Equals(right.Number);
        }

        public int GetHashCode(RoadSegmentEuropeanRoadAttribute instance)
        {
            throw new NotSupportedException();
        }
    }
    private sealed class NationalRoadAttributeEqualityComparer : IEqualityComparer<RoadSegmentNationalRoadAttribute>
    {
        public bool Equals(RoadSegmentNationalRoadAttribute left, RoadSegmentNationalRoadAttribute right)
        {
            return left.Number.Equals(right.Number);
        }

        public int GetHashCode(RoadSegmentNationalRoadAttribute instance)
        {
            throw new NotSupportedException();
        }
    }
    private sealed class NumberedRoadAttributeEqualityComparer : IEqualityComparer<RoadSegmentNumberedRoadAttribute>
    {
        public bool Equals(RoadSegmentNumberedRoadAttribute left, RoadSegmentNumberedRoadAttribute right)
        {
            return left.Number.Equals(right.Number)
                   && left.Direction.Equals(right.Direction)
                   && left.Ordinal.Equals(right.Ordinal);
        }

        public int GetHashCode(RoadSegmentNumberedRoadAttribute instance)
        {
            throw new NotSupportedException();
        }
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
