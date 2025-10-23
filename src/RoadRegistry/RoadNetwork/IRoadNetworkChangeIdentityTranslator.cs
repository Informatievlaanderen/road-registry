namespace RoadRegistry.BackOffice.Core;

using RoadSegment.ValueObjects;

public interface IRoadNetworkChangeIdentityTranslator
{
    RoadNodeId TranslateToTemporaryOrId(RoadNodeId id);
    RoadSegmentId TranslateToOriginalOrTemporaryOrId(RoadSegmentId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
}
