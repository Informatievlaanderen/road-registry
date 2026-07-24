namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public Problems Modify(ModifyGradeSeparatedJunctionChange change, Provenance provenance)
    {
        var problems = Problems.WithContext(GradeSeparatedJunctionId);

        // A modification must change at least one road segment (or the type); with none of them set there is nothing to change.
        if (change.LowerRoadSegmentId is null && change.UpperRoadSegmentId is null && change.Type is null)
        {
            return problems + new GradeSeparatedJunctionNoRoadSegmentSpecified(GradeSeparatedJunctionId);
        }

        Apply(new GradeSeparatedJunctionWasModified
        {
            GradeSeparatedJunctionId = GradeSeparatedJunctionId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return problems;
    }
}
