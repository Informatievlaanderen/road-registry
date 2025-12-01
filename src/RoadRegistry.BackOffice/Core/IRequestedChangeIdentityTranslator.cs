namespace RoadRegistry.BackOffice.Core;

using RoadRegistry.RoadSegment.ValueObjects;

public interface IRequestedChangeIdentityTranslator
{
    RoadNodeId TranslateToTemporaryOrId(RoadNodeId id);
    RoadSegmentId TranslateToOriginalOrTemporaryOrId(RoadSegmentId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
    bool IsSegmentAdded(RoadSegmentId id);
}
