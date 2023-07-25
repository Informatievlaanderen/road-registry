namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentsRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentsRequest>
{
    public bool Equals(ChangeRoadSegmentsRequest x, ChangeRoadSegmentsRequest y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
        {
            return false;
        }

        return x.ChangeRequests.SequenceEqual(y.ChangeRequests, new ChangeRoadSegmentRequestEqualityComparer());
    }

    public int GetHashCode(ChangeRoadSegmentsRequest obj)
    {
        throw new NotSupportedException();
    }
}
