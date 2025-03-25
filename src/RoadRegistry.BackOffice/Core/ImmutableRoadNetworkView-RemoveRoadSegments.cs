namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

public partial class ImmutableRoadNetworkView
{
    private ImmutableRoadNetworkView With(RemoveRoadSegments command)
    {
        var view = this;

        foreach (var roadSegmentId in command.Ids)
        {
            var segment = _segments[roadSegmentId];

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

            view = view.TryFixNodeType(command, segment.Start);
            view = view.TryFixNodeType(command, segment.End);
        }

        return view;
    }

    private ImmutableRoadNetworkView TryFixNodeType(RemoveRoadSegments command, RoadNodeId nodeId)
    {
        var node = _nodes[nodeId];

        if (node.Type == RoadNodeType.EndNode)
        {
            return WithRemovedRoadNode(nodeId);
        }

        if (node.Type == RoadNodeType.FakeNode)
        {
            return WithChangedRoadNodeType(nodeId, RoadNodeType.EndNode);
        }

        if ((node.Type == RoadNodeType.RealNode || node.Type == RoadNodeType.MiniRoundabout) && node.Segments.Count == 2)
        {
            return WithChangedRoadNodeType(nodeId, RoadNodeType.FakeNode)
                .TryMergeFakeNodeSegments(command, nodeId);
        }

        if (node.Type == RoadNodeType.TurningLoopNode)
        {
            return WithChangedRoadNodeType(nodeId, RoadNodeType.EndNode);
        }

        return this;
    }

    private ImmutableRoadNetworkView TryMergeFakeNodeSegments(RemoveRoadSegments command, RoadNodeId nodeId)
    {
        var node = _nodes[nodeId];

        if (node.Type != RoadNodeType.FakeNode || node.Segments.Count != 2)
        {
            throw new InvalidOperationException($"Node {nodeId} should be of type {nameof(RoadNodeType.FakeNode)} and have exactly 2 connecting road segments.");
        }

        var segmentOne = _segments[node.Segments.First()];
        var segmentTwo = _segments[node.Segments.Last()];

        var anyConnectedSegmentIsMarkedForRemoval = command.Ids.Contains(segmentOne.Id) || command.Ids.Contains(segmentTwo.Id);
        if (anyConnectedSegmentIsMarkedForRemoval || !SegmentAttributesAreEqual(segmentOne, segmentTwo))
        {
            return this;
        }

        if (segmentOne.GetOppositeNode(nodeId) == segmentTwo.GetOppositeNode(nodeId))
        {
            return WithChangedRoadNodeType(nodeId, RoadNodeType.TurningLoopNode);
        }

        var mergedSegment = MergeSegments(segmentOne, segmentTwo)
            .WithLastEventHash(command.GetHash());

        var segments = _segments
            .Remove(segmentOne!.Id)
            .Remove(segmentTwo!.Id)
            .Add(mergedSegment.Id, mergedSegment);

        var nodes = _nodes
            .Remove(nodeId)
            .TryReplace(mergedSegment.Start, x => x
                .ConnectWith(mergedSegment.Id)
                .DisconnectFrom(segmentOne.Id))
            .TryReplace(mergedSegment.End, x => x
                .ConnectWith(mergedSegment.Id)
                .DisconnectFrom(segmentTwo.Id));

        var junctions = _gradeSeparatedJunctions;
        var connectedJunctions = junctions
            .Where(x =>
                node.Segments.Contains(x.Value.LowerSegment)
                || node.Segments.Contains(x.Value.UpperSegment));
        foreach (var junction in connectedJunctions)
        {
            junctions = junctions.TryReplace(junction.Key, j =>
            {
                if (node.Segments.Contains(j.LowerSegment))
                {
                    j = j.WithLowerSegment(mergedSegment.Id);
                }

                if (node.Segments.Contains(j.UpperSegment))
                {
                    j = j.WithUpperSegment(mergedSegment.Id);
                }

                return j;
            });
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

    private RoadSegment MergeSegments(RoadSegment segment1, RoadSegment segment2)
    {
        var id = _segments.Keys.Max().Next();

        var commonNode = segment1.GetCommonNode(segment2)!.Value;
        var startNode = segment1.GetOppositeNode(commonNode)!.Value;
        var endNode = segment2.GetOppositeNode(commonNode)!.Value;

        var segment1HasIdealDirection = startNode == segment1.Start;
        var segment2HasIdealDirection = endNode == segment2.End;

        var leftStreetNameId = segment1HasIdealDirection
            ? segment1.AttributeHash.LeftStreetNameId
            : segment1.AttributeHash.RightStreetNameId;
        var rightStreetNameId = segment1HasIdealDirection
            ? segment1.AttributeHash.RightStreetNameId
            : segment1.AttributeHash.LeftStreetNameId;

        var geometry = MergeGeometries(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection, startNode, endNode, commonNode);
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

    private MultiLineString MergeGeometries(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection,
        RoadNodeId startNode, RoadNodeId endNode, RoadNodeId commonNode)
    {
        var geometry1Coordinates = segment1.Geometry.GetSingleLineString().Coordinates;
        var geometry2Coordinates = segment2.Geometry.GetSingleLineString().Coordinates;

        var startNodeCoordinate = _nodes[startNode].Geometry.Coordinate;
        var endNodeCoordinate = _nodes[endNode].Geometry.Coordinate;
        var commonNodeCoordinate = _nodes[commonNode].Geometry.Coordinate;

        var coordinates = Enumerable.Empty<Coordinate>()
            .Concat([startNodeCoordinate])
            .Concat((segment1HasIdealDirection ? geometry1Coordinates : geometry1Coordinates.Reverse()).ExcludeFirstAndLast())
            .Concat([commonNodeCoordinate])
            .Concat((segment2HasIdealDirection ? geometry2Coordinates : geometry2Coordinates.Reverse()).ExcludeFirstAndLast())
            .Concat([endNodeCoordinate])
            .ToArray();

        return new MultiLineString([new LineString(coordinates)])
            .WithSrid(segment1.Geometry.SRID);
    }

    private static List<BackOffice.RoadSegmentLaneAttribute> BuildLanes(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentLaneAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Lanes : segment1.Lanes.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Lanes : segment2.Lanes.Reverse())
            .ToArray();

        var mergedItems = new List<BackOffice.RoadSegmentLaneAttribute>();
        RoadSegmentLaneAttributeData? previousItemData = null;
        var fromPosition = 0.0M;
        var toPosition = 0.0M;

        foreach (var item in itemsToAdd)
        {
            var itemData = new RoadSegmentLaneAttributeData(item.Count, item.Direction);
            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();

            if (previousItemData is null || previousItemData == itemData)
            {
                toPosition += laneDistance;
                previousItemData = itemData;
                continue;
            }

            AddCurrentItemToMergedItems();

            fromPosition = toPosition;
            toPosition += laneDistance;
            previousItemData = itemData;
        }

        AddCurrentItemToMergedItems();

        return mergedItems;

        void AddCurrentItemToMergedItems()
        {
            mergedItems.Add(new BackOffice.RoadSegmentLaneAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(toPosition),
                previousItemData!.Count,
                previousItemData.Direction,
                GeometryVersion.Initial
            ));
        }
    }
    private sealed record RoadSegmentLaneAttributeData(RoadSegmentLaneCount Count, RoadSegmentLaneDirection Direction);

    private static List<BackOffice.RoadSegmentSurfaceAttribute> BuildSurfaces(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentSurfaceAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Surfaces : segment1.Surfaces.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Surfaces : segment2.Surfaces.Reverse())
            .ToArray();

        var mergedItems = new List<BackOffice.RoadSegmentSurfaceAttribute>();
        RoadSegmentSurfaceAttributeData? previousItemData = null;
        var fromPosition = 0.0M;
        var toPosition = 0.0M;

        foreach (var item in itemsToAdd)
        {
            var itemData = new RoadSegmentSurfaceAttributeData(item.Type);
            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();

            if (previousItemData is null || previousItemData == itemData)
            {
                toPosition += laneDistance;
                previousItemData = itemData;
                continue;
            }

            AddCurrentItemToMergedItems();

            fromPosition = toPosition;
            toPosition += laneDistance;
            previousItemData = itemData;
        }

        AddCurrentItemToMergedItems();

        return mergedItems;

        void AddCurrentItemToMergedItems()
        {
            mergedItems.Add(new BackOffice.RoadSegmentSurfaceAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(toPosition),
                previousItemData!.Type,
                GeometryVersion.Initial
            ));
        }
    }
    private sealed record RoadSegmentSurfaceAttributeData(RoadSegmentSurfaceType Type);

    private static List<BackOffice.RoadSegmentWidthAttribute> BuildWidths(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection)
    {
        var itemsToAdd = Enumerable.Empty<BackOffice.RoadSegmentWidthAttribute>()
            .Concat(segment1HasIdealDirection ? segment1.Widths : segment1.Widths.Reverse())
            .Concat(segment2HasIdealDirection ? segment2.Widths : segment2.Widths.Reverse())
            .ToArray();

        var mergedItems = new List<BackOffice.RoadSegmentWidthAttribute>();
        RoadSegmentWidthAttributeData? previousItemData = null;
        var fromPosition = 0.0M;
        var toPosition = 0.0M;

        foreach (var item in itemsToAdd)
        {
            var itemData = new RoadSegmentWidthAttributeData(item.Width);
            var laneDistance = item.To.ToDecimal() - item.From.ToDecimal();

            if (previousItemData is null || previousItemData == itemData)
            {
                toPosition += laneDistance;
                previousItemData = itemData;
                continue;
            }

            AddCurrentItemToMergedItems();

            fromPosition = toPosition;
            toPosition += laneDistance;
            previousItemData = itemData;
        }

        AddCurrentItemToMergedItems();

        return mergedItems;

        void AddCurrentItemToMergedItems()
        {
            mergedItems.Add(new BackOffice.RoadSegmentWidthAttribute(
                new RoadSegmentPosition(fromPosition),
                new RoadSegmentPosition(toPosition),
                previousItemData!.Width,
                GeometryVersion.Initial
            ));
        }
    }
    private sealed record RoadSegmentWidthAttributeData(RoadSegmentWidth Width);

    private static bool SegmentAttributesAreEqual(RoadSegment segment1, RoadSegment segment2)
    {
        if (segment1.AttributeHash.Category != segment2.AttributeHash.Category
            || segment1.AttributeHash.Morphology != segment2.AttributeHash.Morphology
            || segment1.AttributeHash.GeometryDrawMethod != segment2.AttributeHash.GeometryDrawMethod
            || segment1.AttributeHash.OrganizationId != segment2.AttributeHash.OrganizationId
            || segment1.AttributeHash.Status != segment2.AttributeHash.Status
            || segment1.AttributeHash.AccessRestriction != segment2.AttributeHash.AccessRestriction
            || !segment1.EuropeanRoadAttributes.Values.OrderBy(x => x.Number)
                .SequenceEqual(segment2.EuropeanRoadAttributes.Values.OrderBy(x => x.Number), new EuropeanRoadAttributeEqualityComparer())
            || !segment1.NationalRoadAttributes.Values.OrderBy(x => x.Number)
                .SequenceEqual(segment2.NationalRoadAttributes.Values.OrderBy(x => x.Number), new NationalRoadAttributeEqualityComparer())
            || !segment1.NumberedRoadAttributes.Values.OrderBy(x => x.Number)
                .SequenceEqual(segment2.NumberedRoadAttributes.Values.OrderBy(x => x.Number), new NumberedRoadAttributeEqualityComparer())
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
}
