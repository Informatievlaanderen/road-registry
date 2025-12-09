namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadSegmentStreetNamesChanged : IMartenEvent
{
    public required string GeometryDrawMethod { get; set; }
    public required int Id { get; set; }
    public required int Version { get; set; }
    public required int? LeftSideStreetNameId { get; set; }
    public required int? RightSideStreetNameId { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
