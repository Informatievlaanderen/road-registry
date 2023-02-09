namespace RoadRegistry.Hosts;

using System;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class DistributedStreamStoreLock : DistributedLock<IStreamStore>
{
    private readonly DistributedStreamStoreLockOptions _options;
    
    public DistributedStreamStoreLock(DistributedStreamStoreLockOptions options, StreamName streamName, ILogger logger)
        : base(options, $"{nameof(DistributedStreamStoreLock)}-{streamName}", logger)
    {
        _options = options;
    }
    
    public async Task RetryRunUntilLockAcquiredAsync(Func<Task> runFunc, CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await RunAsync(runFunc);
                break;
            }
            catch (AcquireLockFailedException)
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.AcquireLockRetryDelaySeconds), cancellationToken);
            }
        }
    }
}

public class DistributedStreamStoreLockConfiguration : DistributedLockConfiguration
{
    public new const string SectionName = "DistributedStreamStoreLock";
}

public class DistributedStreamStoreLockOptions : DistributedLockOptions
{
    public int AcquireLockRetryDelaySeconds { get; set; }
}
