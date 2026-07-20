namespace RoadRegistry.WmsWfsV2.Projections;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

internal static class WmsWfsV2Mappings
{
    // CREATIE / VERSIE are 15-char strings in the product; we render the provenance timestamp as an ISO date.
    public static string ToWmsWfsV2Date(this ProvenanceData provenance)
    {
        return provenance.Timestamp.ToDateTimeUtc().ToString("yyyy-MM-dd");
    }
}
