namespace RoadRegistry.BackOffice.Api.IntegrationTests
{
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DockerUtilities;
    using Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.TestHost;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class IntegrationTestFixture : ApiClientTestFixture
    {
        public TestServer TestServer { get; private set; }
        public SqlConnection SqlConnection { get; private set; }
        
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            using var _ = DockerComposer.Compose("sqlserver.yml", "road-integration-tests");
            await WaitForSqlServerToBecomeAvailable();

            await CreateDatabase();

            var hostBuilder = new WebHostBuilder()
                .ConfigureServices(services => services.AddAutofac())
                .UseConfiguration(Configuration)
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

        public override async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await SqlConnection.DisposeAsync();
        }
    }
}
