namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.DutchTranslations;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using DutchTranslations;
using Extracts.DutchTranslations;
using Infrastructure.DutchTranslations;
using Newtonsoft.Json;
using Schema;
using Schema.RoadNetworkChanges;
using AcceptedChange = Schema.RoadNetworkChanges.AcceptedChange;
using ProblemWithChange = Schema.RoadNetworkChanges.ProblemWithChange;
using RejectedChange = Schema.RoadNetworkChanges.RejectedChange;

public class RoadNetworkChangeFeedProjection : ConnectedProjection<EditorContext>
{
    private static string FormattedTitle(bool isInformative, string description) => isInformative
        ? $"Informatieve extractaanvraag \"{description}\""
        : $"Extractaanvraag \"{description}\"";

    public RoadNetworkChangeFeedProjection(IBlobClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        When<Envelope<BeganRoadNetworkImport>>(async (context, envelope, ct) =>
            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = "Begonnen met importeren",
                    Type = nameof(BeganRoadNetworkImport),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<CompletedRoadNetworkImport>>(async (context, envelope, ct) =>
            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = "Klaar met importeren",
                    Type = nameof(CompletedRoadNetworkImport),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<RoadNetworkExtractGotRequested>>(async (context, envelope, ct) =>
            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"{FormattedTitle(envelope.Message.IsInformative, envelope.Message.Description)}: ontvangen",
                    Type = nameof(RoadNetworkExtractGotRequested),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"{FormattedTitle(envelope.Message.IsInformative, envelope.Message.Description)}: ontvangen",
                    Type = nameof(RoadNetworkExtractGotRequestedV2),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractDownloadBecameAvailableEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId },
                OverlapsWithDownloadIds = envelope.Message.OverlapsWithDownloadIds
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = $"{FormattedTitle(envelope.Message.IsInformative, envelope.Message.Description)}: download beschikbaar",
                Type = nameof(RoadNetworkExtractDownloadBecameAvailable),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);
        });

        When<Envelope<RoadNetworkExtractDownloadTimeoutOccurred>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractDownloadTimeoutOccurredEntry
            {
                RequestId = envelope.Message.RequestId,
                ExternalRequestId = envelope.Message.ExternalRequestId,
                Description = envelope.Message.Description
            };

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = $"{FormattedTitle(envelope.Message.IsInformative, envelope.Message.Description)}: download niet beschikbaar, contour te complex of te groot",
                Type = nameof(RoadNetworkExtractDownloadTimeoutOccurred),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);
        });

        When<Envelope<RoadNetworkChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkChangesArchiveUploadedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId },
                TicketId = envelope.Message.TicketId
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            var description = !string.IsNullOrEmpty(envelope.Message.Description) ? envelope.Message.Description : "onbekend";
            var changeRequestId = ChangeRequestId
                .FromArchiveId(new ArchiveId(envelope.Message.ArchiveId));

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);

            await context.RoadNetworkChangeRequestsBasedOnArchive.AddAsync(
                new RoadNetworkChangeRequestBasedOnArchive
                {
                    ChangeRequestId = changeRequestId
                        .ToBytes()
                        .ToArray(),
                    ArchiveId = envelope.Message.ArchiveId
                }, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractChangesArchiveUploadedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId },
                TicketId = envelope.Message.TicketId
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            var description = !string.IsNullOrEmpty(envelope.Message.Description) ? envelope.Message.Description : "onbekend";
            var changeRequestId = ChangeRequestId
                .FromArchiveId(new ArchiveId(envelope.Message.ArchiveId));

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": ontvangen",
                Type = nameof(RoadNetworkExtractChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);

            await context.RoadNetworkChangeRequestsBasedOnArchive.AddAsync(
                new RoadNetworkChangeRequestBasedOnArchive
                {
                    ChangeRequestId = changeRequestId
                        .ToBytes()
                        .ToArray(),
                    ArchiveId = envelope.Message.ArchiveId
                }, ct);
        });

        When<Envelope<RoadNetworkChangesArchiveRejected>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkChangesArchiveRejectedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId },
                Files = envelope.Message.Problems
                    .GroupBy(problem => problem.File)
                    .Select(group => new FileProblems
                    {
                        File = group.Key,
                        Problems = group
                            .Select(problem => new ProblemWithFile
                            {
                                Severity = problem.Severity.ToString(),
                                Text = FileProblemTranslator.Dutch(problem).Message
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            var description = envelope.Message.Description;
            var changeRequestId = ChangeRequestId
                .FromArchiveId(new ArchiveId(envelope.Message.ArchiveId));

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geweigerd",
                    Type = nameof(RoadNetworkChangesArchiveRejected),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveRejected>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractChangesArchiveRejectedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId },
                Files = envelope.Message.Problems
                    .GroupBy(problem => problem.File)
                    .Select(group => new FileProblems
                    {
                        File = group.Key,
                        Problems = group
                            .Select(problem => new ProblemWithFile
                            {
                                Severity = problem.Severity.ToString(),
                                Text = FileProblemTranslator.Dutch(problem).Message
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            var description = envelope.Message.Description;
            var changeRequestId = ChangeRequestId
                .FromArchiveId(new ArchiveId(envelope.Message.ArchiveId));

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geweigerd",
                    Type = nameof(RoadNetworkExtractChangesArchiveRejected),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            var changeRequestId = ChangeRequestId.FromString(envelope.Message.RequestId);

            var request = context.RoadNetworkChangeRequestsBasedOnArchive.Local
                              .FirstOrDefault(r =>
                                  r.ChangeRequestId == changeRequestId.ToBytes()
                                      .ToArray())
                          ?? context.RoadNetworkChangeRequestsBasedOnArchive.Find(changeRequestId.ToBytes().ToArray());

            var content = new RoadNetworkChangesBasedOnArchiveAcceptedEntry
            {
                Archive = new ArchiveInfo { Id = request?.ArchiveId },
                Summary = AcceptedChanges.Summarize(envelope.Message.Changes),
                Changes = envelope.Message.Changes
                    .Select(change => new AcceptedChange
                    {
                        Change = BackOffice.DutchTranslations.AcceptedChange.Translator(change),
                        Problems = change.Problems?
                            .Select(problem => new ProblemWithChange
                            {
                                Severity = problem.Severity.ToString(),
                                Text = ProblemTranslator.Dutch(problem).Message
                            })
                            .ToArray()
                            ?? []
                    })
                    .ToArray()
            };

            if (content.Archive.Id != null)
            {
                await EnrichWithArchiveInformation(content.Archive.Id, content.Archive, client, ct);
            }

            var description = envelope.Message.Reason;

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": aanvaard",
                    Type = nameof(RoadNetworkChangesAccepted) + ":v2",
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkChangesRejected>>(async (context, envelope, ct) =>
        {
            var changeRequestId = ChangeRequestId.FromString(envelope.Message.RequestId);

            var request = context.RoadNetworkChangeRequestsBasedOnArchive.Local
                              .FirstOrDefault(r =>
                                  r.ChangeRequestId == changeRequestId.ToBytes()
                                      .ToArray())
                          ?? context.RoadNetworkChangeRequestsBasedOnArchive.Find(changeRequestId.ToBytes().ToArray());
            var content = new RoadNetworkChangesBasedOnArchiveRejectedEntry
            {
                Archive = new ArchiveInfo { Id = request?.ArchiveId },
                Changes = envelope.Message.Changes
                    .Select(change => new RejectedChange
                    {
                        Change = BackOffice.DutchTranslations.RejectedChange.Translator(change),
                        Problems = change.Problems
                            .Select(problem => new ProblemWithChange
                            {
                                Severity = problem.Severity.ToString(),
                                Text = ProblemTranslator.Dutch(problem).Message
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            if (content.Archive.Id != null)
            {
                await EnrichWithArchiveInformation(content.Archive.Id, content.Archive, client, ct);
            }

            var description = envelope.Message.Reason;

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geweigerd",
                    Type = nameof(RoadNetworkChangesRejected),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<NoRoadNetworkChanges>>(async (context, envelope, ct) =>
        {
            var request = context.RoadNetworkChangeRequestsBasedOnArchive.Local
                              .FirstOrDefault(r =>
                                  r.ChangeRequestId == ChangeRequestId.FromString(envelope.Message.RequestId).ToBytes()
                                      .ToArray())
                          ?? context.RoadNetworkChangeRequestsBasedOnArchive.Find(ChangeRequestId
                              .FromString(envelope.Message.RequestId).ToBytes().ToArray());

            var content = new NoRoadNetworkChangesBasedOnArchiveEntry
            {
                Archive = new ArchiveInfo { Id = request?.ArchiveId }
            };

            if (content.Archive.Id != null)
            {
                await EnrichWithArchiveInformation(content.Archive.Id, content.Archive, client, ct);
            }

            var description = envelope.Message.Reason;
            var changeRequestId = envelope.Message.RequestId;

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extractaanvraag \"{description}\" - oplading \"{changeRequestId}\": geen wijzigingen gevonden",
                    Type = nameof(NoRoadNetworkChanges),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });
    }

    private static async Task EnrichWithArchiveInformation(string archiveId, ArchiveInfo archiveInfo, IBlobClient client, CancellationToken ct)
    {
        var blobName = new BlobName(archiveId);
        if (await client.BlobExistsAsync(blobName, ct))
        {
            var blob = await client.GetBlobAsync(blobName, ct);
            var metadata = blob.Metadata.Where(pair => pair.Key == new MetadataKey("filename")).ToArray();
            var filename = metadata.Length == 1 ? metadata[0].Value : archiveId + ".zip";
            archiveInfo.Available = true;
            archiveInfo.Filename = filename;
        }
        else
        {
            archiveInfo.Available = false;
            archiveInfo.Filename = "";
        }
    }
}
