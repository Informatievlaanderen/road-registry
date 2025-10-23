namespace RoadRegistry.BackOffice.Core;

using RoadSegment.ValueObjects;

public interface IRequestedChangeIdentityTranslator : IRoadNetworkChangeIdentityTranslator
{
    bool IsSegmentAdded(RoadSegmentId id);
}
