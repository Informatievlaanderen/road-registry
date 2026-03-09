namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

public interface IRoadSegmentIdProvider
{
    RoadSegmentId NewId();
}

public sealed class NextRoadSegmentIdProvider : IRoadSegmentIdProvider
{
    private RoadSegmentId _nextValue;

    public NextRoadSegmentIdProvider(RoadSegmentId initialValue)
    {
        _nextValue = initialValue.Next();
    }

    public RoadSegmentId NewId()
    {
        var result = _nextValue;
        _nextValue = _nextValue.Next();
        return result;
    }
}
