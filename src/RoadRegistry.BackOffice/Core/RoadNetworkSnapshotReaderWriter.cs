namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using MessagePack;
using Messages;
using Microsoft.IO;
using SqlStreamStore.Streams;

[Obsolete("Use RoadNetworkSnapshotReader and RoadNetworkSnapshotWriter instead")]
internal class RoadNetworkSnapshotReaderWriter : IRoadNetworkSnapshotReader, IRoadNetworkSnapshotWriter
{
    public static readonly MetadataKey AtVersionKey = new("at-version");
    private static readonly ContentType MessagePackContentType = ContentType.Parse("application/msgpack");
    private static readonly BlobName SnapshotHead = new("roadnetworksnapshot-HEAD");
    private static readonly BlobName SnapshotPrefix = new("roadnetworksnapshot-");
    private readonly IBlobClient _client;
    private readonly RecyclableMemoryStreamManager _streamManager;

    public RoadNetworkSnapshotReaderWriter(RoadNetworkSnapshotsBlobClient client, RecyclableMemoryStreamManager streamManager)
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

    public async Task SetHeadToVersion(int version, CancellationToken cancellationToken)
    {
        var newSnapshotBlobName = SnapshotPrefix.Append(new BlobName(version.ToString()));
        if (version > StreamVersion.Start && !await _client.BlobExistsAsync(newSnapshotBlobName, cancellationToken))
        {
            throw new InvalidOperationException($"Snapshot with name {newSnapshotBlobName} not found");
        }

        if (await _client.BlobExistsAsync(SnapshotHead, cancellationToken))
        {
            await _client.DeleteBlobAsync(SnapshotHead, cancellationToken);
        }

        var newSnapshotHead = new RoadNetworkSnapshotHead
        {
            SnapshotBlobName = newSnapshotBlobName.ToString()
        };

        using (var stream = _streamManager.GetStream())
        {
            await MessagePackSerializer.SerializeAsync(stream, newSnapshotHead,
                cancellationToken: cancellationToken);

            stream.Position = 0;

            await _client.CreateBlobAsync(
                SnapshotHead,
                Metadata.None,
                MessagePackContentType,
                stream,
                cancellationToken);
        }
    }

    public async Task WriteSnapshot(RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
    {
        var snapshotBlobName = SnapshotPrefix.Append(new BlobName(version.ToString()));
        if (!await _client.BlobExistsAsync(snapshotBlobName, cancellationToken))
        {
            using (var stream = _streamManager.GetStream())
            {
                await MessagePackSerializer.SerializeAsync(stream, snapshot, cancellationToken: cancellationToken);

                stream.Position = 0;

                await _client.CreateBlobAsync(
                    snapshotBlobName,
                    Metadata.None.AtVersion(version),
                    MessagePackContentType,
                    stream,
                    cancellationToken);
            }

            if (await _client.BlobExistsAsync(SnapshotHead, cancellationToken))
            {
                await _client.DeleteBlobAsync(SnapshotHead, cancellationToken);
            }

            var snapshotHead = new RoadNetworkSnapshotHead
            {
                SnapshotBlobName = snapshotBlobName.ToString()
            };
            using (var stream = _streamManager.GetStream())
            {
                await MessagePackSerializer.SerializeAsync(stream, snapshotHead,
                    cancellationToken: cancellationToken);

                stream.Position = 0;

                await _client.CreateBlobAsync(
                    SnapshotHead,
                    Metadata.None,
                    MessagePackContentType,
                    stream,
                    cancellationToken);
            }
        }
    }
}

internal static class MetadataExtensions
{
    public static Metadata AtVersion(this Metadata metadata, int version)
    {
        return metadata.Add(new KeyValuePair<MetadataKey, string>(RoadNetworkSnapshotReaderWriter.AtVersionKey, version.ToString()));
    }

    public static bool TryGetAtVersion(this Metadata metadata, out int version)
    {
        var found = metadata.Where(metadatum => metadatum.Key == RoadNetworkSnapshotReaderWriter.AtVersionKey).ToArray();
        if (found.Length == 1)
        {
            version = int.Parse(found[0].Value);
            return true;
        }

        version = ExpectedVersion.NoStream;
        return false;
    }
}
