namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Newtonsoft.Json;
    using Schema;

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

            When<Envelope<RoadNetworkChangesArchiveUploaded>>(async (context, envelope, ct) =>
            {
                var content = new RoadNetworkChangesArchiveUploadedEntry
                {
                    Archive = new RoadNetworkChangesArchiveInfo { Id = envelope.Message.ArchiveId }
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

            When<Envelope<RoadNetworkChangesArchiveAccepted>>(async (context, envelope, ct) =>
            {
                var content = new RoadNetworkChangesArchiveAcceptedEntry
                {
                    Archive = new RoadNetworkChangesArchiveInfo { Id = envelope.Message.ArchiveId },
                    Files = envelope.Message.Problems
                        .GroupBy(problem => problem.File)
                        .Select(group => new RoadNetworkChangesArchiveFile
                        {
                            File = group.Key,
                            Problems = group
                                .Select(problem => new RoadNetworkChangesArchiveFileProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = DutchTranslations.ProblemWithZipArchive.Translator(problem)
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

            When<Envelope<RoadNetworkChangesArchiveRejected>>(async (context, envelope, ct) =>
            {
                var content = new RoadNetworkChangesArchiveRejectedEntry
                {
                    Archive = new RoadNetworkChangesArchiveInfo { Id = envelope.Message.ArchiveId },
                    Files = envelope.Message.Problems
                        .GroupBy(problem => problem.File)
                        .Select(group => new RoadNetworkChangesArchiveFile
                        {
                            File = group.Key,
                            Problems = group
                                .Select(problem => new RoadNetworkChangesArchiveFileProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = DutchTranslations.ProblemWithZipArchive.Translator(problem)
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
                    Archive = new RoadNetworkChangesArchiveInfo { Id = request.ArchiveId },
                    Summary = DutchTranslations.AcceptedChanges.Summarize(envelope.Message.Changes),
                    Changes = envelope.Message.Changes
                        .Select(change => new RoadNetworkAcceptedChange
                        {
                            Change = DutchTranslations.AcceptedChange.Translator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = DutchTranslations.ProblemWithChange.Translator(problem)
                                })
                                .ToArray()
                        })
                        .ToArray()
                    };

                await EnrichWithArchiveInformation(request.ArchiveId, content.Archive, client, ct);

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
                    Archive = new RoadNetworkChangesArchiveInfo { Id = request.ArchiveId },
                    Changes = envelope.Message.Changes
                        .Select(change => new RoadNetworkRejectedChange
                        {
                            Change = DutchTranslations.RejectedChange.Translator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = DutchTranslations.ProblemWithChange.Translator(problem)
                                })
                                .ToArray()
                        })
                        .ToArray()
                };

                await EnrichWithArchiveInformation(request.ArchiveId, content.Archive, client, ct);

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
        }

        private static async Task EnrichWithArchiveInformation(string archiveId,
            RoadNetworkChangesArchiveInfo archiveInfo, IBlobClient client, CancellationToken ct)
        {
            var blobName = new BlobName(archiveId);
            if (await client.BlobExistsAsync(blobName, ct))
            {
                var blob = await client.GetBlobAsync(blobName, ct);
                archiveInfo.Available = true;
                archiveInfo.Filename = blob.Metadata.Single(pair => pair.Key == new MetadataKey("filename")).Value;
            }
            else
            {
                archiveInfo.Available = false;
                archiveInfo.Filename = "";
            }
        }
    }
}
