namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore.Streams;

public class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
{
    public Task<(RoadNetworkSnapshot snapshot, int? version)> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        // For debugging purposes
        var path = @"snapshot";
        if (path is not null && File.Exists(path))
        {
            var snapshot = Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache.S3CacheSerializer.Serializer.DeserializeObject<RoadNetworkSnapshot>(File.ReadAllBytes(path)).Value;
            return Task.FromResult<(RoadNetworkSnapshot snapshot, int? version)>((snapshot, 1));
        }

        return Task.FromResult<(RoadNetworkSnapshot snapshot, int? version)>((null, ExpectedVersion.NoStream));
    }

    public Task<int?> ReadSnapshotVersionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult((int?)0);
    }
}
