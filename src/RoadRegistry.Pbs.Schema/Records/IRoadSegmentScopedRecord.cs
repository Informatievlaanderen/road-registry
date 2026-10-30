namespace RoadRegistry.Pbs.Schema.Records;

// The PBS read model is keyed on the road segment throughout: every table below Wegsegmenten carries the segment it
// belongs to in WS_OIDN, and every projection lookup is "the rows for this segment". Saying so in the type system is
// what lets those lookups be written once, generically - both the per-batch prefetch that loads many segments at a
// time and the per-segment read that serves them back out of the change tracker.
//
// Deliberately read-only: a record is placed in a segment when it is built, and nothing afterwards moves it to
// another one.
public interface IRoadSegmentScopedRecord
{
    int WS_OIDN { get; }
}
