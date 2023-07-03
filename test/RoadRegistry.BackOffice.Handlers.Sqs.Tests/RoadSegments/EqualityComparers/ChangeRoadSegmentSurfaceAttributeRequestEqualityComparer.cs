namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentSurfaceAttributeRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentSurfaceAttributeRequest>
{
    public bool Equals(ChangeRoadSegmentSurfaceAttributeRequest x, ChangeRoadSegmentSurfaceAttributeRequest y)
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
               && x.Type.Equals(y.Type)
            ;
    }

    public int GetHashCode(ChangeRoadSegmentSurfaceAttributeRequest obj)
    {
        throw new NotSupportedException();
    }
}
