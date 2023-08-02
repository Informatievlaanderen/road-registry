namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentsRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentsDynamicAttributesRequest>
{
    public bool Equals(ChangeRoadSegmentsDynamicAttributesRequest x, ChangeRoadSegmentsDynamicAttributesRequest y)
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

    public int GetHashCode(ChangeRoadSegmentsDynamicAttributesRequest obj)
    {
        throw new NotSupportedException();
    }
}
