namespace RoadRegistry.BackOffice.Translation
{
    using System;
    using System.IO.Compression;
    using System.Linq;
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
                    var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                    using (var archiveStream = await archiveBlob.OpenAsync(ct))
                    using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, false))
                    {
                        var errors = validator.Validate(archive);
                        if (errors.Count == 0)
                        {
                            using (var archiveStream = await archiveBlob.OpenAsync(ct))
                            using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Read, false))
                            {
                                translator.Translate()
                                new RoadNetworkChangesArchiveAccepted
                                {
                                    ArchiveId = archiveId,
                                    Warnings = new Messages.Problem[0]
                                };
                            }
                        }
                        else
                        {
                            new RoadNetworkChangesArchiveRejected
                            {
                                ArchiveId = archiveId,
                                Errors = errors.Select(error => error.Translate()).ToArray(),
                                Warnings = new Messages.Problem[0]
                            };
                        }
                    }
                });
        }
    }
}
