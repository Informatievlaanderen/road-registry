namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Quadtree;
using NetTopologySuite.Operation.Union;
using RoadRegistry.Extensions;

internal static class RoadNetworkChangesScope
{
    public static MultiPolygon? Build(IReadOnlyCollection<Geometry> geometries)
    {
        if (!geometries.Any())
        {
            return null;
        }

        // Radical optimization: Work with envelopes only, defer expensive geometry operations
        var envelopes = geometries.Select(x => x.EnvelopeInternal).ToList();

        // Step 2: Merge overlapping envelopes using union-find approach (O(n log n))
        var mergedEnvelopes = MergeOverlappingEnvelopes(envelopes);

        // Step 3: Convert merged envelopes to buffered polygons (only for final result)
        var factory = geometries.First().Factory;

        var scopes = mergedEnvelopes
            .Select(env =>
            {
                // Create polygon from envelope and buffer it
                var coords = new[]
                {
                    new Coordinate(env.MinX, env.MinY),
                    new Coordinate(env.MaxX, env.MinY),
                    new Coordinate(env.MaxX, env.MaxY),
                    new Coordinate(env.MinX, env.MaxY),
                    new Coordinate(env.MinX, env.MinY)
                };
                var ring = factory.CreateLinearRing(coords);
                var poly = factory.CreatePolygon(ring);
                return (Polygon)poly.Buffer(1);
            })
            .ToList();

        var mp = MergeOverlappingScopes(geometries, scopes);
        return mp;
    }

    private static List<Envelope> MergeOverlappingEnvelopes(List<Envelope> envelopes)
    {
        if (envelopes.Count == 0)
        {
            return [];
        }

        // Step 1: Build spatial index once (read-only, thread-safe)
        var spatialIndex = new Quadtree<int>();
        for (var i = 0; i < envelopes.Count; i++)
        {
            var searchEnv = new Envelope(envelopes[i]);
            searchEnv.ExpandBy(2.0);
            spatialIndex.Insert(searchEnv, i);
        }

        // Step 2: Find overlapping pairs in parallel (read-only queries)
        var overlappingPairs = new ConcurrentBag<(int, int)>();

        Parallel.For(0, envelopes.Count, i =>
        {
            var searchEnv = new Envelope(envelopes[i]);
            searchEnv.ExpandBy(2.0);

            var candidates = spatialIndex.Query(searchEnv);
            foreach (var j in candidates)
            {
                if (i < j) // Avoid duplicates
                {
                    var env1 = envelopes[i];
                    var env2 = new Envelope(envelopes[j]);
                    env2.ExpandBy(2.0);

                    if (env1.Intersects(env2))
                    {
                        overlappingPairs.Add((i, j));
                    }
                }
            }
        });

        // Step 3: Use union-find to group connected envelopes
        var unionFind = new UnionFind(envelopes.Count);
        foreach (var (i, j) in overlappingPairs)
        {
            unionFind.Union(i, j);
        }

        // Step 4: Group envelopes by their root
        var groups = new Dictionary<int, List<int>>();
        for (var i = 0; i < envelopes.Count; i++)
        {
            var root = unionFind.Find(i);
            if (!groups.ContainsKey(root))
            {
                groups[root] = new List<int>();
            }

            groups[root].Add(i);
        }

        // Step 5: Merge envelopes within each group in parallel
        var mergedEnvelopes = groups.Values
            .AsParallel()
            .Select(group =>
            {
                var merged = new Envelope(envelopes[group[0]]);
                for (var i = 1; i < group.Count; i++)
                {
                    merged.ExpandToInclude(envelopes[group[i]]);
                }

                return merged;
            })
            .ToList();

        return mergedEnvelopes;
    }

    private static MultiPolygon MergeOverlappingScopes(IReadOnlyCollection<Geometry> geometries, List<Polygon> scopes)
    {
        var factory = scopes[0].Factory;
        var geom = factory.BuildGeometry(scopes);
        var unioned = UnaryUnionOp.Union(geom);

        // Keep separate islands as polygons
        var mergedPolys = new List<Polygon>();
        for (var i = 0; i < unioned.NumGeometries; i++)
            if (unioned.GetGeometryN(i) is Polygon p)
                mergedPolys.Add(p);

        return new MultiPolygon(mergedPolys.ToArray(), geometries.First().Factory)
            .WithSrid(geometries.First().SRID);
    }

    // Simple Union-Find data structure for grouping overlapping envelopes
    private sealed class UnionFind
    {
        private readonly int[] _parent;
        private readonly int[] _rank;

        public UnionFind(int size)
        {
            _parent = new int[size];
            _rank = new int[size];
            for (var i = 0; i < size; i++)
            {
                _parent[i] = i;
                _rank[i] = 0;
            }
        }

        public int Find(int x)
        {
            if (_parent[x] != x)
            {
                _parent[x] = Find(_parent[x]); // Path compression
            }

            return _parent[x];
        }

        public void Union(int x, int y)
        {
            var rootX = Find(x);
            var rootY = Find(y);

            if (rootX == rootY) return;

            // Union by rank
            if (_rank[rootX] < _rank[rootY])
            {
                _parent[rootX] = rootY;
            }
            else if (_rank[rootX] > _rank[rootY])
            {
                _parent[rootY] = rootX;
            }
            else
            {
                _parent[rootY] = rootX;
                _rank[rootX]++;
            }
        }
    }
}
