namespace RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;

/// <summary>
/// The old road registry had a single maintenance authority per road segment, taken over on both sides by the new one.
/// A road segment on a municipality border mostly has its left street name in one municipality and its right street name
/// in the other, while both of its sides got the maintenance authority of just one of them.
///
/// The street names lead: when the left street name lies in municipality A, the right one in municipality B, and the
/// maintenance authority on each side is A or B, the left side becomes A and the right side B. A maintenance authority
/// that is neither of both municipalities is left alone. The organization code of a municipality is its NIS code.
/// </summary>
public static class MaintenanceAuthorityByStreetNameMunicipality
{
    public static bool TryCorrect(
        string? leftStreetNameNisCode,
        string? rightStreetNameNisCode,
        OrganizationId leftMaintenanceAuthorityId,
        OrganizationId rightMaintenanceAuthorityId,
        out OrganizationId correctedLeftMaintenanceAuthorityId,
        out OrganizationId correctedRightMaintenanceAuthorityId)
    {
        correctedLeftMaintenanceAuthorityId = leftMaintenanceAuthorityId;
        correctedRightMaintenanceAuthorityId = rightMaintenanceAuthorityId;

        if (string.IsNullOrWhiteSpace(leftStreetNameNisCode)
            || string.IsNullOrWhiteSpace(rightStreetNameNisCode)
            || leftStreetNameNisCode == rightStreetNameNisCode)
        {
            return false;
        }

        var municipalityA = new OrganizationId(leftStreetNameNisCode);
        var municipalityB = new OrganizationId(rightStreetNameNisCode);

        if (!IsEither(leftMaintenanceAuthorityId, municipalityA, municipalityB)
            || !IsEither(rightMaintenanceAuthorityId, municipalityA, municipalityB))
        {
            return false;
        }

        if (leftMaintenanceAuthorityId == municipalityA && rightMaintenanceAuthorityId == municipalityB)
        {
            return false;
        }

        correctedLeftMaintenanceAuthorityId = municipalityA;
        correctedRightMaintenanceAuthorityId = municipalityB;
        return true;
    }

    private static bool IsEither(OrganizationId maintenanceAuthorityId, OrganizationId municipalityA, OrganizationId municipalityB)
    {
        return maintenanceAuthorityId == municipalityA || maintenanceAuthorityId == municipalityB;
    }
}
