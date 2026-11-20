namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// The counterpart of RoadNodeRemovalWasCorrectedBecauseOfMigration, for the road segment that ran to the road node.
// Inwinning runs per municipality, and two neighbours are collected independently of one another. A road segment that
// crosses the border is in both extracts, and the municipality that uploads first may remove it. The neighbour was
// prepared against the extract from before that, and still brings the road segment - one it is entitled to collect,
// since it lies on its own side as well.
//
// The road segment comes back under the identifier it always had: it is the same road segment, and everything that
// refers to it by number - the crossings, the road numbers, the inwinning that is waiting on it - has to keep pointing
// at it.
//
// This event says no more than that the removal was undone, deliberately: the road segment is collected again right
// after it, so the RoadSegmentWasMigrated that follows carries the geometry, the status, the road nodes and every
// attribute. A projection that had to delete its row recreates it from that, with real values instead of a placeholder
// it would have to invent here.
public record RoadSegmentRemovalWasCorrectedBecauseOfMigration : IMartenEvent
{
    public const string EventName = "RoadSegmentRemovalWasCorrectedBecauseOfMigration"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
