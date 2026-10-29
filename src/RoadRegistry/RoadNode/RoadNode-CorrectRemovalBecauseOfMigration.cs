namespace RoadRegistry.RoadNode;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadNode
{
    // Brings a road node a neighbouring inwinning removed back, under the identifier it always had. Whether that is
    // the right thing to do is the migration's to work out - see ResolveRemovalCorrections - and this says what has
    // to hold for the answer to have been that: only a removal by an inwinning can be undone.
    public Problems CorrectRemovalBecauseOfMigration(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadNodeId);

        if (!IsRemoved)
        {
            throw new InvalidOperationException($"Road node {RoadNodeId} is not removed, so its removal cannot be corrected.");
        }

        if (!IsRemovedBecauseOfMigration)
        {
            throw new InvalidOperationException($"Road node {RoadNodeId} was not removed by an inwinning, so its removal cannot be corrected.");
        }

        Apply(new RoadNodeRemovalWasCorrectedBecauseOfMigration
        {
            RoadNodeId = RoadNodeId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
