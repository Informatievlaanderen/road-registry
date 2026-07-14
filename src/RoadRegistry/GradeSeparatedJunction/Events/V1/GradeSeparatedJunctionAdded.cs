namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class GradeSeparatedJunctionAdded : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionAdded"; // BE CAREFUL CHANGING THIS!!

    public required int Id { get; set; }
    public required int LowerRoadSegmentId { get; set; }
    public required int TemporaryId { get; set; }
    public required string Type { get; set; }
    public required int UpperRoadSegmentId { get; set; }

    // The junction point (intersection of the two linked segments), computed by the Marten migration. Null when it
    // could not be computed.
    public JunctionGeometry? Geometry { get; set; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
