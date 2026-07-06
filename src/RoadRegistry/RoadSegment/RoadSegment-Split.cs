namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RetireBecauseOfSplit(Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        Apply(new RoadSegmentWasRetiredBecauseOfSplit
        {
            RoadSegmentId = RoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }

    public Problems Split(IReadOnlyCollection<RoadSegmentId> newRoadSegmentIds, RoadSegmentSplitModifications? modifications, Provenance provenance)
    {
        var problems = Problems.WithContext(RoadSegmentId);

        Apply(new RoadSegmentWasSplit
        {
            RoadSegmentId = RoadSegmentId,
            NewRoadSegmentIds = newRoadSegmentIds.ToArray(),
            Modifications = modifications,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
