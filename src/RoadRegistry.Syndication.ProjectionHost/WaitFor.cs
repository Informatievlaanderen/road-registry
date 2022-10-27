namespace RoadRegistry.Syndication.ProjectionHost;

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
                                await Task.Delay(TimeSpan.FromSeconds(1), token);
                            else
                                exit = true;
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

    public static async Task SyndicationApiToBecomeAvailable(
        IHttpClientFactory httpClientFactory,
        ISyndicationFeedConfiguration configuration,
        ILogger<Program> logger,
        CancellationToken cancellationToken = default)
    {
        var watch = Stopwatch.StartNew();
        var exit = false;
        while (!exit)
            try
            {
                if (logger.IsEnabled(LogLevel.Information))
                    logger.LogInformation(
                        $"Waiting until syndication api ({configuration.Uri}) becomes available ... ({watch.Elapsed:c})");

                var client = httpClientFactory.CreateClient();
                client.BaseAddress = new Uri($"{configuration.Uri.Scheme}://{configuration.Uri.Authority}");

                var httpResponseMessage = await client.GetAsync(configuration.Uri.PathAndQuery, cancellationToken);
                httpResponseMessage.EnsureSuccessStatusCode();

                exit = true;
            }
            catch (Exception exception)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                    logger.LogWarning(exception,
                        $"Encountered an exception while waiting for the syndication api ({configuration.Uri}) to become available.");

                await Task.Delay(1000, cancellationToken);
            }
    }
}