namespace RoadRegistry.BackOffice.Core;

public interface IRequestedChangeIdentityTranslator
{
    RoadNodeId TranslateToTemporaryOrId(RoadNodeId id);
    RoadSegmentId TranslateToTemporaryOrId(RoadSegmentId id);
    RoadSegmentId TranslateToOriginalOrId(RoadSegmentId id);
    GradeSeparatedJunctionId TranslateToTemporaryOrId(GradeSeparatedJunctionId id);
    bool TryTranslateToPermanent(RoadNodeId id, out RoadNodeId permanent);
    bool TryTranslateToPermanent(RoadSegmentId id, out RoadSegmentId permanent);
    bool TryTranslateToPermanent(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary);
    bool TryTranslateToTemporary(RoadNodeId id, out RoadNodeId temporary);
    bool TryTranslateToTemporary(RoadSegmentId id, out RoadSegmentId temporary);
    bool TryTranslateToOriginal(RoadSegmentId id, out RoadSegmentId originalId);
    bool TryTranslateToTemporary(GradeSeparatedJunctionId id, out GradeSeparatedJunctionId temporary);
}
