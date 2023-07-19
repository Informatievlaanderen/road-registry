namespace RoadRegistry.BackOffice.Api.Infrastructure.Authentication;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.AcmIdm;

public static class RoadRegistryRoles
{
    public const string Admin = "admin";
    public const string Editor = "editeerder";
    public const string Reader = "lezer";

    private static readonly Dictionary<string, string[]> RoleToScopeMapping = new()
    {
        {Admin, new []{Scopes.VoInfo, Scopes.DvWrAttribuutWaardenBeheer, Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer, Scopes.DvWrUitzonderingenBeheer}},
        {Editor, new []{Scopes.VoInfo, Scopes.DvWrAttribuutWaardenBeheer, Scopes.DvWrGeschetsteWegBeheer, Scopes.DvWrIngemetenWegBeheer}},
        {Reader, new []{Scopes.VoInfo}}
    };

    public static string[] GetScopes(string role)
    {
        if (RoleToScopeMapping.TryGetValue(role, out var scopes))
        {
            return scopes;
        }

        return RoleToScopeMapping[Reader];
    }
}
