namespace RoadRegistry.RoadSegment.Events;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public record RoadSegmentRemovedFromNationalRoad : IMartenEvent
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required NationalRoadNumber Number { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public RoadSegmentRemovedFromNationalRoad()
    {
    }
    protected RoadSegmentRemovedFromNationalRoad(RoadSegmentRemovedFromNationalRoad other) // Needed for Marten
    {
    }
}
