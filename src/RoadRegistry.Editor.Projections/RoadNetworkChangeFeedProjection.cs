namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using DutchTranslations;
using Newtonsoft.Json;
using Schema;
using Schema.RoadNetworkChanges;
using AcceptedChange = Schema.RoadNetworkChanges.AcceptedChange;
using ProblemWithChange = Schema.RoadNetworkChanges.ProblemWithChange;
using RejectedChange = Schema.RoadNetworkChanges.RejectedChange;

public class RoadNetworkChangeFeedProjection : ConnectedProjection<EditorContext>
{
    public RoadNetworkChangeFeedProjection(IBlobClient client)
    {
        if (client == null) throw new ArgumentNullException(nameof(client));

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
                    Title = $"Extract aanvraag {envelope.Message.ExternalRequestId} voor download {envelope.Message.DownloadId:N} ontvangen",
                    Type = nameof(RoadNetworkExtractGotRequested),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<RoadNetworkExtractGotRequestedV2>>(async (context, envelope, ct) =>
            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Extract aanvraag {envelope.Message.ExternalRequestId} voor download {envelope.Message.DownloadId:N} met omschrijving {envelope.Message.Description} ontvangen",
                    Type = nameof(RoadNetworkExtractGotRequestedV2),
                    Content = null,
                    When = envelope.Message.When
                }, ct));

        When<Envelope<RoadNetworkExtractDownloadBecameAvailable>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractDownloadBecameAvailableEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId }
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = $"Download {envelope.Message.DownloadId:N} van extract aanvraag {envelope.Message.ExternalRequestId} werd beschikbaar",
                Type = nameof(RoadNetworkExtractDownloadBecameAvailable),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);
        });

        When<Envelope<RoadNetworkChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkChangesArchiveUploadedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId }
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title = "Oplading bestand ontvangen",
                Type = nameof(RoadNetworkChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);

            await context.RoadNetworkChangeRequestsBasedOnArchive.AddAsync(
                new RoadNetworkChangeRequestBasedOnArchive
                {
                    ChangeRequestId = ChangeRequestId
                        .FromArchiveId(new ArchiveId(envelope.Message.ArchiveId))
                        .ToBytes()
                        .ToArray(),
                    ArchiveId = envelope.Message.ArchiveId
                }, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveUploaded>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractChangesArchiveUploadedEntry
            {
                Archive = new ArchiveInfo { Id = envelope.Message.ArchiveId }
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(new RoadNetworkChange
            {
                Id = envelope.Position,
                Title =
                    $"Oplading bestand voor download {envelope.Message.DownloadId:N} van extract aanvraag {envelope.Message.ExternalRequestId} ontvangen",
                Type = nameof(RoadNetworkExtractChangesArchiveUploaded),
                Content = JsonConvert.SerializeObject(content),
                When = envelope.Message.When
            }, ct);

            await context.RoadNetworkChangeRequestsBasedOnArchive.AddAsync(
                new RoadNetworkChangeRequestBasedOnArchive
                {
                    ChangeRequestId = ChangeRequestId
                        .FromUploadId(new UploadId(envelope.Message.UploadId))
                        .ToBytes()
                        .ToArray(),
                    ArchiveId = envelope.Message.ArchiveId
                }, ct);
        });

        When<Envelope<RoadNetworkChangesArchiveAccepted>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkChangesArchiveAcceptedEntry
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
                                Text = ProblemWithZipArchive.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = "Oplading bestand werd aanvaard",
                    Type = nameof(RoadNetworkChangesArchiveAccepted),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkExtractChangesArchiveAccepted>>(async (context, envelope, ct) =>
        {
            var content = new RoadNetworkExtractChangesArchiveAcceptedEntry
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
                                Text = ProblemWithZipArchive.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Oplading bestand voor download {envelope.Message.DownloadId:N} van extract aanvraag {envelope.Message.ExternalRequestId} werd aanvaard",
                    Type = nameof(RoadNetworkExtractChangesArchiveAccepted),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
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
                                Text = ProblemWithZipArchive.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = "Oplading bestand werd geweigerd",
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
                                Text = ProblemWithZipArchive.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            await EnrichWithArchiveInformation(envelope.Message.ArchiveId, content.Archive, client, ct);

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Oplading bestand voor download {envelope.Message.DownloadId:N} van extract aanvraag {envelope.Message.ExternalRequestId} werd geweigerd",
                    Type = nameof(RoadNetworkExtractChangesArchiveRejected),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, ct) =>
        {
            var request = context.RoadNetworkChangeRequestsBasedOnArchive.Local
                              .FirstOrDefault(r =>
                                  r.ChangeRequestId == ChangeRequestId.FromString(envelope.Message.RequestId).ToBytes()
                                      .ToArray())
                          ?? context.RoadNetworkChangeRequestsBasedOnArchive.Find(ChangeRequestId
                              .FromString(envelope.Message.RequestId).ToBytes().ToArray());
            var content = new RoadNetworkChangesBasedOnArchiveAcceptedEntry
            {
                Archive = new ArchiveInfo { Id = request?.ArchiveId },
                Summary = AcceptedChanges.Summarize(envelope.Message.Changes),
                Changes = envelope.Message.Changes
                    .Select(change => new AcceptedChange
                    {
                        Change = DutchTranslations.AcceptedChange.Translator(change),
                        Problems = change.Problems
                            .Select(problem => new ProblemWithChange
                            {
                                Severity = problem.Severity.ToString(),
                                Text = DutchTranslations.ProblemWithChange.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            if (content.Archive.Id != null)
            {
                await EnrichWithArchiveInformation(content.Archive.Id, content.Archive, client, ct);
            }

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Oplading \"{envelope.Message.Reason}\" door {envelope.Message.Organization} ({envelope.Message.Operator}) werd aanvaard",
                    Type = nameof(RoadNetworkChangesAccepted) + ":v2",
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });

        When<Envelope<RoadNetworkChangesRejected>>(async (context, envelope, ct) =>
        {
            var request = context.RoadNetworkChangeRequestsBasedOnArchive.Local
                              .FirstOrDefault(r =>
                                  r.ChangeRequestId == ChangeRequestId.FromString(envelope.Message.RequestId).ToBytes()
                                      .ToArray())
                          ?? context.RoadNetworkChangeRequestsBasedOnArchive.Find(ChangeRequestId
                              .FromString(envelope.Message.RequestId).ToBytes().ToArray());
            var content = new RoadNetworkChangesBasedOnArchiveRejectedEntry
            {
                Archive = new ArchiveInfo { Id = request?.ArchiveId },
                Changes = envelope.Message.Changes
                    .Select(change => new RejectedChange
                    {
                        Change = DutchTranslations.RejectedChange.Translator(change),
                        Problems = change.Problems
                            .Select(problem => new ProblemWithChange
                            {
                                Severity = problem.Severity.ToString(),
                                Text = DutchTranslations.ProblemWithChange.Translator(problem)
                            })
                            .ToArray()
                    })
                    .ToArray()
            };

            if (content.Archive.Id != null)
            {
                await EnrichWithArchiveInformation(content.Archive.Id, content.Archive, client, ct);
            }

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Oplading \"{envelope.Message.Reason}\" door {envelope.Message.Organization} ({envelope.Message.Operator}) werd geweigerd",
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

            await context.RoadNetworkChanges.AddAsync(
                new RoadNetworkChange
                {
                    Id = envelope.Position,
                    Title = $"Geen wijzigingen in oplading \"{envelope.Message.Reason}\" door {envelope.Message.Organization} ({envelope.Message.Operator})",
                    Type = nameof(NoRoadNetworkChanges),
                    Content = JsonConvert.SerializeObject(content),
                    When = envelope.Message.When
                }, ct);
        });
    }

    private static async Task EnrichWithArchiveInformation(string archiveId,
        ArchiveInfo archiveInfo, IBlobClient client, CancellationToken ct)
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
