namespace RoadRegistry.WmsWfsV2.Projections;

using System;

internal static class WmsWfsV2DerivedLabels
{
    public static string? BuildStreetNameLabel(int? leftStreetNameId, int? rightStreetNameId, string? leftName, string? rightName)
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

    public static string? BuildMaintainerCategoryLabel(int? status, string? leftBeheer, string? rightBeheer, string? leftOrgName, string? rightOrgName)
    {
        if (status != RoadSegmentStatusV2.Gerealiseerd.Translation.Identifier)
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
