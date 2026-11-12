namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// An 'ingeschetst' road segment was measured and now counts as 'ingemeten'. The geometry draw method it takes on is
// the event itself, so it is not carried as a field.
//
// Nothing else about the segment changes: the geometry, the status and every attribute stay exactly as they were.
public record RoadSegmentWasMeasured : IMartenEvent
{
    public const string EventName = "RoadSegmentWasMeasured"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
