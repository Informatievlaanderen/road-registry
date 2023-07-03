namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentLaneAttributeRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentLaneAttributeRequest>
{
    public bool Equals(ChangeRoadSegmentLaneAttributeRequest x, ChangeRoadSegmentLaneAttributeRequest y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
        {
            return false;
        }

        return x.FromPosition.Equals(y.FromPosition)
               && x.ToPosition.Equals(y.ToPosition)
               && x.Count.Equals(y.Count)
               && x.Direction.Equals(y.Direction)
            ;
    }

    public int GetHashCode(ChangeRoadSegmentLaneAttributeRequest obj)
    {
        throw new NotSupportedException();
    }
}
