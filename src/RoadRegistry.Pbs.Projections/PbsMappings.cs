namespace RoadRegistry.Pbs.Projections;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

internal static class PbsMappings
{
    // CREATIE / VERSIE are 15-char strings in the product; we render the provenance timestamp as an ISO date.
    public static string ToPbsDate(this ProvenanceData provenance)
    {
        return provenance.Timestamp.ToDateTimeUtc().ToString("yyyy-MM-dd");
    }
}
