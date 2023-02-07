namespace RoadRegistry.Hosts;

using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class DistributedStreamStoreLock : DistributedLock<IStreamStore>
{
    public DistributedStreamStoreLock(DistributedStreamStoreLockOptions options, StreamName streamName, ILogger logger)
        : base(options, $"{typeof(DistributedStreamStoreLock).Name}-{streamName}", logger)
    {
    }
}

public class DistributedStreamStoreLockConfiguration : DistributedLockConfiguration
{
    public const string SectionName = "DistributedStreamStoreLock";
}

public class DistributedStreamStoreLockOptions : DistributedLockOptions
{
}
