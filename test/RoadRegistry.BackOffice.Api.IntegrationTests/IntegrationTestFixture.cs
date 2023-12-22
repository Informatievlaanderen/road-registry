namespace RoadRegistry.BackOffice.Api.IntegrationTests
{
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using IdentityModel;
    using IdentityModel.Client;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class IntegrationTestFixture : IAsyncLifetime
    {
        private string _clientId;
        private string _clientSecret;
        private string _createTokenEndpoint;
        private readonly IDictionary<string, AccessToken> _accessTokens = new Dictionary<string, AccessToken>();

        public TestServer TestServer { get; private set; }
        public SqlConnection SqlConnection { get; private set; }

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

        public async Task InitializeAsync()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            _clientId = configuration.GetRequiredValue<string>("ClientId");
            _clientSecret = configuration.GetRequiredValue<string>("ClientSecret");
            _createTokenEndpoint = configuration.GetRequiredValue<string>("CreateTokenEndpoint");

            using var _ = DockerComposer.Compose("sqlserver.yml", "road-integration-tests");
            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services => services.AddAutofac())
                .UseConfiguration(configuration)
                .UseStartup<Startup>()
                .ConfigureLogging(loggingBuilder => loggingBuilder.AddConsole())
                .UseTestServer();

            TestServer = new TestServer(hostBuilder);
        }

        private async Task WaitForSqlServerToBecomeAvailable()
        {
            foreach (var _ in Enumerable.Range(0, 60))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                if (await OpenConnection())
                {
                    break;
                }
            }
        }

        private async Task<bool> OpenConnection()
        {
            try
            {
                SqlConnection = new SqlConnection("Server=localhost,5434;User Id=sa;Password=E@syP@ssw0rd;database=master;TrustServerCertificate=True;");
                await SqlConnection.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task CreateDatabase()
        {
            var cmd = new SqlCommand(@"IF NOT EXISTS(SELECT * FROM sys.databases WHERE name = 'road-registry') BEGIN CREATE DATABASE [road-registry] END", SqlConnection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DisposeAsync()
        {
            await SqlConnection.DisposeAsync();
        }

        public async Task<HttpClient> CreateApiClient(string[] requiredScopes)
        {
            var configuration = TestServer.Services.GetRequiredService<IConfiguration>();

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(configuration.GetRequiredValue<string>("BackOfficeApiBaseUrl"))
            };
            
            if (requiredScopes.Any())
            {
                var apiKey = configuration.GetRequiredValue<string>("ApiKey");
                if (!string.IsNullOrEmpty(apiKey))
                {
                    httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
                }
                else
                {
                    var client = TestServer.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAcmIdmAccessToken(string.Join(" ", requiredScopes)));
                }
            }

            return httpClient;
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
}
