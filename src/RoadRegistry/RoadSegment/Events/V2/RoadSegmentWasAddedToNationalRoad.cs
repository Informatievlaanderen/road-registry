namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record RoadSegmentWasAddedToNationalRoad : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }

    public required ProvenanceData Provenance { get; init; }
}
