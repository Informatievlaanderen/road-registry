namespace RoadRegistry.Api
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using AspNetCore.AsyncInitialization;
    using Microsoft.Extensions.Configuration;
    using SqlStreamStore;

    public class WaitForSqlStreamStore : IAsyncInitializer
    {
        private readonly IStreamStore _store;
        private readonly IConfiguration _configuration;

        public WaitForSqlStreamStore(IStreamStore store, IConfiguration configuration)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task InitializeAsync()
        {
            if (_store is MsSqlStreamStore)
            {
                var builder = new SqlConnectionStringBuilder(_configuration.GetConnectionString("Events"));
                var exit = false;
                while(!exit)
                {
                    try
                    {
                        using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(builder.ConnectionString)
                        {
                            Schema = "RoadRegistry"
                        }))
                        {
                            await streamStore.ReadHeadPosition(default);
                            exit = true;
                        }
                    }
                    catch
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }
            }
        }
    }
}