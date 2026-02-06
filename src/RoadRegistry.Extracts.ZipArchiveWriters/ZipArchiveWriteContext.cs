namespace RoadRegistry.Extracts.ZipArchiveWriters;

public class ZipArchiveWriteContext
{
    private readonly Dictionary<RoadSegmentId, List<(RoadSegmentId, RoadSegmentGeometry)>> _roadSegmentTempIds = new();
    private RoadSegmentId _nextTempRoadSegmentId = new(1);
    private RoadNodeId _nextSchijnknoopId = new(5000000);

    public RoadSegmentId NewTempId(RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry)
    {
        var tempId = _nextTempRoadSegmentId;

        _roadSegmentTempIds.TryAdd(roadSegmentId, []);
        _roadSegmentTempIds[roadSegmentId].Add((tempId, geometry));

        _nextTempRoadSegmentId = _nextTempRoadSegmentId.Next();
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

    public RoadNodeId NewSchijnknoopId()
    {
        var nodeId = _nextSchijnknoopId;
        _nextSchijnknoopId = _nextSchijnknoopId.Next();
        return nodeId;
    }
}
