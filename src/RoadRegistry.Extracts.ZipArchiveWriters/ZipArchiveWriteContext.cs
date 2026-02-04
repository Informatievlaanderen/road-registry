namespace RoadRegistry.Extracts.ZipArchiveWriters;

public class ZipArchiveWriteContext
{
    private readonly Dictionary<RoadSegmentId, List<(RoadSegmentId, RoadSegmentGeometry)>> _roadSegmentTempIds = new();
    private RoadSegmentId _latestTempRoadSegmentId = new(1);

    public RoadSegmentId NewTempId(RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry)
    {
        var tempId = _latestTempRoadSegmentId;

        _roadSegmentTempIds.TryAdd(roadSegmentId, []);
        _roadSegmentTempIds[roadSegmentId].Add((tempId, geometry));

        _latestTempRoadSegmentId = _latestTempRoadSegmentId.Next();
        return tempId;
    }

    public IReadOnlyCollection<(RoadSegmentId Id, RoadSegmentGeometry Geometry)> GetTempSegments(RoadSegmentId roadSegmentId)
    {
        if (_roadSegmentTempIds.TryGetValue(roadSegmentId, out var tempIds))
        {
            return tempIds;
        }

        throw new InvalidOperationException();
    }
}
