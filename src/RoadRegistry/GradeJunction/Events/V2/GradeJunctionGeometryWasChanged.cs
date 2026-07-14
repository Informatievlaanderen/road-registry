namespace RoadRegistry.GradeJunction.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public record GradeJunctionGeometryWasChanged : IMartenEvent
{
    public const string EventName = "GradeJunctionGeometryWasChanged"; // BE CAREFUL CHANGING THIS!!

    public required GradeJunctionId GradeJunctionId { get; init; }
    public required JunctionGeometry Geometry { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
