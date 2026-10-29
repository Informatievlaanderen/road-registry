namespace RoadRegistry.RoadNode.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// Inwinning runs per municipality, and two neighbours are collected independently of one another. A road node that sits
// just across the border is in both extracts, and the municipality that uploads first may remove it along with the road
// segment that ran to it. The neighbour was prepared against the extract from before that, and still brings the road
// node together with a road segment of its own that runs to it - which is a road it is entitled to collect.
//
// The road node comes back under the identifier it always had: it is the same road node, and everything that refers to
// it by number has to keep pointing at it.
//
// This event says no more than that the removal was undone, deliberately: the road node is collected again right after
// it, so the RoadNodeWasMigrated that follows carries the geometry and the grensknoop, and the RoadNodeTypeWasChanged
// after that carries the type. A projection that had to delete its row recreates it from those, with real values
// instead of a placeholder it would have to invent here.
public record RoadNodeRemovalWasCorrectedBecauseOfMigration : IMartenEvent
{
    public const string EventName = "RoadNodeRemovalWasCorrectedBecauseOfMigration"; // BE CAREFUL CHANGING THIS!!

    public required RoadNodeId RoadNodeId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
