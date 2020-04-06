namespace RoadRegistry.BackOffice.ProjectionHost
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using SqlStreamStore;

    public static class StreamStoreExtensions
    {
        public static async Task WaitUntilAvailable(this IStreamStore store, ILogger<Program> logger, CancellationToken cancellationToken = default)
        {
            if (store is MsSqlStreamStore)
            {
                var watch = Stopwatch.StartNew();
                var exit = false;
                while(!exit)
                {
                    try
                    {
                        if (logger.IsEnabled(LogLevel.Information))
                        {
                            logger.LogInformation($"Waiting until sql stream store becomes available ... ({watch.Elapsed:c})");
                        }
                        await store.ReadHeadPosition(cancellationToken);
                        exit = true;
                    }
                    catch (Exception exception)
                    {
                        if (logger.IsEnabled(LogLevel.Warning))
                        {
                            logger.LogWarning(exception, "Encountered an exception while waiting for sql stream store to become available.");
                        }

                        await Task.Delay(1000, cancellationToken);
                    }
                }
            }
        }
    }
}
