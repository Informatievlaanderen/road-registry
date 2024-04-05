namespace RoadRegistry.BackOffice.Api.E2ETests
{
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;

    public class ApiClientTestFixture : IAsyncLifetime
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _createTokenEndpoint;
        private readonly string _backOfficeApiBaseUrl;
        private readonly string _publicApiBaseUrl;
        private readonly string _apiKey;
        private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();

        public IConfiguration Configuration { get; }

        public ApiClientTestFixture(IConfiguration configuration)
        {
            Configuration = configuration;

            _clientId = Configuration.GetRequiredValue<string>("ClientId");
            _clientSecret = Configuration.GetRequiredValue<string>("ClientSecret");
            _createTokenEndpoint = Configuration.GetRequiredValue<string>("CreateTokenEndpoint");
            _publicApiBaseUrl = Configuration.GetRequiredValue<string>("PublicApiBaseUrl");
            _backOfficeApiBaseUrl = Configuration.GetRequiredValue<string>("BackOfficeApiBaseUrl");
            _apiKey = Configuration.GetRequiredValue<string>("ApiKey");
        }

        public virtual Task InitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task<string> GetAcmIdmAccessToken(string requiredScopes = "")
        {
            if (_accessTokens.ContainsKey(requiredScopes) && !_accessTokens[requiredScopes].IsExpired)
            {
                return _accessTokens[requiredScopes].Token;
            }

            var tokenClient = new TokenClient(
                () => new HttpClient(),
                new TokenClientOptions
                {
                    Address = _createTokenEndpoint,
                    ClientId = _clientId,
                    ClientSecret = _clientSecret,
                    Parameters = new Parameters(new[] { new KeyValuePair<string, string>("scope", requiredScopes) })
                });

            var response = await tokenClient.RequestTokenAsync(OidcConstants.GrantTypes.ClientCredentials);

            _accessTokens[requiredScopes] = new AccessToken(response.AccessToken, response.ExpiresIn);

            return _accessTokens[requiredScopes].Token;
        }

        public Task<BackOfficeApiHttpClient?> CreateBackOfficeApiClient(string[] requiredScopes)
        {
            return CreateApiClient<BackOfficeApiHttpClient>(_backOfficeApiBaseUrl, requiredScopes);
        }
        public Task<PublicApiHttpClient?> CreatePublicApiClient(string[] requiredScopes)
        {
            return CreateApiClient<PublicApiHttpClient>(_publicApiBaseUrl, requiredScopes);
        }

        private async Task<THttpClient?> CreateApiClient<THttpClient>(string baseUrl, string[] requiredScopes)
            where THttpClient : HttpClient, new()
        {
            if (string.IsNullOrEmpty(baseUrl))
            {
                return null;
            }

            var httpClient = new THttpClient
            {
                BaseAddress = new Uri(baseUrl)
            };
            
            if (requiredScopes.Any())
            {
                if (!string.IsNullOrEmpty(_apiKey))
                {
                    httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
                }
                else
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAcmIdmAccessToken(string.Join(" ", requiredScopes)));
                }
            }

            return httpClient;
        }
    }

    public class BackOfficeApiHttpClient : HttpClient
    {
    }

    public class PublicApiHttpClient : HttpClient
    {
    }
}
