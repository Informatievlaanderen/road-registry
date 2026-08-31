namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using System;
using System.Linq;
using Authentication;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Http;

public static class IdentityExtensions
{
    public static string? GetOperator(this HttpContext httpContext)
    {
        return httpContext.FindOrgCodeClaim() ?? httpContext.User.FindFirst("operator")?.Value;
    }

    public static bool IsDigitaalVlaanderen(this HttpContext httpContext)
    {
        var ovoCode = httpContext.FindOvoCodeClaim();

        return ovoCode is not null
               && string.Equals(ovoCode, OrganizationOvoCode.DigitaalVlaanderen, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsAdmin(this HttpContext httpContext)
    {
        return httpContext.User.FindAll(RoadRegistryClaim.ClaimType)
            .Select(RoadRegistryClaim.ReadFrom)
            .Any(claim => claim is not null
                          && string.Equals(claim.Role, RoadRegistryRoles.Admin, StringComparison.OrdinalIgnoreCase));
    }
}
