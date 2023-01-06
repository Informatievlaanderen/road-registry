namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.IO.Compression;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Framework;
using Messages;
using SqlStreamStore;

public class RoadNetworkChangesArchiveEventModule : EventHandlerModule
{
    public RoadNetworkChangesArchiveEventModule(
        RoadNetworkUploadsBlobClient client,
        IZipArchiveTranslator translator,
        IStreamStore store,
        CommandMetadata commandMetadata)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));
        if (translator == null) throw new ArgumentNullException(nameof(translator));
        if (store == null) throw new ArgumentNullException(nameof(store));

        For<RoadNetworkChangesArchiveAccepted>()
            .UseRoadNetworkCommandQueue(store, commandMetadata)
            .Handle(async (queue, message, ct) =>
            {
                var archiveId = new ArchiveId(message.Body.ArchiveId);
                var requestId = ChangeRequestId.FromArchiveId(archiveId);
                var archiveBlob = await client.GetBlobAsync(new BlobName(archiveId), ct);
                await using (var archiveBlobStream = await archiveBlob.OpenAsync(ct))
                using (var archive = new ZipArchive(archiveBlobStream, ZipArchiveMode.Read, false))
                {
                    var requestedChanges = new List<RequestedChange>();
                    var translatedChanges = translator.Translate(archive);
                    foreach (var change in translatedChanges)
                    {
                        var requestedChange = new RequestedChange();
                        change.TranslateTo(requestedChange);
                        requestedChanges.Add(requestedChange);
                    }

                    var command = new Command(new ChangeRoadNetwork
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
