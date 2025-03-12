namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

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
        var view = this;

        foreach (var roadSegmentId in command.Ids)
        {
            _segments.TryGetValue(roadSegmentId, out var segment);
            if (segment is null)
            {
                throw new NullReferenceException($"Segment with ID {roadSegmentId} was not found");
            }

            view = view.WithRemovedRoadSegment(roadSegmentId);

            if (segment.AttributeHash.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined)
            {
                continue;
            }

            var removeJunctionIds = _gradeSeparatedJunctions
                .Where(x => x.Value.LowerSegment == roadSegmentId || x.Value.UpperSegment == roadSegmentId)
                .Select(x => x.Value.Id)
                .ToArray();
            foreach (var junctionId in removeJunctionIds)
            {
                view = view.WithRemovedGradeSeparatedJunction(junctionId);
            }

            view = view.WithFixedNodeType(command, segment.Start);
            view = view.WithFixedNodeType(command, segment.End);
        }

        return view;
    }

    private ImmutableRoadNetworkView WithFixedNodeType(RemoveRoadSegments command, RoadNodeId nodeId)
    {
        var nodes = _nodes;
        var segments = _segments;
        var laneAttributeIdentifiers = SegmentReusableLaneAttributeIdentifiers;
        var surfaceAttributeIdentifiers = SegmentReusableSurfaceAttributeIdentifiers;
        var widthAttributeIdentifiers = SegmentReusableWidthAttributeIdentifiers;

        var lastEventHash = command.GetHash();

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

            var anyConnectedSegmentIsMarkedForRemoval = command.Ids.Contains(segmentOne!.Id) || command.Ids.Contains(segmentTwo!.Id);
            if (!anyConnectedSegmentIsMarkedForRemoval && SegmentAttributesAreEqual(segmentOne, segmentTwo))
            {
                var mergedSegment = MergeSegments(segmentOne, segmentTwo)
                    .WithLastEventHash(lastEventHash);

                segments = segments
                    .Remove(segmentOne!.Id)
                    .Remove(segmentTwo!.Id)
                    .Add(mergedSegment.Id, mergedSegment);

                nodes = nodes
                    .Remove(nodeId)
                    .TryReplace(mergedSegment.Start, x => x
                        .ConnectWith(mergedSegment.Id)
                        .DisconnectFrom(segmentOne.Id))
                    .TryReplace(mergedSegment.End, x => x
                        .ConnectWith(mergedSegment.Id)
                        .DisconnectFrom(segmentTwo.Id));

                laneAttributeIdentifiers = laneAttributeIdentifiers
                    .Remove(segmentOne.Id)
                    .Add(mergedSegment.Id, laneAttributeIdentifiers[segmentOne.Id].Concat(laneAttributeIdentifiers[segmentTwo.Id]).ToList());

                surfaceAttributeIdentifiers = surfaceAttributeIdentifiers
                    .Remove(segmentOne.Id)
                    .Add(mergedSegment.Id, surfaceAttributeIdentifiers[segmentOne.Id].Concat(surfaceAttributeIdentifiers[segmentTwo.Id]).ToList());

                widthAttributeIdentifiers = widthAttributeIdentifiers
                    .Remove(segmentOne.Id)
                    .Add(mergedSegment.Id, widthAttributeIdentifiers[segmentOne.Id].Concat(widthAttributeIdentifiers[segmentTwo.Id]).ToList());
            }
        }

        if (node.Type == RoadNodeType.TurningLoopNode)
        {
            nodes = nodes.TryReplace(nodeId, x => x.WithType(RoadNodeType.EndNode));
        }

        return new ImmutableRoadNetworkView(
            nodes,
            segments,
            _gradeSeparatedJunctions,
            laneAttributeIdentifiers,
            widthAttributeIdentifiers,
            surfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView WithRemovedRoadSegment(RoadSegmentId id)
    {
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(id))
                : _nodes,
            _segments.Remove(id),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView WithRemovedGradeSeparatedJunction(GradeSeparatedJunctionId id)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(id),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private RoadSegment MergeSegments(RoadSegment segment1, RoadSegment segment2)
    {
        //TODO-pr TBD: nieuwe ID generaten, hoe? tijdelijke ID (int max) gebruiken om bij de VerifyAfter een echte ID te genereren -> YES
        var id = new RoadSegmentId(int.MaxValue);

        var commonNode = segment1.GetCommonNode(segment2)!.Value;
        var startNode = segment1.GetOppositeNode(commonNode)!.Value;
        var endNode = segment2.GetOppositeNode(commonNode)!.Value;

        var segment1HasIdealDirection = startNode == segment1.Start;
        var segment2HasIdealDirection = endNode == segment2.End;

        var leftStreetNameId = segment1HasIdealDirection
            ? segment1.AttributeHash.LeftStreetNameId
            : segment1.AttributeHash.RightStreetNameId;
        var rightStreetNameId = segment1HasIdealDirection
            ? segment1.AttributeHash.LeftStreetNameId
            : segment1.AttributeHash.RightStreetNameId;

        var geometry = MergeGeometries(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection);
        var lanes = BuildLanes(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection);
        var surfaces = BuildSurfaces(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection);
        var widths = BuildWidths(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection);

        var mergedSegment = new RoadSegment(
            id,
            RoadSegmentVersion.Initial,
            geometry,
            GeometryVersion.Initial,
            startNode,
            endNode,
            new AttributeHash(
                segment1.AttributeHash.AccessRestriction,
                segment1.AttributeHash.Category,
                segment1.AttributeHash.Morphology,
                segment1.AttributeHash.Status,
                leftStreetNameId,
                rightStreetNameId,
                segment1.AttributeHash.OrganizationId,
                segment1.AttributeHash.GeometryDrawMethod
            ),
            lanes,
            surfaces,
            widths,
            segment1.LastEventHash
        );

        foreach (var europeanRoad in segment1.EuropeanRoadAttributes)
        {
            mergedSegment = mergedSegment.PartOfEuropeanRoad(europeanRoad.Value);
        }
        foreach (var nationalRoad in segment1.NationalRoadAttributes)
        {
            mergedSegment = mergedSegment.PartOfNationalRoad(nationalRoad.Value);
        }
        foreach (var numberedRoad in segment1.NumberedRoadAttributes)
        {
            mergedSegment = mergedSegment.PartOfNumberedRoad(numberedRoad.Value);
        }

        return mergedSegment;
    }

    private static MultiLineString MergeGeometries(RoadSegment segment1, RoadSegment segment2, bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var geometry1Coordinates = segment1.Geometry.GetSingleLineString().Coordinates;
        var geometry2Coordinates = segment2.Geometry.GetSingleLineString().Coordinates;

        var coordinates = Enumerable.Empty<Coordinate>()
            .Concat(segment1HasIdealDirection ? geometry1Coordinates : geometry1Coordinates.Reverse())
            .Concat(segment2HasIdealDirection ? geometry2Coordinates.Skip(1) : geometry1Coordinates.Reverse().Skip(1))
            .ToArray();

        return new MultiLineString([new LineString(coordinates)])
            .WithSrid(segment1.Geometry.SRID);
    }

    private static List<BackOffice.RoadSegmentLaneAttribute> BuildLanes(RoadSegment segment1, RoadSegment segment2, bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentLaneAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Lanes : segment1.Lanes.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Lanes : segment2.Lanes.Reverse())
            .ToArray();

        //TODO-pr merge lanes that are equal (WIP)
        var lanes = new List<BackOffice.RoadSegmentLaneAttribute>();
        RoadSegmentLaneAttributeData? previousItemData = null;
        var fromPosition = 0.0M;
        var toPosition = 0.0M;

        for(var i = 0; i < itemsToAdd.Length; i++)
        {
            var item = itemsToAdd[i];
            var itemData = new RoadSegmentLaneAttributeData(item.Count, item.Direction);
            if (previousItemData is not null && previousItemData == itemData && i < itemsToAdd.Length - 1)
            {
                continue;
            }

            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();
            var newItem = new BackOffice.RoadSegmentLaneAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(fromPosition + laneDistance),
                item.Count,
                item.Direction,
                GeometryVersion.Initial
            );

            lanes.Add(newItem);
            fromPosition += laneDistance;

            previousItemData = itemData;
        }

        return lanes;
    }

    private sealed record RoadSegmentLaneAttributeData(RoadSegmentLaneCount Count, RoadSegmentLaneDirection Direction);

    private static List<BackOffice.RoadSegmentSurfaceAttribute> BuildSurfaces(RoadSegment segment1, RoadSegment segment2, bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var lanes = new List<BackOffice.RoadSegmentSurfaceAttribute>();

        var fromPosition = 0.0M;

        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentSurfaceAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Surfaces : segment1.Surfaces.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Surfaces : segment2.Surfaces.Reverse());

        foreach (var item in itemsToAdd)
        {
            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();
            lanes.Add(new BackOffice.RoadSegmentSurfaceAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(fromPosition + laneDistance),
                item.Type,
                GeometryVersion.Initial
            ));
            fromPosition += laneDistance;
        }

        return lanes;
    }
    private static List<BackOffice.RoadSegmentWidthAttribute> BuildWidths(RoadSegment segment1, RoadSegment segment2, bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var lanes = new List<BackOffice.RoadSegmentWidthAttribute>();

        var fromPosition = 0.0M;

        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentWidthAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Widths : segment1.Widths.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Widths : segment2.Widths.Reverse());

        foreach (var item in itemsToAdd)
        {
            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();
            lanes.Add(new BackOffice.RoadSegmentWidthAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(fromPosition + laneDistance),
                item.Width,
                GeometryVersion.Initial
            ));
            fromPosition += laneDistance;
        }

        return lanes;
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
