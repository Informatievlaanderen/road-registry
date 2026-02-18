namespace RoadRegistry.ScopedRoadNetwork;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.ValueObjects.Problems;
using ValueObjects;

public partial class ScopedRoadNetwork
{
    public void RemoveRoadSegments(
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        IRoadNetworkIdGenerator idGenerator,
        Provenance provenance)
    {
        var problems = Problems.None;

        var invalidCategories = new[]
        {
            RoadSegmentCategoryV2.EuropeseHoofdweg,
            RoadSegmentCategoryV2.VlaamseHoofdweg
        };

        var idTranslator = new IdentifierTranslator();
        foreach (var roadSegmentId in roadSegmentIds)
        {
            var segmentProblems = TryRemoveRoadSegment(roadSegmentId, invalidCategories, roadSegmentIds, idGenerator, idTranslator, provenance);
            if (segmentProblems.HasError())
            {
                problems += segmentProblems;
            }
        }

        if (problems.Any())
        {
            throw new RoadRegistryProblemsException(problems);
        }
    }

    private Problems TryRemoveRoadSegment(
        RoadSegmentId roadSegmentId,
        IReadOnlyCollection<RoadSegmentCategoryV2> invalidCategories,
        IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds,
        IRoadNetworkIdGenerator idGenerator,
        IIdentifierTranslator idTranslator,
        Provenance provenance)
    {
        var problems = Problems.For(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var segment))
        {
            return problems + new RoadSegmentNotFound(roadSegmentId);
        }

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

        if (segment.Attributes.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingeschetst || problems.HasError())
        {
            return problems;
        }

        problems += TryFixNodeType(segment.StartNodeId, removingRoadSegmentIds, idGenerator, idTranslator, provenance);
        problems += TryFixNodeType(segment.EndNodeId, removingRoadSegmentIds, idGenerator, idTranslator, provenance);

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

    private Problems TryFixNodeType(
        RoadNodeId nodeId,
        IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds,
        IRoadNetworkIdGenerator idGenerator,
        IIdentifierTranslator idTranslator,
        Provenance provenance)
    {
        //var node = _roadNodes[nodeId];
        //
        // if (node.Type == RoadNodeTypeV2.Eindknoop)
        // {
        //     return node.Remove(provenance);
        // }
        //
        // if (node.Type == RoadNodeTypeV2.Schijnknoop)
        // {
        //     return node.Modify(new ModifyRoadNodeChange
        //     {
        //         RoadNodeId = nodeId,
        //         Type = RoadNodeTypeV2.Eindknoop
        //     }, provenance);
        // }
        //
        // var nodeSegments = GetNonRemovedRoadSegments()
        //     .Where(x => x.StartNodeId == nodeId || x.EndNodeId == nodeId)
        //     .ToList();
        //
        // if (node.Type == RoadNodeTypeV2.EchteKnoop && nodeSegments.Count == 2)
        // {
        //     var problems = node.Modify(new ModifyRoadNodeChange
        //     {
        //         RoadNodeId = nodeId,
        //         Type = RoadNodeTypeV2.Schijnknoop
        //     }, provenance);
        //     if (!problems.HasError())
        //     {
        //         problems += TryMergeFakeNodeSegments(nodeId, nodeSegments, removingRoadSegmentIds, idGenerator, idTranslator, provenance);
        //     }
        //
        //     return problems;
        // }

        return Problems.None;
    }

    private Problems TryMergeFakeNodeSegments(RoadNodeId nodeId,
        IReadOnlyCollection<RoadSegment> nodeSegments,
        IReadOnlyCollection<RoadSegmentId> removingRoadSegmentIds,
        IRoadNetworkIdGenerator idGenerator,
        ScopedRoadNetworkContext context)
    {
        var node = _roadNodes[nodeId];

        if (node.Type != RoadNodeTypeV2.Schijnknoop || nodeSegments.Count != 2)
        {
            throw new InvalidOperationException($"Node {nodeId} should be of type {nameof(RoadNodeTypeV2.Schijnknoop)} and have exactly 2 connecting road segments.");
        }

        var segmentOne = nodeSegments.First();
        var segmentTwo = nodeSegments.Last();

        var anyConnectedSegmentIsMarkedForRemoval = removingRoadSegmentIds.Contains(segmentOne.RoadSegmentId) || removingRoadSegmentIds.Contains(segmentTwo.RoadSegmentId);
        if (anyConnectedSegmentIsMarkedForRemoval || !segmentOne.Attributes.EqualsOnlyNonDynamicAttributes(segmentTwo.Attributes))
        {
            return Problems.None;
        }

        //TODO-pr TBD roadnodetype logic voor V2?
        // if (segmentOne.GetOppositeNode(nodeId) == segmentTwo.GetOppositeNode(nodeId))
        // {
        //     return node.Modify(new ModifyRoadNodeChange
        //     {
        //         RoadNodeId = nodeId,
        //         Type = RoadNodeTypeV2.TurningLoopNode
        //     }, provenance);
        // }

        return MergeSegments(segmentOne, segmentTwo, idGenerator, context);
    }

    private Problems MergeSegments(RoadSegment segment1, RoadSegment segment2, IRoadNetworkIdGenerator idGenerator, ScopedRoadNetworkContext context)
    {
        var commonNodeId = segment1.GetCommonNode(segment2)!.Value;
        var startNodeId = segment1.GetOppositeNode(commonNodeId)!.Value;
        var endNodeId = segment2.GetOppositeNode(commonNodeId)!.Value;

        var segment1HasIdealDirection = startNodeId == segment1.StartNodeId;
        var segment2HasIdealDirection = endNodeId == segment2.EndNodeId;

        var geometry = MergeGeometries(segment1, segment2, segment1HasIdealDirection, segment2HasIdealDirection, startNodeId, endNodeId, commonNodeId);

        var mergedSegment = new MergeRoadSegmentChange
        {
            TemporaryId = _roadSegments.Keys.Max().Next(),
            OriginalIds = [segment1.RoadSegmentId, segment2.RoadSegmentId],
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = segment1.Attributes.GeometryDrawMethod,
            AccessRestriction = segment1.Attributes.AccessRestriction.MergeWith(segment2.Attributes.AccessRestriction, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Category = segment1.Attributes.Category.MergeWith(segment2.Attributes.Category, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Morphology = segment1.Attributes.Morphology.MergeWith(segment2.Attributes.Morphology, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            Status = segment1.Attributes.Status.MergeWith(segment2.Attributes.Status, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            StreetNameId = segment1.Attributes.StreetNameId.MergeWith(segment2.Attributes.StreetNameId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            MaintenanceAuthorityId = segment1.Attributes.MaintenanceAuthorityId.MergeWith(segment2.Attributes.MaintenanceAuthorityId, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            SurfaceType = segment1.Attributes.SurfaceType.MergeWith(segment2.Attributes.SurfaceType, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            CarAccess = segment1.Attributes.CarAccess.MergeWith(segment2.Attributes.CarAccess, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            BikeAccess = segment1.Attributes.BikeAccess.MergeWith(segment2.Attributes.BikeAccess, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            PedestrianAccess = segment1.Attributes.PedestrianAccess.MergeWith(segment2.Attributes.PedestrianAccess, segment1.Geometry.Value.Length, segment2.Geometry.Value.Length, segment1HasIdealDirection, segment2HasIdealDirection),
            EuropeanRoadNumbers = segment1.Attributes.EuropeanRoadNumbers,
            NationalRoadNumbers = segment1.Attributes.NationalRoadNumbers
        };

        var (roadSegment, problems) = RoadSegment.Merge(mergedSegment, idGenerator, context);
        if (problems.HasError())
        {
            return problems;
        }

        problems += context.IdTranslator.RegisterMapping(mergedSegment.TemporaryId, roadSegment!.RoadSegmentId);
        _roadSegments.Add(roadSegment.RoadSegmentId, roadSegment);

        problems += segment1.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
        problems += segment2.RetireBecauseOfMerger(roadSegment.RoadSegmentId, context.Provenance);
        problems += _roadNodes[commonNodeId].Remove(context.Provenance);

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

            problems += junction.Modify(modify, context.Provenance);
        }

        return problems;
    }

    private MultiLineString MergeGeometries(
        RoadSegment segment1, RoadSegment segment2,
        bool segment1HasIdealDirection, bool segment2HasIdealDirection,
        RoadNodeId startNode, RoadNodeId endNode, RoadNodeId commonNode)
    {
        var geometry1Coordinates = segment1.Geometry.Value.GetSingleLineString().Coordinates;
        var geometry2Coordinates = segment2.Geometry.Value.GetSingleLineString().Coordinates;

        var startNodeCoordinate = _roadNodes[startNode].Geometry.Value.Coordinate;
        var endNodeCoordinate = _roadNodes[endNode].Geometry.Value.Coordinate;
        var commonNodeCoordinate = _roadNodes[commonNode].Geometry.Value.Coordinate;

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
