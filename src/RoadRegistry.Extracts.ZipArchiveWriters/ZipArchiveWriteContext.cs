namespace RoadRegistry.Extracts.ZipArchiveWriters;

public class ZipArchiveWriteContext
{
    private readonly Dictionary<RoadSegmentId, List<RoadSegmentId>> _roadSegmentTempIds = new();

    private RoadSegmentId _latestTempRoadSegmentId = new(1);

    public ZipArchiveWriteContext()
    {
    }

    public RoadSegmentId NewTempId(RoadSegmentId roadSegmentId)
    {
        var tempId = _latestTempRoadSegmentId;

        _roadSegmentTempIds.TryAdd(roadSegmentId, []);
        _roadSegmentTempIds[roadSegmentId].Add(tempId);

        _latestTempRoadSegmentId = _latestTempRoadSegmentId.Next();
        return tempId;
    }

    public IReadOnlyCollection<RoadSegmentId> GetTempIds(RoadSegmentId roadSegmentId)
    {
        if (_roadSegmentTempIds.TryGetValue(roadSegmentId, out var tempIds))
        {
            return tempIds;
        }

        throw new InvalidOperationException();
    }
}
