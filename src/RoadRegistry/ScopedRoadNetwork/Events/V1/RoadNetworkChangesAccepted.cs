namespace RoadRegistry.ScopedRoadNetwork.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadNetworkChangesAccepted : IMartenEvent
{
    public required string Operator { get; set; }
    public required string Organization { get; set; }
    public required string OrganizationId { get; set; }
    public required string Reason { get; set; }
    public required string RequestId { get; set; }
    public required Guid? DownloadId { get; set; }
    public required int TransactionId { get; set; }
    public required Guid? TicketId { get; set; }
    public required DateTimeOffset When {get; set; }

    public required ProvenanceData Provenance { get; set; }
}
