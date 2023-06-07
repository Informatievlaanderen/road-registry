namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using RoadRegistry.BackOffice.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class RoadNetworkSnapshotWriter : IRoadNetworkSnapshotWriter
{
    private readonly S3CacheService _cacheService;

    public RoadNetworkSnapshotWriter(S3CacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
    {
        await _cacheService.SetValue(version.ToString(), snapshot, cancellationToken);
    }
}
