namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using QuikGraph;
using QuikGraph.Algorithms;

internal static class RoadNetworkValidation
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

        var nodesWithRequiredConnections = context.GetPossibleConnectionsForRemovedSegments(removedRoadSegmentIds);
        var graph = BuildGraph(view);

        foreach (var nodeWithRequiredConnections in nodesWithRequiredConnections)
        {
            var start = nodeWithRequiredConnections.Key;
            var ends = nodeWithRequiredConnections.Value;

            var tryGetPaths = graph.ShortestPathsDijkstra(_ => 1, start);
            foreach (var end in ends)
            {
                if (!tryGetPaths(end, out _))
                {
                    problems += new RoadNetworkDisconnected(start, end);
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

    private static Dictionary<RoadNodeId, IEnumerable<RoadNodeId>> GetPossibleConnectionsForRemovedSegments(this AfterVerificationContext context, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds)
    {
        return removedRoadSegmentIds
            .SelectMany(id =>
            {
                context.BeforeView.View.Segments.TryGetValue(id, out var segment);
                return segment!.Nodes;
            })
            .Distinct()
            .Where(x => context.AfterView.Nodes.ContainsKey(x))
            .SelectMany(startNodeId => { return FindNodesToConnectTo(startNodeId, removedRoadSegmentIds, context).Select(endNodeId => new NodeConnection(startNodeId, endNodeId)); })
            .DistinctBy(x => new { x.Source, x.Destination })
            .GroupBy(x => x.Source)
            .Where(x => x.Count() > 1)
            .ToDictionary(x => x.Key, g => g.Select(nodeConnection => nodeConnection.Destination));
    }

    private static IEnumerable<RoadNodeId> FindNodesToConnectTo(RoadNodeId startNodeId, IReadOnlyCollection<RoadSegmentId> removedRoadSegmentIds, AfterVerificationContext context)
    {
        var startNode = context.RootView.Nodes[startNodeId];

        var processedSegmentIds = new List<RoadSegmentId>();
        var processedNodeIds = new List<RoadNodeId> { startNodeId };

        return startNode.Segments
            .Where(removedRoadSegmentIds.Contains)
            .Select(id => context.RootView.Segments[id])
            .SelectMany(segment => GetConnectedOffspringNodes(startNodeId, segment, processedNodeIds, processedSegmentIds, context))
            .Distinct()
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

    private sealed class NodeConnection
    {
        public RoadNodeId Source { get; set; }
        public RoadNodeId Destination { get; set; }

        public NodeConnection(RoadNodeId start, RoadNodeId end)
        {
            Source = RoadNodeId.Min(start, end);
            Destination = RoadNodeId.Max(start, end);
        }
    }
}
