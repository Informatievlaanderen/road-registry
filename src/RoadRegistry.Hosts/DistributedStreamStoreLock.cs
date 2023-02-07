namespace RoadRegistry.Hosts;

using System;
using System.Threading.Tasks;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
using Microsoft.Extensions.Logging;
using SqlStreamStore;

public class DistributedStreamStoreLock : DistributedLock<IStreamStore>
{
    public DistributedStreamStoreLock(DistributedStreamStoreLockOptions options, StreamName streamName, ILogger logger)
        : base(options, logger) //TODO-rik after update use pass lockName to base
    {
    }

    public Task RunAsync(Func<Task> runFunc) //TODO-rik mag weg na update package
    {
        return runFunc();
    }
}

public class DistributedStreamStoreLockConfiguration : DistributedLockConfiguration
{
    public const string SectionName = "DistributedStreamStoreLock";
}

public class DistributedStreamStoreLockOptions : DistributedLockOptions
{
}
