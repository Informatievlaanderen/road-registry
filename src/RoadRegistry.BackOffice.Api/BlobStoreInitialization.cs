namespace RoadRegistry.Api
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using AspNetCore.AsyncInitialization;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Sql;
    using Microsoft.Extensions.Configuration;

    public class BlobStoreInitialization : IAsyncInitializer
    {
        private readonly IBlobClient _client;
        private readonly IConfiguration _configuration;

        public BlobStoreInitialization(IBlobClient client, IConfiguration configuration)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public Task InitializeAsync()
        {
            if (_client is SqlBlobClient)
            {
                return new SqlBlobSchema(
                    new SqlConnectionStringBuilder(_configuration.GetConnectionString("BlobsAdmin"))
                ).CreateSchemaIfNotExists("RoadRegistryBlobs");
            }
            return Task.CompletedTask;
        }
    }
}
