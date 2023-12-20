namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.RoadSegments.EqualityComparers;

using RoadRegistry.BackOffice.Abstractions.RoadSegments;

public class ChangeRoadSegmentRequestEqualityComparer : IEqualityComparer<ChangeRoadSegmentDynamicAttributesRequest>
{
    public bool Equals(ChangeRoadSegmentDynamicAttributesRequest x, ChangeRoadSegmentDynamicAttributesRequest y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (ReferenceEquals(x, null) || ReferenceEquals(y, null) || x.GetType() != y.GetType())
        {
            return false;
        }
        
        return x.Id.Equals(y.Id)
               && (x.Lanes ?? Array.Empty<ChangeRoadSegmentLaneAttributeRequest>())
               .SequenceEqual(y.Lanes ?? Array.Empty<ChangeRoadSegmentLaneAttributeRequest>(), new ChangeRoadSegmentLaneAttributeRequestEqualityComparer())
               && (x.Surfaces ?? Array.Empty<ChangeRoadSegmentSurfaceAttributeRequest>())
               .SequenceEqual(y.Surfaces ?? Array.Empty<ChangeRoadSegmentSurfaceAttributeRequest>(), new ChangeRoadSegmentSurfaceAttributeRequestEqualityComparer())
               && (x.Widths ?? Array.Empty<ChangeRoadSegmentWidthAttributeRequest>())
               .SequenceEqual(y.Widths ?? Array.Empty<ChangeRoadSegmentWidthAttributeRequest>(), new ChangeRoadSegmentWidthAttributeRequestEqualityComparer())
            ;
    }

    public int GetHashCode(ChangeRoadSegmentDynamicAttributesRequest obj)
    {
        throw new NotSupportedException();
    }
}
