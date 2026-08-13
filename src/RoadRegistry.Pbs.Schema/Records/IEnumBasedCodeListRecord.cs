namespace RoadRegistry.Pbs.Schema.Records;

// Marks the enum-based code lists that PbsCodeListSyncService syncs from the V2 domain types instead of being
// projected from events. A projection rebuild leaves these tables alone and truncates everything else, and the
// sync service only accepts records carrying this marker - so a newly added code list is either synced (marker,
// spared by the rebuild) or event-driven (no marker, truncated and replayed), never silently lost.
public interface IEnumBasedCodeListRecord;
