namespace RoadRegistry.BackOffice.Api.IntegrationTests
{
    using IdentityModel;
    using IdentityModel.Client;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Xunit;

    public class ApiClientTestFixture : IAsyncLifetime
    {
        private string _clientId;
        private string _clientSecret;
        private string _createTokenEndpoint;
        private string _backOfficeApiBaseUrl;
        private string _apiKey;
        private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();

        public IConfiguration Configuration { get; private set; }

        public virtual Task InitializeAsync()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = Configuration.GetRequiredValue<string>("ClientId");
            _clientSecret = Configuration.GetRequiredValue<string>("ClientSecret");
            _createTokenEndpoint = Configuration.GetRequiredValue<string>("CreateTokenEndpoint");
            _backOfficeApiBaseUrl = Configuration.GetRequiredValue<string>("BackOfficeApiBaseUrl");
            _apiKey = Configuration.GetRequiredValue<string>("ApiKey");

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

        public async Task<HttpClient?> CreateApiClient(string[] requiredScopes)
        {
            if (string.IsNullOrEmpty(_backOfficeApiBaseUrl))
            {
                return null;
            }

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(_backOfficeApiBaseUrl)
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
}
