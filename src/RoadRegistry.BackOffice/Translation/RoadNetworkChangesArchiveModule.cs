namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.Collections.Generic;
    using System.IO.Compression;
    using System.Linq;
    using Autofac.Core.Resolving;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Framework;
    using Messages;
    using Model;
    using SqlStreamStore;

    public class RoadNetworkChangesArchiveModule : CommandHandlerModule
    {
        public RoadNetworkChangesArchiveModule(
            IBlobClient client,
            IStreamStore store,
            IZipArchiveValidator validator,
            IZipArchiveTranslator translator)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (validator == null) throw new ArgumentNullException(nameof(validator));
            if (translator == null) throw new ArgumentNullException(nameof(translator));

            For<UploadRoadNetworkChangesArchive>()
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var upload = RoadNetworkChangesArchive.Upload(archiveId);
                    var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        upload.ValidateArchiveUsing(archive, validator);
                    }
                    context.RoadNetworkChangesArchives.Add(upload);
                });

            For<RoadNetworkChangesArchiveAccepted>()
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        var requestedChanges = new List<Messages.RequestedChange>();
                        foreach (var change in translator.Translate(archive))
                        {
                            var requestedChange = new Messages.RequestedChange();
                            change.TranslateTo(requestedChange);
                            requestedChanges.Add(requestedChange);
                        }

                        var command = new Messages.ChangeRoadNetwork
                        {
                            Changes = requestedChanges.ToArray()
                        };
                    }
                });
        }
    }
}
