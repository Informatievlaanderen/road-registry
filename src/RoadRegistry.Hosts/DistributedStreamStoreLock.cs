namespace RoadRegistry.Hosts;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SqlStreamStore;

public class DistributedStreamStoreLock : DistributedLock<IStreamStore>
{
    private readonly DistributedStreamStoreLockOptions _options;
    private readonly ILogger _logger;

    public DistributedStreamStoreLock(DistributedStreamStoreLockOptions options, StreamName streamName, ILogger logger)
        : base(options, $"{nameof(DistributedStreamStoreLock)}-{streamName}", NullLogger.Instance)
    {
        _options = options;
        _logger = logger;
    }

    public async Task RetryRunUntilLockAcquiredAsync(Func<Task> runFunc, CancellationToken cancellationToken)
    {
        await RetryRunUntilLockAcquiredAsync(async () =>
        {
            await runFunc();
            return 0;
        }, cancellationToken);
    }

    public async Task<T> RetryRunUntilLockAcquiredAsync<T>(Func<Task<T>> runFunc, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogWarning("Bypassing distributed stream store lock because it is disabled in configuration.");
            return await runFunc();
        }

        T result = default;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(async () =>
                {
                    result = await runFunc();
                });
                return result;
            }
            catch (AcquireLockFailedException)
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.AcquireLockRetryDelaySeconds), cancellationToken);
            }
        }

        throw new TaskCanceledException();
    }
}

public class DistributedStreamStoreLockConfiguration : DistributedLockConfiguration, IHasConfigurationKey
{
    public int AcquireLockRetryDelaySeconds { get; set; }

    public string GetConfigurationKey()
    {
        return "DistributedStreamStoreLock";
    }
}

public class DistributedStreamStoreLockOptions : DistributedLockOptions
{
    public int AcquireLockRetryDelaySeconds { get; set; }
}
