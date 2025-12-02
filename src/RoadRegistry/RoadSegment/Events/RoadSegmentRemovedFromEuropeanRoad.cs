namespace RoadRegistry.RoadSegment.Events;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record RoadSegmentRemovedFromEuropeanRoad : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required EuropeanRoadNumber Number { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public RoadSegmentRemovedFromEuropeanRoad()
    {
    }
    protected RoadSegmentRemovedFromEuropeanRoad(RoadSegmentRemovedFromEuropeanRoad other) // Needed for Marten
    {
    }
}
