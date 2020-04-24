namespace RoadRegistry.Legacy.Extract
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
            SqlConnectionStringBuilder builder,
            ILogger<Program> logger,
            CancellationToken token = default)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    logger.LogInformation("Waiting for sql server to become available ...");
                    using (var connection = new SqlConnection(builder.ConnectionString))
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        using (var command = new SqlCommand(@"SELECT
    (SELECT COUNT(*) FROM [dbo].[wegknoop]) AS RoadNodeCount,
    (SELECT COUNT(*) FROM [dbo].[wegsegment]) AS RoadSegmentCount,
    (SELECT COUNT(*) FROM [dbo].[listOrganisatie]) AS OrganizationCount,
    (SELECT COUNT(*) FROM [dbo].[crabsnm]) AS StreetNameCount,
    (SELECT COUNT(*) FROM [dbo].[gemeenteNIS]) AS MunicipalityCount,
    (SELECT COUNT(*) FROM [dbo].[EuropeseWeg]) AS EuropeanRoadAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[nationaleWeg]) AS NationalRoadAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[genummerdeWeg]) AS NumberedRoadAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[rijstroken]) AS LaneAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[wegbreedte]) AS WidthAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[wegverharding]) AS SurfaceAttributeCount,
    (SELECT COUNT(*) FROM [dbo].[ongelijkgrondseKruising]) AS GradeSeparatedJunctionCount", connection))
                        {
                            command.CommandType = CommandType.Text;
                            using (var reader = await command.ExecuteReaderAsync(token).ConfigureAwait(false))
                            {
                                if (!reader.IsClosed && reader.HasRows)
                                {
                                    exit = true;
                                }
                            }
                        }
                    }
                }
                catch(Exception exception)
                {
                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug(exception, "Sql server still not available because: {0}", exception.Message);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(1), token).ConfigureAwait(false);
                }
            }

            logger.LogInformation("Sql server became available.");
        }
    }
}
