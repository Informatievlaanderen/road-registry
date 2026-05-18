namespace RoadRegistry.Extracts.DataValidation;

using System.Security.Authentication;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;

public interface IDataValidationTokenProvider
{
    Task<string> GetAccessTokenAsync();
}

public class DataValidationTokenProvider : IDataValidationTokenProvider
{
    private string? _cachedToken;
    private DateTime _tokenExpiresAt;
    private readonly DataValidationOptions _options;

    public DataValidationTokenProvider(DataValidationOptions options)
    {
        _options = options;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken is not null && DateTime.Now < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        var parameters = new Parameters();

        var tokenClient = new TokenClient(
            () => new HttpClient(),
            new TokenClientOptions
            {
                Address = _options.TokenEndPoint,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Parameters = parameters
            });

        var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

        if (response.IsError)
        {
            throw new AuthenticationException(response.ErrorDescription);
        }

        _cachedToken = response.AccessToken!;
        _tokenExpiresAt = DateTime.Now.AddSeconds(response.ExpiresIn - 10);
        return _cachedToken;
    }
}
