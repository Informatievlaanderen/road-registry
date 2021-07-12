namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO.Compression;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Core;
    using Framework;
    using SqlStreamStore;

    public class RoadNetworkExtractEventModule : EventHandlerModule
    {
        public RoadNetworkExtractEventModule(
            IBlobClient client,
            IRoadNetworkExtractArchiveAssembler assembler,
            IStreamStore store)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (store == null) throw new ArgumentNullException(nameof(store));

            For<Messages.RoadNetworkExtractGotRequested>()
                .UseRoadNetworkExtractCommandQueue(store)
                .Handle(async (queue, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.DownloadId.ToString("N"));
                    var blobName = new BlobName(archiveId);
                    if (await client.BlobExistsAsync(blobName, ct))
                    {
                        //Case: we previously uploaded a blob for this particular download.
                        //var blob = await client.GetBlobAsync(blobName, ct);
                        // var revision = new RoadNetworkRevision(
                        //     int.Parse(
                        //         blob.Metadata
                        //             .Single(metadatum => metadatum.Key == new MetadataKey("Revision"))
                        //             .Value,
                        //         CultureInfo.InvariantCulture));
                        var command = new Command(new Messages.AnnounceRoadNetworkExtractDownloadBecameAvailable
                        {
                            RequestId = message.Body.RequestId,
                            DownloadId = message.Body.DownloadId,
                            //Revision = revision,
                            ArchiveId = archiveId
                        })
                        .WithMessageId(message.MessageId);
                        await queue.Write(command, ct);
                    }
                    else
                    {
                        var contour = GeometryTranslator.Translate(message.Body.Contour);
                        var content = await assembler.AssembleWithin(contour); //(content, revision)
                        await client.CreateBlobAsync(
                            new BlobName(archiveId),
                            Metadata.None, // .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("Revision"), revision.ToInt32().ToString(CultureInfo.InvariantCulture))),
                            ContentType.Parse("application/x-zip-compressed"),
                            content,
                            ct);
                        var command = new Command(new Messages.AnnounceRoadNetworkExtractDownloadBecameAvailable
                            {
                                RequestId = message.Body.RequestId,
                                DownloadId = message.Body.DownloadId,
                                //Revision = revision,
                                ArchiveId = archiveId
                            })
                            .WithMessageId(message.MessageId);
                        await queue.Write(command, ct);
                    }
                });
        }
    }
}
