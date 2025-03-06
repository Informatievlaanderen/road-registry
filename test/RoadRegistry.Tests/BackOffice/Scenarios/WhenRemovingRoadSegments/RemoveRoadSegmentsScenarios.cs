namespace RoadRegistry.Tests.BackOffice.Scenarios
{
    using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
    using FluentAssertions;
    using NetTopologySuite.Geometries;
    using QuikGraph;
    using QuikGraph.Algorithms;
    using QuikGraph.Algorithms.Observers;
    using QuikGraph.Algorithms.ShortestPath;
    using RoadRegistry.BackOffice.Messages;

    public class RemoveRoadSegmentsScenarios
    {
        [Fact]
        public void BuildSimpleGraph()
        {
            var graph = new AdjacencyGraph<Coordinate, IEdge<Coordinate>>(true);

            // Edge is wegsegment
            // Vertex is wegknoop

            var bottomLeft = new Coordinate(0, 0);
            var topLeft = new Coordinate(0, 1);
            var topRight = new Coordinate(1, 1);
            var bottomRight = new Coordinate(1, 0);

            var start = bottomLeft;
            var end = bottomRight;

            graph.AddVertex(bottomLeft);
            graph.AddVertex(topLeft);
            graph.AddVertex(topRight);
            graph.AddVertex(bottomRight);

            graph.AddEdge(new Edge<Coordinate>(bottomLeft, topLeft));
            graph.AddEdge(new Edge<Coordinate>(topLeft, topRight));
            graph.AddEdge(new Edge<Coordinate>(topRight, bottomRight));

            var dijkstra =
                new DijkstraShortestPathAlgorithm<Coordinate, IEdge<Coordinate>>(graph, _ => 1);

            var predecessorObserver =
                new VertexPredecessorRecorderObserver<Coordinate, IEdge<Coordinate>>();
            predecessorObserver.Attach(dijkstra);

            dijkstra.Compute(start);

            var result = predecessorObserver.TryGetPath(end, out _);
        }

        [Fact]
        public void BuildRoadNetworkGraphFromWithNodeId()
        {
            var graph = BuildGraph();

            var start = 1992495;
            var ends = new[] { 315810, 1171123, 1419272, 2062984 };

            for (var i = 0; i < 5; ++i)
            {
                var result = HasPath(graph, start, ends);
                result.Should().BeTrue();
            }
        }

        private static bool HasPath(UndirectedGraph<int, IEdge<int>> graph, int start, int[] ends)
        {
            var tryGetPaths = graph.ShortestPathsDijkstra(_ => 1, start);

            return ends.All(end => tryGetPaths(end, out _));
        }

        private static UndirectedGraph<int, IEdge<int>> BuildGraph()
        {
            var graph = new UndirectedGraph<int, IEdge<int>>(true);

            var bytes = File.ReadAllBytes(@"C:\code\1861035");
            var roadNetwork = S3CacheSerializer.Serializer.DeserializeObject<RoadNetworkSnapshot>(bytes).Value;

            foreach (var node in roadNetwork.Nodes)
            {
                graph.AddVertex(node.Id);
            }

            foreach (var segment in roadNetwork.Segments)
            {
                var startVertex = segment.StartNodeId;
                var endVertex = segment.EndNodeId;

                graph.AddEdge(Comparer<int>.Default.Compare(startVertex, endVertex) > 0
                    ? new UndirectedEdge<int>(endVertex, startVertex)
                    : new UndirectedEdge<int>(startVertex, endVertex));
            }

            return graph;
        }
    }
}
