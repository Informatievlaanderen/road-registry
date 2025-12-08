namespace RoadRegistry.RoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Schema;
using BackOffice;
using BackOffice.Exceptions;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Extensions;
using GradeSeparatedJunction.Changes;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using RoadSegment;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;

public partial class RoadNetwork
{
    public void RemoveRoadSegments(IReadOnlyCollection<RoadSegmentId> roadSegmentIds, Provenance provenance)
    {
        var roadSegmentsProblems = new Dictionary<RoadSegmentId, Problems>();

        var invalidCategories = new[]
        {
            RoadSegmentCategory.EuropeanMainRoad,
            RoadSegmentCategory.FlemishMainRoad,
            RoadSegmentCategory.MainRoad,
            RoadSegmentCategory.PrimaryRoadI,
            RoadSegmentCategory.PrimaryRoadII
        };

        foreach (var roadSegmentId in roadSegmentIds)
        {
            var problems = TryRemoveRoadSegment(roadSegmentId, invalidCategories, roadSegmentIds, provenance);
            if (problems.HasError())
            {
                roadSegmentsProblems.Add(roadSegmentId, problems);
            }
        }

        if (roadSegmentsProblems.Any())
        {
            throw new RoadSegmentsProblemsException(roadSegmentsProblems);
        }
    }

    private Problems TryRemoveRoadSegment(RoadSegmentId roadSegmentId, IReadOnlyCollection<RoadSegmentCategory> invalidCategories, IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds, Provenance provenance)
    {
        if (!_roadSegments.TryGetValue(roadSegmentId, out var segment))
        {
            return Problems.Single(new RoadSegmentNotFound(roadSegmentId));
        }

        var problems = Problems.None;

        var segmentCategories = segment.Attributes.Category.Values.Select(x => x.Value).ToArray();
        var segmentInvalidCategories = invalidCategories.Intersect(segmentCategories).ToArray();
        if (segmentInvalidCategories.Any())
        {
            foreach (var category in segmentInvalidCategories)
            {
                problems += new RoadSegmentNotRemovedBecauseCategoryIsInvalid(roadSegmentId, category);
            }

            return problems;
        }

        problems += segment.Remove(provenance);

        if (segment.Attributes.GeometryDrawMethod == RoadSegmentGeometryDrawMethod.Outlined || problems.HasError())
        {
            return problems;
        }

        problems += TryFixNodeType(segment.StartNodeId, removingRoadSegmentIds, provenance);
        problems += TryFixNodeType(segment.EndNodeId, removingRoadSegmentIds, provenance);

        var junctions = _gradeSeparatedJunctions
            .Where(x => x.Value.IsConnectedTo(roadSegmentId))
            .Select(x => x.Value)
            .ToArray();
        foreach (var junction in junctions)
        {
            problems += junction.Remove(provenance);
        }

        return problems;
    }

    private Problems TryFixNodeType(RoadNodeId nodeId, IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds, Provenance provenance)
    {
        var node = _roadNodes[nodeId];

        if (node.Type == RoadNodeType.EndNode)
        {
            return node.Remove(provenance);
        }

        if (node.Type == RoadNodeType.FakeNode)
        {
            return node.Modify(new ModifyRoadNodeChange
            {
                RoadNodeId = nodeId,
                Type = RoadNodeType.EndNode
            }, provenance);
        }

        var nodeSegments = GetNonRemovedRoadSegments()
            .Where(x => x.StartNodeId == nodeId || x.EndNodeId == nodeId)
            .ToList();

        if ((node.Type == RoadNodeType.RealNode || node.Type == RoadNodeType.MiniRoundabout) && nodeSegments.Count == 2)
        {
            var problems = node.Modify(new ModifyRoadNodeChange
            {
                RoadNodeId = nodeId,
                Type = RoadNodeType.FakeNode
            }, provenance);
            if (!problems.HasError())
            {
                problems += TryMergeFakeNodeSegments(nodeId, nodeSegments, removingRoadSegmentIds, provenance);
            }

            return problems;
        }

        if (node.Type == RoadNodeType.TurningLoopNode)
        {
            return node.Modify(new ModifyRoadNodeChange
            {
                RoadNodeId = nodeId,
                Type = RoadNodeType.EndNode
            }, provenance);
        }

        return Problems.None;
    }

    private Problems TryMergeFakeNodeSegments(RoadNodeId nodeId,
        IReadOnlyCollection<RoadSegment> nodeSegments,
        IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds,
        Provenance provenance)
    {
        var node = _roadNodes[nodeId];

        if (node.Type != RoadNodeType.FakeNode || nodeSegments.Count != 2)
        {
            throw new InvalidOperationException($"Node {nodeId} should be of type {nameof(RoadNodeType.FakeNode)} and have exactly 2 connecting road segments.");
        }

        var segmentOne = nodeSegments.First();
        var segmentTwo = nodeSegments.Last();

        var anyConnectedSegmentIsMarkedForRemoval = removingRoadSegmentIds.Contains(segmentOne.RoadSegmentId) || removingRoadSegmentIds.Contains(segmentTwo.RoadSegmentId);
        if (anyConnectedSegmentIsMarkedForRemoval || !segmentOne.Attributes.EqualsOnlyNonDynamicAttributes(segmentTwo.Attributes))
        {
            return Problems.None;
        }

        if (segmentOne.GetOppositeNode(nodeId) == segmentTwo.GetOppositeNode(nodeId))
        {
            return node.Modify(new ModifyRoadNodeChange
            {
                RoadNodeId = nodeId,
                Type = RoadNodeType.TurningLoopNode
            }, provenance);
        }

        throw new NotImplementedException();
        //return MergeSegments(segmentOne, segmentTwo);
    }

    private Problems MergeSegments(RoadSegment segment1, RoadSegment segment2, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator, Provenance provenance)
    {
        var commonNodeId = segment1.GetCommonNode(segment2)!.Value;
        var startNodeId = segment1.GetOppositeNode(commonNodeId)!.Value;
        var endNodeId = segment2.GetOppositeNode(commonNodeId)!.Value;

        var segment1HasIdealDirection = startNodeId == segment1.StartNodeId;
        var segment2HasIdealDirection = endNodeId == segment2.EndNodeId;

        var geometry = MergeGeometries(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection, startNodeId, endNodeId, commonNodeId);

        var mergedSegment = new AddRoadSegmentChange
        {
            TemporaryId = _roadSegments.Keys.Max().Next(),
            StartNodeId = startNodeId,
            EndNodeId = endNodeId,
            Geometry = geometry,
            GeometryDrawMethod = segment1.Attributes.GeometryDrawMethod,
            AccessRestriction = segment1.Attributes.AccessRestriction.MergeWith(segment2.Attributes.AccessRestriction, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Category = segment1.Attributes.Category.MergeWith(segment2.Attributes.Category, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Morphology = segment1.Attributes.Morphology.MergeWith(segment2.Attributes.Morphology, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Status = segment1.Attributes.Status.MergeWith(segment2.Attributes.Status, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            StreetNameId = segment1.Attributes.StreetNameId.MergeWith(segment2.Attributes.StreetNameId, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            MaintenanceAuthorityId = segment1.Attributes.MaintenanceAuthorityId.MergeWith(segment2.Attributes.MaintenanceAuthorityId, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            SurfaceType = segment1.Attributes.SurfaceType.MergeWith(segment2.Attributes.SurfaceType, segment1.Geometry.Length, segment2.Geometry.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            EuropeanRoadNumbers = segment1.Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = segment1.Attributes.NationalRoadNumbers
        };

        var (roadSegment, problems) = RoadSegment.Add(mergedSegment, provenance, idGenerator, idTranslator);
        if (problems.HasError())
        {
            return problems;
        }

        problems += idTranslator.RegisterMapping(mergedSegment.TemporaryId, roadSegment!.RoadSegmentId);
        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);

        problems += segment1.Remove(provenance);
        problems += segment2.Remove(provenance);
        problems += _roadNodes[commonNodeId].Remove(provenance);

        var nodeSegmentIds = new[] { segment1.RoadSegmentId, segment2.RoadSegmentId };
        var connectedJunctions = _gradeSeparatedJunctions
            .Where(x => nodeSegmentIds.Any(segmentId => x.Value.IsConnectedTo(segmentId)))
            .Select(x => x.Value)
            .ToArray();

        foreach (var junction in connectedJunctions)
        {
            var modify = new ModifyGradeSeparatedJunctionChange
            {
                GradeSeparatedJunctionId = junction.GradeSeparatedJunctionId
            };

            if (nodeSegmentIds.Contains(junction.LowerRoadSegmentId))
            {
                modify = modify with
                {
                    LowerRoadSegmentId = mergedSegment.TemporaryId
                };
            }

            if (nodeSegmentIds.Contains(junction.UpperRoadSegmentId))
            {
                modify = modify with
                {
                    UpperRoadSegmentId = mergedSegment.TemporaryId
                };
            }

            problems += junction.Modify(modify, provenance);
        }

        return problems;
    }

    private MultiLineString MergeGeometries(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection,
        RoadNodeId startNode, RoadNodeId endNode, RoadNodeId commonNode)
    {
        var geometry1Coordinates = segment1.Geometry.GetSingleLineString().Coordinates;
        var geometry2Coordinates = segment2.Geometry.GetSingleLineString().Coordinates;

        var startNodeCoordinate = _roadNodes[startNode].Geometry.Coordinate;
        var endNodeCoordinate = _roadNodes[endNode].Geometry.Coordinate;
        var commonNodeCoordinate = _roadNodes[commonNode].Geometry.Coordinate;

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
}
