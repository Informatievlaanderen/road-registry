namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
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

            For<RoadNetworkChangesArchiveUploaded>()
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var upload = await context.RoadNetworkChangesArchives.Get(archiveId);
                    var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        upload.ValidateArchiveUsing(archive, validator);
                    }
                });
            For<RoadNetworkChangesArchiveAccepted>()
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var archiveId = new ArchiveId(message.Body.ArchiveId);
                    var upload = await context.RoadNetworkChangesArchives.Get(archiveId);
                    var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                    {
                        var translation = translator.Translate(archive);
                    }
                });
        }
    }
}
