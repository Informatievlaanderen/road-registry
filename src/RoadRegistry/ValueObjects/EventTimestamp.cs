namespace RoadRegistry.ValueObjects;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NodaTime;

public sealed record EventTimestamp(Instant Timestamp, OrganizationId OrganizationId);

public static class EventTimestampExtensions
{
    public static EventTimestamp ToEventTimestamp(this ProvenanceData provenance)
    {
        return new EventTimestamp(provenance.Timestamp, !string.IsNullOrEmpty(provenance.Operator) ? new OrganizationId(provenance.Operator) : OrganizationId.DigitaalVlaanderen);
    }
}
