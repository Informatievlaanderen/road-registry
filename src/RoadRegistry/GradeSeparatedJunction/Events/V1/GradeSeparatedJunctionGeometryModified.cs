namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// V1 (legacy) variant of GradeSeparatedJunctionGeometryWasChanged, produced by the Marten migration when a linked road
// segment's geometry changes. Carries the junction point in Lambert72 (the legacy CRS); only the aggregate state and the
// V2 events are in Lambert08.
public class GradeSeparatedJunctionGeometryModified : IMartenEvent
{
    public const string EventName = "GradeSeparatedJunctionGeometryModified"; // BE CAREFUL CHANGING THIS!!

    public required int Id { get; set; }
    public JunctionGeometry? Geometry { get; set; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
