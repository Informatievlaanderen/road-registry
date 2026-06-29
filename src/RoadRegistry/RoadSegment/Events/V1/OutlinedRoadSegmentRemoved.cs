namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class OutlinedRoadSegmentRemoved : IMartenEvent
{
    public const string EventName = "OutlinedRoadSegmentRemoved"; // BE CAREFUL CHANGING THIS!!

    public required int RoadSegmentId { get; set; }

    public required ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
