namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Http;

public static class IdentityExtensions
{
    public static string? GetOperator(this HttpContext httpContext)
    {
        return httpContext.FindOrgCodeClaim() ?? httpContext.User.FindFirst("operator")?.Value;
    }
}
