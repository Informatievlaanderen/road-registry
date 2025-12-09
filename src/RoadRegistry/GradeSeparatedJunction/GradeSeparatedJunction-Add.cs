namespace RoadRegistry.GradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Changes;
using Events.V2;
using RoadNetwork;
using RoadRegistry.ValueObjects.Problems;

public partial class GradeSeparatedJunction
{
    public static (GradeSeparatedJunction?, Problems) Add(AddGradeSeparatedJunctionChange change, Provenance provenance, IRoadNetworkIdGenerator idGenerator, IIdentifierTranslator idTranslator)
    {
        var problems = Problems.None;

        var junction = Create(new GradeSeparatedJunctionAdded
        {
            GradeSeparatedJunctionId = idGenerator.NewGradeSeparatedJunctionId(),
            OriginalId = change.TemporaryId,
            LowerRoadSegmentId = idTranslator.TranslateToPermanentId(change.LowerRoadSegmentId),
            UpperRoadSegmentId = idTranslator.TranslateToPermanentId(change.UpperRoadSegmentId),
            Type = change.Type,
            Provenance = new ProvenanceData(provenance)
        });

        return (junction, problems);
    }
}
