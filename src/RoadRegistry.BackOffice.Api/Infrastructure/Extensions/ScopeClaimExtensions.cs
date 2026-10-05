namespace RoadRegistry.BackOffice.Api.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;

public static class ScopeClaimExtensions
{
    /// <summary>
    ///     The scopes of the caller, whichever shape they arrived in.
    ///
    ///     A scope claim reaches us in one of two shapes, depending on the authentication scheme. The schemes we mint
    ///     ourselves - the JWT from the code exchange and the api key - add one claim per scope. The Bearer scheme maps
    ///     the introspection response of ACM/IDM verbatim, and that response carries all scopes in a single space
    ///     separated value. Reading only the first shape silently refuses every token that holds more than one scope.
    /// </summary>
    public static IEnumerable<string> GetAcmIdmScopes(this ClaimsPrincipal user)
    {
        return user
            .FindAll(AcmIdmClaimTypes.Scope)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool HasAcmIdmScope(this ClaimsPrincipal user, string scope)
    {
        return user.GetAcmIdmScopes().Contains(scope, StringComparer.OrdinalIgnoreCase);
    }
}
