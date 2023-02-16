namespace RoadRegistry.BackOffice.Core;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using MessagePack;
using Messages;
using Microsoft.IO;
using SqlStreamStore.Streams;

internal class RoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
{
    public static readonly MetadataKey AtVersionKey = new("at-version");
    private static readonly ContentType MessagePackContentType = ContentType.Parse("application/msgpack");
    private static readonly BlobName SnapshotHead = new("roadnetworksnapshot-HEAD");
    private static readonly BlobName SnapshotPrefix = new("roadnetworksnapshot-");
    private readonly IBlobClient _client;
    private readonly RecyclableMemoryStreamManager _streamManager;

    public RoadNetworkSnapshotReader(RoadNetworkSnapshotsBlobClient client, RecyclableMemoryStreamManager streamManager)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _streamManager = streamManager ?? throw new ArgumentNullException(nameof(streamManager));
    }

    public async Task<int> ReadSnapshotVersionAsync(CancellationToken cancellationToken)
    {
        if (!await _client.BlobExistsAsync(SnapshotHead, cancellationToken))
        {
            return ExpectedVersion.NoStream;
        }

        var snapshotHeadBlob = await _client.GetBlobAsync(SnapshotHead, cancellationToken);
        await using (var headStream = await snapshotHeadBlob.OpenAsync(cancellationToken))
        {
            var snapshotHead =
                await MessagePackSerializer.DeserializeAsync<RoadNetworkSnapshotHead>(
                    headStream,
                    cancellationToken: cancellationToken
                );
            var snapshotBlobName = new BlobName(snapshotHead.SnapshotBlobName);
            if (!await _client.BlobExistsAsync(snapshotBlobName, cancellationToken))
            {
                return ExpectedVersion.NoStream;
            }

            var snapshotBlob = await _client.GetBlobAsync(snapshotBlobName, cancellationToken);
            if (!snapshotBlob.Metadata.TryGetAtVersion(out var version))
            {
                return ExpectedVersion.NoStream;
            }

            return version;
        }
    }

    public async Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshotAsync(CancellationToken cancellationToken)
    {
        if (!await _client.BlobExistsAsync(SnapshotHead, cancellationToken))
        {
            return (null, ExpectedVersion.NoStream);
        }

        var snapshotHeadBlob = await _client.GetBlobAsync(SnapshotHead, cancellationToken);
        await using (var headStream = await snapshotHeadBlob.OpenAsync(cancellationToken))
        {
            var snapshotHead =
                await MessagePackSerializer.DeserializeAsync<RoadNetworkSnapshotHead>(
                    headStream,
                    cancellationToken: cancellationToken
                );
            var snapshotBlobName = new BlobName(snapshotHead.SnapshotBlobName);
            if (!await _client.BlobExistsAsync(snapshotBlobName, cancellationToken))
            {
                return (null, ExpectedVersion.NoStream);
            }

            var snapshotBlob = await _client.GetBlobAsync(snapshotBlobName, cancellationToken);
            if (!snapshotBlob.Metadata.TryGetAtVersion(out var version))
            {
                return (null, ExpectedVersion.NoStream);
            }

            await using (var snapshotStream = await snapshotBlob.OpenAsync(cancellationToken))
            {
                var snapshot = await MessagePackSerializer.DeserializeAsync<RoadNetworkSnapshot>(
                    snapshotStream,
                    cancellationToken: cancellationToken);
                return (snapshot, version);
            }
        }
    }
}
