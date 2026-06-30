namespace RoadRegistry.ScopedRoadNetwork.Events.V1;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadNetworkChangesAccepted : IMartenEvent
{
    public const string EventName = "RoadNetworkChangesAccepted"; // BE CAREFUL CHANGING THIS!!

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

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
