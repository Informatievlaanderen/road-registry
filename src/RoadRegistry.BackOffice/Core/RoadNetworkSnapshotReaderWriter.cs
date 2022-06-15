namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using MessagePack;
    using Microsoft.IO;
    using SqlStreamStore.Streams;

    public class RoadNetworkSnapshotReaderWriter : IRoadNetworkSnapshotReader, IRoadNetworkSnapshotWriter
    {
        public static readonly MetadataKey AtVersionKey = new MetadataKey("at-version");
        private static readonly BlobName SnapshotHead = new BlobName("roadnetworksnapshot-HEAD");
        private static readonly BlobName SnapshotPrefix = new BlobName("roadnetworksnapshot-");
        private static readonly ContentType MessagePackContentType = ContentType.Parse("application/msgpack");

        private readonly IBlobClient _client;
        private readonly RecyclableMemoryStreamManager _streamManager;

        public RoadNetworkSnapshotReaderWriter(RoadNetworkSnapshotsBlobClient client, RecyclableMemoryStreamManager streamManager)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _streamManager = streamManager ?? throw new ArgumentNullException(nameof(streamManager));
        }

        public async Task<(Messages.RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
        {
            if (!await _client.BlobExistsAsync(SnapshotHead, cancellationToken))
            {
                return (null, ExpectedVersion.NoStream);
            }

            var snapshotHeadBlob = await _client.GetBlobAsync(SnapshotHead, cancellationToken);
            using (var headStream = await snapshotHeadBlob.OpenAsync(cancellationToken))
            {
                var snapshotHead =
                    await MessagePackSerializer.DeserializeAsync<Messages.RoadNetworkSnapshotHead>(
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

                using (var snapshotStream = await snapshotBlob.OpenAsync(cancellationToken))
                {
                    var snapshot = await MessagePackSerializer.DeserializeAsync<Messages.RoadNetworkSnapshot>(
                        snapshotStream,
                        cancellationToken: cancellationToken);
                    return (snapshot, version);
                }
            }
        }

        public async Task WriteSnapshot(Messages.RoadNetworkSnapshot snapshot, int version, CancellationToken cancellationToken)
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

                var snapshotHead = new Messages.RoadNetworkSnapshotHead
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

        public async Task SetHeadToVersion(int version, CancellationToken cancellationToken)
        {
            var newSnapshotBlobName = SnapshotPrefix.Append(new BlobName(version.ToString()));
            if (!await _client.BlobExistsAsync(newSnapshotBlobName, cancellationToken))
            {
                throw new Exception();
            }

            var newSnapshotHead = new Messages.RoadNetworkSnapshotHead
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
    }
}
