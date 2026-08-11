namespace RoadRegistry.RoadSegment;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // Records that this segment is no longer realized and has come loose from the road nodes it hung off. The caller
    // has already established that it is realized, which is what guarantees both node identifiers are there.
    public Problems CorrectFromRealizedToPlanned(ScopedRoadNetworkChangeContext context)
    {
        Apply(new RoadSegmentWasCorrectedFromRealizedToPlanned
        {
            RoadSegmentId = RoadSegmentId,
            PreviousStartNodeId = StartNodeId!.Value,
            PreviousEndNodeId = EndNodeId!.Value,
            Provenance = new ProvenanceData(context.Provenance)
        });

        return Problems.None;
    }
}
