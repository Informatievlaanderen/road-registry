namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Microsoft.AspNetCore.Http;

public static class IdentityExtensions
{
    public static string? GetOperatorName(this HttpContext httpContext)
    {
        return httpContext.FindOrgCodeClaim() ?? httpContext.User.FindFirst("operator")?.Value;
    }
    //TODO-pr de operator valideren of deze een bestaande org is om zo de juiste orgid in de operator te steken, voor de upload van extracten
    //in aparte interface plaatsen, bvb IOperatorRetriever
    /*
    private async Task<OrganizationId> FindOrganizationId(OrganizationId organizationId, CancellationToken cancellationToken)
    {
        var maintenanceAuthorityOrganization = await _organizationCache.FindByIdOrOvoCodeOrKboNumberAsync(organizationId, cancellationToken);
        if (maintenanceAuthorityOrganization is not null)
        {
            return maintenanceAuthorityOrganization.Code;
        }

        if (OrganizationOvoCode.AcceptsValue(organizationId))
        {
            //TODO-pr Logger.LogError();
            //problems = problems.Add(new MaintenanceAuthorityNotKnown(organizationId));
        }

        return organizationId;
    }*/
}
