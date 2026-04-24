namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using NetTopologySuite.Geometries;

public class RoadSegmentsSpatialIndex<T>
{
    private readonly Dictionary<(int, int), List<(MultiLineString Geometry, T RoadSegment)>> _spatialIndex;

    private const double GridSize = 100.0;

    public RoadSegmentsSpatialIndex(IEnumerable<(MultiLineString Geometry, T RoadSegment)> segments)
    {
        _spatialIndex = new Dictionary<(int, int), List<(MultiLineString Geometry, T RoadSegment)>>();

        foreach (var segment in segments)
        {
            var envelope = segment.Geometry.EnvelopeInternal;
            var minGridX = (int)(envelope.MinX / GridSize);
            var maxGridX = (int)(envelope.MaxX / GridSize);
            var minGridY = (int)(envelope.MinY / GridSize);
            var maxGridY = (int)(envelope.MaxY / GridSize);

            for (var gx = minGridX; gx <= maxGridX; gx++)
            {
                for (var gy = minGridY; gy <= maxGridY; gy++)
                {
                    var key = (gx, gy);
                    if (!_spatialIndex.TryGetValue(key, out var list))
                    {
                        list = new List<(MultiLineString Geometry, T RoadSegment)>();
                        _spatialIndex[key] = list;
                    }
                    list.Add(segment);
                }
            }
        }
    }

    public IReadOnlyCollection<T> Query(Geometry geometry)
    {
        // Find candidate segments using spatial index
        var envelope = geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / GridSize);
        var maxGridX = (int)(envelope.MaxX / GridSize);
        var minGridY = (int)(envelope.MinY / GridSize);
        var maxGridY = (int)(envelope.MaxY / GridSize);

        var candidateSegments = new HashSet<T>();
        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (_spatialIndex.TryGetValue(key, out var list))
                {
                    foreach (var candidate in list)
                    {
                        candidateSegments.Add(candidate.RoadSegment);
                    }
                }
            }
        }

        return candidateSegments;
    }

    public void Add(MultiLineString geometry, T roadSegment)
    {
        var envelope = geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / GridSize);
        var maxGridX = (int)(envelope.MaxX / GridSize);
        var minGridY = (int)(envelope.MinY / GridSize);
        var maxGridY = (int)(envelope.MaxY / GridSize);

        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (!_spatialIndex.TryGetValue(key, out var list))
                {
                    list = new List<(MultiLineString, T)>();
                    _spatialIndex[key] = list;
                }
                list.Add((geometry, roadSegment));
            }
        }
    }

    public void Remove(MultiLineString geometry, T roadSegment)
    {
        var envelope = geometry.EnvelopeInternal;
        var minGridX = (int)(envelope.MinX / GridSize);
        var maxGridX = (int)(envelope.MaxX / GridSize);
        var minGridY = (int)(envelope.MinY / GridSize);
        var maxGridY = (int)(envelope.MaxY / GridSize);

        for (var gx = minGridX; gx <= maxGridX; gx++)
        {
            for (var gy = minGridY; gy <= maxGridY; gy++)
            {
                var key = (gx, gy);
                if (_spatialIndex.TryGetValue(key, out var list))
                {
                    list.Remove((geometry, roadSegment));
                }
            }
        }
    }
}
