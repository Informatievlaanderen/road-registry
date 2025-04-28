namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Http;

public static class IdentityExtensions
{
    public static string? GetOperatorName(this HttpContext httpContext)
    {
        return httpContext.FindOrgCodeClaim() ?? httpContext.User.FindFirst("operator")?.Value;
    }
}
//TODO-pr dotnet list package --outdated

