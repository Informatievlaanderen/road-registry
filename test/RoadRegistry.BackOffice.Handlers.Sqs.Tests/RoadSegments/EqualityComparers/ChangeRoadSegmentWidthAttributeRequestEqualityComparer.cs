namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentWidthAttributeRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentWidthAttributeRequest>
{
    public bool Equals(ChangeRoadSegmentWidthAttributeRequest x, ChangeRoadSegmentWidthAttributeRequest y)
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
               && x.Width.Equals(y.Width)
            ;
    }

    public int GetHashCode(ChangeRoadSegmentWidthAttributeRequest obj)
    {
        throw new NotSupportedException();
    }
}
