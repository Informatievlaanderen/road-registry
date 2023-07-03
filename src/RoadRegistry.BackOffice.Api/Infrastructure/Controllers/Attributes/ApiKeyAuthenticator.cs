namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Threading;
using Authentication;
using Be.Vlaanderen.Basisregisters.AcmIdm;

internal class ApiKeyAuthenticator : IApiKeyAuthenticator
{
    private readonly IApiTokenReader _apiKeyReader;

    public ApiKeyAuthenticator(IApiTokenReader apiKeyReader)
    {
        _apiKeyReader = apiKeyReader;
    }

    public async Task<IIdentity> AuthenticateAsync(string apiKey, CancellationToken cancellationToken)
    {
        if (apiKey is null) return new ClaimsIdentity();

        var dbResult = await ReadIdentityAsync(apiKey);
        return dbResult;
    }

    public async Task<IIdentity> AuthenticateAsync(ApiToken apiToken, CancellationToken cancellationToken)
    {
        if (apiToken is null) return new ClaimsIdentity();

        var dbResult = await ReadIdentityAsync(apiToken.ApiKey);
        return dbResult;
    }

    private async Task<IIdentity> ReadIdentityAsync(string apiKey)
    {
        var token = await _apiKeyReader.ReadAsync(apiKey);
        if (token is null || token.Revoked || !token.Metadata.WrAccess)
        {
            return new ClaimsIdentity();
        }

        return new ClaimsIdentity(new Claim[]
            {
                new("sub", apiKey),
                new("active", true.ToString()),
                new(AcmIdmClaimTypes.VoApplicatieNaam, token.ClientName),
                new(AcmIdmClaimTypes.Scope, "digitaalvlaanderen_grar_toegang"),
                new(AcmIdmClaimTypes.Scope, Scopes.VoInfo),
                new(AcmIdmClaimTypes.Scope, Scopes.DvWrAttribuutWaardenBeheer),
                new(AcmIdmClaimTypes.Scope, Scopes.DvWrGeschetsteWegBeheer),
                new(AcmIdmClaimTypes.Scope, Scopes.DvWrIngemetenWegBeheer),
                new(AcmIdmClaimTypes.Scope, Scopes.DvWrUitzonderingenBeheer)
            }, ApiKeyDefaults.AuthenticationScheme);
    }
}
