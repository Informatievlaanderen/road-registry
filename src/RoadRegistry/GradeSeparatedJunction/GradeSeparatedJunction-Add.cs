namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadRegistry.ValueObjects.Problems;
using ScopedRoadNetwork;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator)
    {
        var problems = Problems.For(change.TemporaryId);

        var junction = Create(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            OriginalId = change.TemporaryId,
            LowerRoadSegmentId = change.LowerRoadSegmentId,
            UpperRoadSegmentId = change.UpperRoadSegmentId,
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return (junction, problems);
    }
}
