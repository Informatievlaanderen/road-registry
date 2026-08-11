namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// A 'gerealiseerd' road segment was corrected back to 'gepland' and with that unhooked from the network. The status
// it takes on is the event itself, so it is not carried as a field.
//
// Only a realized segment is knotted in, so the segment gives up its road nodes here. The nodes it hung off are named
// so a reader can tell what it came loose from; whether those nodes themselves survived is their own business and is
// recorded on them.
//
// Nothing else about the segment changes: the geometry and every attribute stay exactly as they were.
public record RoadSegmentWasCorrectedFromRealizedToPlanned : IMartenEvent
{
    public const string EventName = "RoadSegmentWasCorrectedFromRealizedToPlanned"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadNodeId PreviousStartNodeId { get; init; }
    public required RoadNodeId PreviousEndNodeId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
