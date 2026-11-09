namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using QuikGraph;
using QuikGraph.Algorithms;
using RoadRegistry.RoadSegment.ValueObjects;
using ValueObjects.Problems;

public static class RoadNetworkValidation
{
    public static Problems ValidateRoadNetworkConnections(this AfterVerificationContext context, Messages.AcceptedChange[] acceptedChanges, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds)
    {
        var problems = Problems.None;

        var view = context.RootView.RestoreFromEvents([
            new RoadNetworkChangesAccepted
            {
                Changes = acceptedChanges
            }
        ]);

        var nodesWithRequiredConnections = context.GetRequiredConnectionsForRemovedSegments(removedRoadSegmentIds)
            .GroupBy(x => x.Source);
        var graph = BuildGraph(view);

        foreach (var nodeWithRequiredConnections in nodesWithRequiredConnections)
        {
            var start = nodeWithRequiredConnections.Key;

            var tryGetPaths = graph.ShortestPathsDijkstra(_ => 1, start);
            foreach (var connection in nodeWithRequiredConnections)
            {
                if (!tryGetPaths(connection.Destination, out _))
                {
                    problems += new RoadNetworkDisconnected(start, connection.Destination)
                        .WithContext(ProblemContext.For(connection.RemovedRoadSegmentId));
                }
            }
        }

        return problems;
    }

    private static UndirectedGraph<int, IEdge<int>> BuildGraph(IRoadNetworkView view)
    {
        var graph = new UndirectedGraph<int, IEdge<int>>(true);

        foreach (var nodeId in view.Nodes.Keys)
        {
            graph.AddVertex(nodeId);
        }

        foreach (var segment in view.Segments.Select(x => x.Value))
        {
            var startVertex = segment.Start;
            var endVertex = segment.End;

            try
            {
                graph.AddEdge(Comparer<int>.Default.Compare(startVertex, endVertex) > 0
                    ? new UndirectedEdge<int>(endVertex, startVertex)
                    : new UndirectedEdge<int>(startVertex, endVertex));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error trying to create an edge between vertices {startVertex} and {endVertex}", ex);
            }
        }

        return graph;
    }

    public static Dictionary<RoadNodeId, IEnumerable<RoadNodeId>> GetPossibleConnectionsForRemovedSegments(this AfterVerificationContext context, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds)
    {
        return context.GetRequiredConnectionsForRemovedSegments(removedRoadSegmentIds)
            .GroupBy(x => x.Source)
            .ToDictionary(x => x.Key, g => g.Select(nodeConnection => nodeConnection.Destination));
    }

    public static IReadOnlyCollection<RequiredRoadNodeConnection> GetRequiredConnectionsForRemovedSegments(this AfterVerificationContext context, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds)
    {
        var existingNodesAfterRemoval = removedRoadSegmentIds
            .SelectMany(id => context.BeforeView.View.Segments[id].Nodes)
            .Distinct()
            .Where(x => context.AfterView.Nodes.ContainsKey(x));

        return existingNodesAfterRemoval
            .SelectMany(startNodeId => FindNodesToConnectTo(startNodeId, removedRoadSegmentIds, context)
                .Select(x => RequiredRoadNodeConnection.Create(startNodeId, x.EndNodeId, x.RemovedRoadSegmentId)))
            .DistinctBy(x => new { x.Source, x.Destination })
            .ToList();
    }

    // Each node to connect to is paired with the removed segment the search left the start node through, so a
    // disconnection can be traced back to the segment whose removal causes it.
    private static IEnumerable<(RoadNodeId EndNodeId, RoadSegmentId RemovedRoadSegmentId)> FindNodesToConnectTo(RoadNodeId startNodeId, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds, AfterVerificationContext context)
    {
        var startNode = context.RootView.Nodes[startNodeId];

        var processedSegmentIds = new List<RoadSegmentId>();
        var processedNodeIds = new List<RoadNodeId> { startNodeId };

        return startNode.Segments
            .Where(removedRoadSegmentIds.Contains)
            .Select(id => context.RootView.Segments[id])
            .SelectMany(segment => GetConnectedOffspringNodes(startNodeId, segment, processedNodeIds, processedSegmentIds, context)
                .Select(endNodeId => (EndNodeId: endNodeId, RemovedRoadSegmentId: segment.Id)))
            .DistinctBy(x => x.EndNodeId)
            .ToList();
    }

    private static IEnumerable<RoadNodeId> GetConnectedOffspringNodes(
        RoadNodeId currentNodeId,
        RoadSegment currentSegment,
        List<RoadNodeId> processedNodeIds,
        List<RoadSegmentId> processedSegmentIds,
        AfterVerificationContext context)
    {
        if (processedSegmentIds.Contains(currentSegment.Id))
        {
            yield break;
        }

        processedSegmentIds.Add(currentSegment.Id);

        var otherNodeId = currentSegment.GetOppositeNode(currentNodeId)!.Value;
        if (processedNodeIds.Contains(otherNodeId))
        {
            yield break;
        }

        processedNodeIds.Add(otherNodeId);

        if (context.AfterView.Nodes.ContainsKey(otherNodeId))
        {
            yield return otherNodeId;
            yield break;
        }

        var otherNode = context.RootView.Nodes[otherNodeId];

        foreach (var endNodeId in otherNode.Segments
                     .Select(id => context.RootView.Segments[id])
                     .SelectMany(segment => GetConnectedOffspringNodes(otherNodeId, segment, processedNodeIds, processedSegmentIds, context)))
        {
            yield return endNodeId;
        }
    }
}

public sealed record RequiredRoadNodeConnection(RoadNodeId Source, RoadNodeId Destination, RoadSegmentId RemovedRoadSegmentId)
{
    public static RequiredRoadNodeConnection Create(RoadNodeId start, RoadNodeId end, RoadSegmentId removedRoadSegmentId)
    {
        return new RequiredRoadNodeConnection(RoadNodeId.Min(start, end), RoadNodeId.Max(start, end), removedRoadSegmentId);
    }
}
