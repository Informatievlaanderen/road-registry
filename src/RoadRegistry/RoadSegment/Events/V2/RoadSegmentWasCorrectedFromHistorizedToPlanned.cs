namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// A 'gehistoreerd' road segment was corrected and counts as 'gepland' again.
//
// Neither status knots the segment into the network, so nothing but the status moves: the segment carried no road
// nodes before and carries none after, and its geometry and every attribute stay exactly as they were. The status it
// takes on is the event itself, so it is not carried as a field.
public record RoadSegmentWasCorrectedFromHistorizedToPlanned : IRoadSegmentUnconnectedStatusChangeEvent
{
    public const string EventName = "RoadSegmentWasCorrectedFromHistorizedToPlanned"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
