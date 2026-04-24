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
        var problems = Problems.WithContext(roadSegmentId);

        if (!_roadSegments.TryGetValue(roadSegmentId, out var segment))
        {
            return problems + new RoadSegmentNotFound();
        }

        var segmentCategories = segment.Attributes!.Category.Values.Select(x => x.Value).ToArray();
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

        if (segment.Attributes.Status != RoadSegmentStatusV2.Gerealiseerd || problems.HasError())
        {
            return problems;
        }

        problems += TryFixNodeType(segment.StartNodeId!.Value, removingRoadSegmentIds, idGenerator, idTranslator, provenance);
        problems += TryFixNodeType(segment.EndNodeId!.Value, removingRoadSegmentIds, idGenerator, idTranslator, provenance);

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
        if (anyConnectedSegmentIsMarkedForRemoval || !segmentOne.Attributes!.EqualsOnlyNonDynamicAttributes(segmentTwo.Attributes))
        {
            return Problems.None;
        }

        //TODO-pr uncomment lambda integrationtest
        //TODO-pr TBD roadnodetype logic voor V2?
        // if (segmentOne.GetOppositeNode(nodeId) == segmentTwo.GetOppositeNode(nodeId))
        // {
        //     return node.Modify(new ModifyRoadNodeChange
        //     {
        //         RoadNodeId = nodeId,
        //         Type = RoadNodeTypeV2.TurningLoopNode
        //     }, provenance);
        // }

        return MergeRoadSegments(segmentOne, segmentTwo, idGenerator, context).Item2;
    }
}
