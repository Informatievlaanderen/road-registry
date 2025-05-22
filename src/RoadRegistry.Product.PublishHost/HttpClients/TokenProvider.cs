namespace RoadRegistry.Product.PublishHost.HttpClients;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

public interface ITokenProvider
{
    Task<string> GetAccessToken();
}

public class TokenProvider : ITokenProvider
{
    private AccessToken? _accessToken;
    private readonly MetadataCenterOptions _options;

    public TokenProvider(IOptions<MetadataCenterOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> GetAccessToken()
    {
        if (_accessToken is not null && !_accessToken.IsExpired)
        {
            return _accessToken.Token;
        }

        var tokenClient = new TokenClient(
            () => new HttpClient(),
            new TokenClientOptions
            {
                Address = _options.TokenEndPoint,
                ClientId = _options.ClientId,
                ClientSecret = _options.ClientSecret,
                Parameters = new Parameters([new KeyValuePair<string, string>("scope", "vo_info dv_metadata_read dv_metadata_write")])
            });

        var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

        if (response.IsError)
        {
            throw new AuthenticationException(response.ErrorDescription);
        }

        _accessToken = new AccessToken(response.AccessToken!, response.ExpiresIn);

        return _accessToken.Token;
    }
}

public class AccessToken
{
    private readonly DateTime _expiresAt;

    public string Token { get; }

    // Let's regard it as expired 10 seconds before it actually expires.
    public bool IsExpired => _expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(10));

    public AccessToken(string token, int expiresIn)
    {
        _expiresAt = DateTime.Now.AddSeconds(expiresIn);
        Token = token;
    }
}

