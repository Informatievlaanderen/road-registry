namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers.Attributes;

using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Authentication;
using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;

public class ApiKeyAuthenticator : IApiKeyAuthenticator
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

        var role = RoadRegistryRoles.Admin;
        var scopes = RoadRegistryRoles.GetScopes(role);

        return new ClaimsIdentity(new Claim[]
        {
            new("sub", apiKey),
            new("active", true.ToString()),
            new(AcmIdmClaimTypes.VoApplicatieNaam, token.ClientName),
            new(RoadRegistryClaim.ClaimType, RoadRegistryClaim.ConvertRoleToClaimValue(role)),
            new("operator", AnonymizeApiKey(apiKey))
        }.Concat(
            scopes.Select(scope => new Claim(AcmIdmClaimTypes.Scope, scope))
        ), AuthenticationSchemes.ApiKey);
    }

    private string AnonymizeApiKey(string apiKey)
    {
        var sanitized = apiKey
            .Replace(" ", string.Empty)
            .Replace(".", string.Empty)
            .Replace("-", string.Empty);

        var partsLength = sanitized.Length / 4;

        if (partsLength * 2 > 16)
        {
            partsLength = 8;
        }

        var part1 = sanitized[..partsLength];
        var part2 = sanitized[^partsLength..];

        return $"{part1}***{part2}";
    }
}
