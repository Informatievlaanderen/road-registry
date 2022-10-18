namespace RoadRegistry.Editor.ProjectionHost;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

internal class WriteTo
{
    public WriteTo()
    {
        Args = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }

    public Dictionary<string, string> Args { get; set; }

    public string Name { get; set; }
}

internal static class WaitFor
{
    public static async Task SeqToBecomeAvailable(
        IConfiguration configuration,
        CancellationToken token = default)
    {
        var writeTos = new List<WriteTo>();
        configuration.GetSection("SERILOG").Bind("WRITETO", writeTos);

        var seq = writeTos.SingleOrDefault(to => "seq".Equals(to.Name, StringComparison.InvariantCultureIgnoreCase));
        if (seq != null)
        {
            var serverUrl = seq.Args["SERVERURL"];
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(serverUrl);
                var exit = false;
                while (!exit)
                {
                    Console.WriteLine("Waiting for Seq to become available ...");

                    try
                    {
                        using var response = await client.GetAsync("/api", token);
                        if (!response.IsSuccessStatusCode)
                            await Task.Delay(TimeSpan.FromSeconds(1), token);
                        else
                            exit = true;
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

    public static async Task SqlStreamStoreToBecomeAvailable(IStreamStore store, ILogger<Program> logger, CancellationToken cancellationToken = default)
    {
        if (store is MsSqlStreamStoreV3)
        {
            var watch = Stopwatch.StartNew();
            var exit = false;
            while (!exit)
                try
                {
                    if (logger.IsEnabled(LogLevel.Information)) logger.LogInformation($"Waiting until sql stream store becomes available ... ({watch.Elapsed:c})");
                    await store.ReadHeadPosition(cancellationToken);
                    exit = true;
                }
                catch (Exception exception)
                {
                    if (logger.IsEnabled(LogLevel.Warning)) logger.LogWarning(exception, "Encountered an exception while waiting for sql stream store to become available.");

                    await Task.Delay(1000, cancellationToken);
                }
        }
    }
}