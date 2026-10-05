namespace RoadRegistry.BackOffice.Api.Infrastructure.Authorization;

using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm.AuthorizationHandlers;
using Extensions;
using Microsoft.AspNetCore.Authorization;

/// <summary>
///     Replaces the <see cref="AcmIdmAuthorizationRequirement" /> handler of the AcmIdm package, which compares the
///     scope claim by exact value equality and so never sees a scope inside a space separated claim value. ACM/IDM
///     returns all scopes of a token in one such value, which refused every caller holding more than one scope.
///
///     This registers instead of, not next to, the package handler: a handler that calls
///     <see cref="AuthorizationHandlerContext.Fail()" /> decides the outcome even when another one succeeded.
/// </summary>
public class RoadRegistryAcmIdmAuthorizationHandler : AuthorizationHandler<AcmIdmAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AcmIdmAuthorizationRequirement requirement)
    {
        var ovoCode = FindOvoCode(context.User);

        if (!string.IsNullOrWhiteSpace(ovoCode)
            && requirement.BlacklistedOvoCodes.Contains(ovoCode, StringComparer.OrdinalIgnoreCase))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        if (requirement.AllowedScopes.Any(scope => context.User.HasAcmIdmScope(scope)))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        context.Fail();
        return Task.CompletedTask;
    }

    // Same order the package uses: the dedicated claim first, then an organisation code that turns out to be an OVO code.
    private static string? FindOvoCode(ClaimsPrincipal user)
    {
        var ovoCode = user.FindFirst(AcmIdmClaimTypes.VoOvoCode)?.Value;
        if (ovoCode is not null)
        {
            return ovoCode;
        }

        var orgCode = user.FindFirst(AcmIdmClaimTypes.VoOrgCode)?.Value;

        return orgCode is not null && orgCode.StartsWith("ovo", StringComparison.OrdinalIgnoreCase)
            ? orgCode
            : null;
    }
}
