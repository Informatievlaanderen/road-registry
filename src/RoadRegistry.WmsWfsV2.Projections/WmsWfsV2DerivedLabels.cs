namespace RoadRegistry.WmsWfsV2.Projections;

using System;

// Computes the denormalized street-name (STRNM) and maintainer-category (LBLBEHEER) labels that used to be produced by
// CASE expressions in the [wms].[Wegsegmenten] view. Keeping the logic here (instead of the view) lets the projection
// store the results in indexable columns; the projections call this both when a segment is (re)derived and when a street
// name / organization is renamed. String comparisons are case-insensitive to match the SQL Server default (CI) collation
// the view's LIKE/= comparisons ran under.
internal static class WmsWfsV2DerivedLabels
{
    // In-use road segments (STATUS = 11) are the only ones that carry a maintainer category label.
    private const int StatusInUse = 11;

    // STRNM: a single combined label for the left/right street names. Equal sides collapse to one name; otherwise the
    // present side(s) are prefixed with "L: " / "R: ".
    public static string? Strnm(int? leftStreetNameId, int? rightStreetNameId, string? leftName, string? rightName)
    {
        if (leftStreetNameId is not null && leftStreetNameId == rightStreetNameId)
        {
            return leftName;
        }
        if (leftName is null && rightName is null)
        {
            return null;
        }
        if (leftName is not null && rightName is null)
        {
            return $"L: {leftName}";
        }
        if (rightName is not null && leftName is null)
        {
            return $"R: {rightName}";
        }
        return $"L: {leftName} / R: {rightName}";
    }

    // LBLBEHEER: the maintainer category for an in-use segment, derived from the left/right maintainer codes and their
    // organization names.
    public static string? LblBeheer(int? status, string? leftBeheer, string? rightBeheer, string? leftOrgName, string? rightOrgName)
    {
        if (status != StatusInUse)
        {
            return null;
        }
        if (StartsWith(leftBeheer, "AWV") && StartsWith(rightBeheer, "AWV"))
        {
            return "AWV";
        }
        if (IsGemeenteOrStad(leftOrgName) && IsGemeenteOrStad(rightOrgName))
        {
            return "Steden en gemeenten";
        }
        if (Equals(leftBeheer, "PARTIC") && Equals(rightBeheer, "PARTIC"))
        {
            return "Particulieren";
        }
        return "Andere";
    }

    private static bool StartsWith(string? value, string prefix) => value is not null && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    private static bool Equals(string? value, string other) => string.Equals(value, other, StringComparison.OrdinalIgnoreCase);

    private static bool IsGemeenteOrStad(string? name) => StartsWith(name, "Gemeente") || StartsWith(name, "Stad");
}
