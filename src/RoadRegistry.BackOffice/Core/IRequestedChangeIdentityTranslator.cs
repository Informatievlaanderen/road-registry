namespace RoadRegistry.BackOffice.Core;

public interface IRequestedChangeIdentityTranslator
{
    RoadNodeId TranslateToTemporaryOrId(RoadNodeId id);
    RoadSegmentId TranslateToOriginalOrTemporaryOrId(RoadSegmentId id);
    GradeSeparatedJunctionId TranslateToTemporaryOrId(GradeSeparatedJunctionId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
    bool TryTranslateToPermanent(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary);
    bool TryTranslateToTemporary(RoadNodeId id, out RoadNodeId temporary);
    bool TryTranslateToTemporary(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary);
    bool IsSegmentAdded(RoadSegmentId id);
}
