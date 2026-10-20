namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System;
using System.Linq;
using System.Security.Claims;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
using RoadRegistry.ValueObjects;

public static class ClaimsPrincipalExtensions
{
    // Which road segments this caller may modify. An edit endpoint accepts both scopes - holding neither is already
    // answered with a 403 by the authorization policy - so what is left to decide is which of the two geometry draw
    // methods the caller covers. That decision travels with the request, for the domain to enforce per road segment.
    public static EditableGeometryDrawMethods GetEditableGeometryDrawMethods(this ClaimsPrincipal user)
    {
        var scopes = user.GetScopes();

        return new EditableGeometryDrawMethods(
            Ingeschetst: scopes.Contains(Scopes.DvWrGeschetsteWegBeheer, StringComparer.OrdinalIgnoreCase),
            Ingemeten: scopes.Contains(Scopes.DvWrIngemetenWegBeheer, StringComparer.OrdinalIgnoreCase));
    }

    // A scope claim can arrive either as one claim per scope or as a single space separated claim value, depending on
    // the authentication scheme, so both shapes are accounted for.
    private static string[] GetScopes(this ClaimsPrincipal user)
    {
        return user.FindAll(AcmIdmClaimTypes.Scope)
            .SelectMany(claim => claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .ToArray();
    }
}
