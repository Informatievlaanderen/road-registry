namespace RoadRegistry.RoadSegment.Events.V1.ValueObjects;

using System;

public class ImportedOriginProperties
{
    public required string Application { get; set; }
    public required string Operator { get; set; }
    public required string Organization { get; set; }
    public required string OrganizationId { get; set; }
    public required DateTime Since { get; set; }
    public required int TransactionId { get; set; }
}
