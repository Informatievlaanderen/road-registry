namespace RoadRegistry.RoadSegment.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using RoadRegistry.ValueObjects;

public record RoadSegmentGeometryDrawMethodWasChanged : IMartenEvent
{
    public const string EventName = "RoadSegmentGeometryDrawMethodWasChanged"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometryDrawMethodV2 GeometryDrawMethod { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
