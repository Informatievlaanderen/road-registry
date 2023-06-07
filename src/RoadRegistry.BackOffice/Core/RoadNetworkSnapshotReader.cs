namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

internal class RoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
{
    private readonly S3CacheService _cacheService;

    public RoadNetworkSnapshotReader(S3CacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<int?> ReadSnapshotVersionAsync(CancellationToken cancellationToken)
    {
        var version = await _cacheService.GetHeadKey();
        return version != null ? int.Parse(version) : null;
    }

    public async Task<(RoadNetworkSnapshot snapshot, int? version)> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        var version = await ReadSnapshotVersionAsync(cancellationToken);
        if (version == null)
        {
            return (null, version);
        }

        var snapshot = await _cacheService.GetHeadValue<RoadNetworkSnapshot>(cancellationToken);

        return (snapshot, version);
    }
}
