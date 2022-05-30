namespace RoadRegistry.BackOffice.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Framework;
    using Polly;
    using SqlStreamStore;
    using Uploads;

    public class RoadNetworkExtractEventModule : EventHandlerModule
    {
        public RoadNetworkExtractEventModule(
            RoadNetworkExtractDownloadsBlobClient downloadsBlobClient,
            RoadNetworkExtractUploadsBlobClient uploadsBlobClient,
            IRoadNetworkExtractArchiveAssembler assembler,
            IZipArchiveTranslator translator,
            IStreamStore store)
        {
            if (downloadsBlobClient == null) throw new ArgumentNullException(nameof(downloadsBlobClient));
            if (uploadsBlobClient == null) throw new ArgumentNullException(nameof(uploadsBlobClient));
            if (assembler == null) throw new ArgumentNullException(nameof(assembler));
            if (translator == null) throw new ArgumentNullException(nameof(translator));
            if (store == null) throw new ArgumentNullException(nameof(store));

            For<Messages.RoadNetworkExtractGotRequested>()
                .UseRoadNetworkExtractCommandQueue(store)
                .Handle(async (queue, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.DownloadId.ToString("N"));
                    var blobName = new BlobName(archiveId);

                    var policy = Policy
                        .HandleResult<bool>(exists => exists == false)
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(5),
                        });
                    var blobExists = await policy.ExecuteAsync(() => downloadsBlobClient.BlobExistsAsync(blobName, ct));

                    if (blobExists)
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
                        var request = new RoadNetworkExtractAssemblyRequest(
                            new ExternalExtractRequestId(message.Body.ExternalRequestId),
                            new DownloadId(message.Body.DownloadId),
                            new ExtractDescription(string.Empty),
                            GeometryTranslator.Translate(message.Body.Contour));
                        using (var content = await assembler.AssembleArchive(request, ct)) //(content, revision)
                        {
                            content.Position = 0L;

                            await downloadsBlobClient.CreateBlobAsync(
                                new BlobName(archiveId),
                                Metadata
                                    .None, // .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("Revision"), revision.ToInt32().ToString(CultureInfo.InvariantCulture))),
                                ContentType.Parse("application/x-zip-compressed"),
                                content,
                                ct);
                        }

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

            For<Messages.RoadNetworkExtractGotRequestedV2>()
                .UseRoadNetworkExtractCommandQueue(store)
                .Handle(async (queue, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.DownloadId.ToString("N"));
                    var blobName = new BlobName(archiveId);

                    var policy = Policy
                        .HandleResult<bool>(exists => exists == false)
                        .WaitAndRetryAsync(new[]
                        {
                            TimeSpan.FromSeconds(1),
                            TimeSpan.FromSeconds(3),
                            TimeSpan.FromSeconds(5),
                        });
                    var blobExists = await policy.ExecuteAsync(() => downloadsBlobClient.BlobExistsAsync(blobName, ct));

                    if (blobExists)
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
                        var request = new RoadNetworkExtractAssemblyRequest(
                            new ExternalExtractRequestId(message.Body.ExternalRequestId),
                            new DownloadId(message.Body.DownloadId),
                            new ExtractDescription(message.Body.Description),
                            GeometryTranslator.Translate(message.Body.Contour));
                        using (var content = await assembler.AssembleArchive(request, ct)) //(content, revision)
                        {
                            content.Position = 0L;

                            await downloadsBlobClient.CreateBlobAsync(
                                new BlobName(archiveId),
                                Metadata
                                    .None, // .Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("Revision"), revision.ToInt32().ToString(CultureInfo.InvariantCulture))),
                                ContentType.Parse("application/x-zip-compressed"),
                                content,
                                ct);
                        }

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

            For<Messages.RoadNetworkExtractChangesArchiveAccepted>()
                .UseRoadNetworkCommandQueue(store)
                .Handle(async (queue, message, ct) =>
                {
                    var uploadId = new UploadId(message.Body.UploadId);
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var requestId = ChangeRequestId.FromUploadId(uploadId);
                    var archiveBlob = await uploadsBlobClient.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        var requestedChanges = new List<Messages.RequestedChange>();
                        var translatedChanges = translator.Translate(archive);
                        foreach (var change in translatedChanges)
                        {
                            var requestedChange = new Messages.RequestedChange();
                            change.TranslateTo(requestedChange);
                            requestedChanges.Add(requestedChange);
                        }

                        var command = new Command(new Messages.ChangeRoadNetwork
                            {
                                RequestId = requestId,
                                Changes = requestedChanges.ToArray(),
                                Reason = translatedChanges.Reason,
                                Operator = translatedChanges.Operator,
                                OrganizationId = translatedChanges.Organization
                            })
                            .WithMessageId(message.MessageId);

                        await queue.Write(command, ct);
                    }
                });
        }
    }
}
