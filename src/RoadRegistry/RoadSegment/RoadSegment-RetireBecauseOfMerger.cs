namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class RoadSegment
{
    public Problems RetireBecauseOfMerger(RoadSegmentId mergedRoadSegmentId, Provenance provenance)
    {
        //TODO-pr enkel voor measured segments? if yes, add validation + unit test

        Apply(new RoadSegmentWasRetiredBecauseOfMerger
        {
            RoadSegmentId = RoadSegmentId,
            MergedRoadSegmentId = mergedRoadSegmentId,
            Provenance = new ProvenanceData(provenance)
        });

        return Problems.None;
    }
}
