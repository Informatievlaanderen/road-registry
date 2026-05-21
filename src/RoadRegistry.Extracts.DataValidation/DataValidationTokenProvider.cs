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
    private const int ExpirationOffsetSeconds = 10;

    private string? _cachedToken;
    private DateTime _tokenExpiresAt;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DataValidationOptions _options;

    public DataValidationTokenProvider(IHttpClientFactory httpClientFactory, DataValidationOptions options)
    {
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt)
        {
            return _cachedToken;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedToken;
            }

            var parameters = new Parameters();

            var tokenClient = new TokenClient(
                () => _httpClientFactory.CreateClient(),
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

            var expiresInSeconds = response.ExpiresIn > ExpirationOffsetSeconds ? response.ExpiresIn - ExpirationOffsetSeconds : 0;
            _cachedToken = response.AccessToken!;
            _tokenExpiresAt = DateTime.UtcNow.AddSeconds(expiresInSeconds);
            return _cachedToken;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
