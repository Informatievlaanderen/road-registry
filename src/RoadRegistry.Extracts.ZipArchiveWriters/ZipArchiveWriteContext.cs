namespace RoadRegistry.Extracts.ZipArchiveWriters;

using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
using RoadRegistry.Extracts.Projections;

public class ZipArchiveWriteContext
{
    private readonly Dictionary<RoadSegmentId, List<(RoadSegmentId, RoadSegmentGeometry)>> _roadSegmentTempIds = new();
    private RoadSegmentId _nextTempRoadSegmentId = new(1);
    private RoadNodeId _nextSchijnknoopId = RoadNodeConstants.InitialTemporarySchijnknoopId;

    public List<RoadSegmentExtractItem>? IntegrationSegments { get; set; }

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

        throw new InvalidOperationException($"No temp ids were registered for road segment id {roadSegmentId}");
    }

    public RoadNodeId NewSchijnknoopId()
    {
        var nodeId = _nextSchijnknoopId;
        _nextSchijnknoopId = _nextSchijnknoopId.Next();
        return nodeId;
    }
}
