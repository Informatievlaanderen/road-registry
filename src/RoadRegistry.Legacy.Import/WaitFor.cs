namespace RoadRegistry.Legacy.Import
{
    using System;
    using System.Data;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    internal static class WaitFor
    {
        public static async Task SeqToBecomeAvailable(
            IConfiguration configuration,
            CancellationToken token = default)
        {
            var nameOfSink = configuration.GetValue<string>("SERILOG__WRITETO__0__NAME");
            if (nameOfSink == "Seq")
            {
                var serverUrl = configuration.GetValue<string>("SERILOG__WRITETO__0__ARGS__SERVERURL");
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(serverUrl);
                    var exit = false;
                    while (!exit)
                    {
                        Console.WriteLine("Waiting for Seq to become available ...");

                        try
                        {
                            using (var response = await client.GetAsync("/api", token))
                            {
                                if (!response.IsSuccessStatusCode)
                                {
                                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                                }
                                else
                                {
                                    exit = true;
                                }
                            }
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(
                                "Observed an exception while waiting for Seq to become available. Exception:{0}",
                                exception);
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        }
                    }
                }

                Console.WriteLine("Seq became available.");
            }
        }

        public static async Task SqlServerToBecomeAvailable(
            SqlConnectionStringBuilder masterConnectionStringBuilder,
            ILogger<Program> logger,
            CancellationToken token = default)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    logger.LogInformation("Waiting for sql server to become available");
                    using (var connection = new SqlConnection(masterConnectionStringBuilder.ConnectionString))
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        await connection.CloseAsync().ConfigureAwait(false);
                    }

                    exit = true;
                }
                catch(Exception exception)
                {
                    logger.LogWarning(exception, "Encountered an exception while waiting for sql server to become available");
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
            }
        }

        public static async Task SqlServerDatabaseToBecomeAvailable(
            SqlConnectionStringBuilder masterConnectionStringBuilder,
            SqlConnectionStringBuilder eventsConnectionStringBuilder,
            ILogger<Program> logger,
            CancellationToken token = default)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    logger.LogInformation($"Waiting for sql database {eventsConnectionStringBuilder.InitialCatalog} to become available");
                    using (var connection = new SqlConnection(masterConnectionStringBuilder.ConnectionString))
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        var text = $"SELECT COUNT(*) FROM [SYS].[DATABASES] WHERE [Name] = N'{eventsConnectionStringBuilder.InitialCatalog}'";
                        using(var command = new SqlCommand(text, connection))
                        {
                            var value = await command.ExecuteScalarAsync(token);
                            exit = (int) value == 1;
                        }

                        if (!exit)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                        }
                    }
                }
                catch(Exception exception)
                {
                    logger.LogWarning(exception, $"Encountered exception while waiting for sql database {eventsConnectionStringBuilder.InitialCatalog} to become available");
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
            }
        }
    }
}
