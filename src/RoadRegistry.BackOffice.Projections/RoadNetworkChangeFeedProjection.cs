namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Model;
    using Newtonsoft.Json;
    using Schema;
    using Translation;

    public class RoadNetworkChangeFeedProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNetworkChangeFeedProjection(IBlobClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            When<Envelope<BeganRoadNetworkImport>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Begonnen met importeren",
                        Type = nameof(BeganRoadNetworkImport),
                        Content = null,
                        When = envelope.Message.When
                    }, ct));

            When<Envelope<CompletedRoadNetworkImport>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
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
                                    Text = ProblemWithZipArchiveTranslator(problem)
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
                                    Text = ProblemWithZipArchiveTranslator(problem)
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

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, ct) =>
            {
                var content = new RoadNetworkChangesBasedOnArchiveAcceptedEntry
                {
                    Archive = new RoadNetworkChangesArchiveInfo { Id = envelope.Message.ArchiveId },
                    Changes = envelope.Message.Changes
                        .Select(change => new RoadNetworkAcceptedChange
                        {
                            Change = AcceptedChangeTranslator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = "Error",
                                    Text = problem.Reason
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
                        Title = "Oplading werd aanvaard",
                        Type = nameof(RoadNetworkChangesBasedOnArchiveAccepted),
                        Content = JsonConvert.SerializeObject(content),
                        When = envelope.Message.When
                    }, ct);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveRejected>>(async (context, envelope, ct) =>
            {
                var content = new RoadNetworkChangesBasedOnArchiveRejectedEntry
                {
                    Archive = new RoadNetworkChangesArchiveInfo { Id = envelope.Message.ArchiveId },
                    Changes = envelope.Message.Changes
                        .Select(change => new RoadNetworkRejectedChange
                        {
                            Change = RejectedChangeTranslator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = "Error",
                                    Text = problem.Reason
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
                        Title = "Wijziging op basis van oplading werd geweigerd",
                        Type = nameof(RoadNetworkChangesBasedOnArchiveRejected),
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

        private static readonly Converter<Messages.RejectedChange, string> RejectedChangeTranslator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case Messages.AddRoadNode m:
                        translation = $"Voeg wegknoop {m.TemporaryId} toe.";
                        break;
                    case Messages.AddRoadSegment m:
                        translation = $"Voeg wegsegment {m.TemporaryId} toe.";
                        break;
                    case Messages.AddRoadSegmentToEuropeanRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan europese weg {m.Number}.";
                        break;
                    case Messages.AddRoadSegmentToNationalRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Ident2}.";
                        break;
                    case Messages.AddRoadSegmentToNumberedRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Ident8}.";
                        break;
                    case Messages.AddGradeSeparatedJunction m:
                        translation = $"Voeg ongelijkgrondse kruising {m.TemporaryId} toe.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };

        private static readonly Converter<Messages.AcceptedChange, string> AcceptedChangeTranslator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case Messages.RoadNodeAdded m:
                        translation = $"Wegknoop {m.TemporaryId} toegevoegd.";
                        break;
                    case Messages.RoadSegmentAdded m:
                        translation = $"Wegsegment {m.TemporaryId} toegevoegd.";
                        break;
                    case Messages.RoadSegmentAddedToEuropeanRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan europese weg {m.Number}.";
                        break;
                    case Messages.RoadSegmentAddedToNationalRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Ident2}.";
                        break;
                    case Messages.RoadSegmentAddedToNumberedRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Ident8}.";
                        break;
                    case Messages.GradeSeparatedJunctionAdded m:
                        translation = $"Ongelijkgrondse kruising {m.TemporaryId} toegevoegd.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };

        private static readonly Converter<Messages.FileProblem, string> ProblemWithZipArchiveTranslator =
            problem =>
            {
                string translation;
                switch (problem.Reason)
                {
                    case nameof(ZipArchiveProblems.RequiredFileMissing):
                        translation = "Het bestand ontbreekt in het archief.";
                        break;

                    case nameof(DbaseFileProblems.HasNoDbaseRecords):
                        translation = "Het bestand bevat geen rijen.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseHeaderFormatError):
                        translation = "De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseSchemaMismatch):
                        translation =
                            $"Het verwachte dbase schema {problem.Parameters[0].Value} stemt niet overeen met het eigenlijke dbase schema {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.HasDbaseRecordFormatError):
                        translation =
                            $"De dbase record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(DbaseFileProblems.IdentifierZero):
                        translation = $"De dbase record {problem.Parameters[0].Value} bevat een identificator die 0 is.";
                        break;

                    case nameof(DbaseFileProblems.RequiredFieldIsNull):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft geen waarde voor veld {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.IdentifierNotUnique):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat dezelfde identifier {problem.Parameters[1].Value} als dbase record {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige wegsegment identificator: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NotNumberedRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen genummerd wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.NotEuropeanRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen europees wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.NotNationalRoadNumber):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een nummer {problem.Parameters[1].Value} dat geen nationaal wegnummer is.";
                        break;

                    case nameof(DbaseFileProblems.FromPositionOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige van positie: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.ToPositionOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige tot positie: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.GradeSeparatedJunctionTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type ongelijkgrondse kruising in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.UpperRoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige bovenliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.BO_WS_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LowerRoadSegmentIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige onderliggende wegsegment identificator in veld {nameof(GradeSeparatedJunctionChangeDbaseRecord.Schema.ON_WS_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NumberedRoadOrdinalOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldig volgnummer in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.VOLGNUMMER)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.NumberedRoadDirectionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige richting in veld {nameof(NumberedRoadChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadNodeTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegknoop in veld {nameof(RoadNodeChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentAccessRestrictionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige toegangsbeperking in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.TGBEP)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentStatusMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige status in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.STATUS)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentCategoryMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.WEGCAT)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentGeometryDrawMethodMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige methode in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.METHODE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RoadSegmentMorphologyMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige morfologie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.MORFOLOGIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.BeginRoadNodeIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige begin wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.B_WK_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.EndRoadNodeIdOutOfRange):
                        translation = $"De dbase record {problem.Parameters[0].Value} heeft een ongeldige eind wegknoop in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.E_WK_OIDN)}: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LaneCountOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig rijstrook aantal in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.AANTAL)}: {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.LaneDirectionMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige rijstrook richting in veld {nameof(RoadSegmentLaneChangeDbaseRecord.Schema.RICHTING)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.SurfaceTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig type wegverharding in veld {nameof(RoadSegmentSurfaceChangeDbaseRecord.Schema.TYPE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(DbaseFileProblems.WidthOutOfRange):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegbreedte in veld {nameof(RoadSegmentWidthChangeDbaseRecord.Schema.BREEDTE)}: {problem.Parameters[2].Value}.";
                        break;

                    case nameof(DbaseFileProblems.RecordTypeMismatch):
                        translation =
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldig record type in veld RECORDTYPE: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
                        break;

                    case nameof(ShapeFileProblems.HasNoShapeRecords):
                        translation = "Het bestand bevat geen enkele geometrie.";
                        break;

                    case nameof(ShapeFileProblems.ShapeHeaderFormatError):
                        translation = "De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(ShapeFileProblems.HasShapeRecordFormatError):
                        translation =
                            $"De shape record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordShapeTypeMismatch):
                        translation =
                            $"De shape record {problem.Parameters[0].Value} bevat geen {problem.Parameters[1].Value} maar een {problem.Parameters[2].Value}.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometryMismatch):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie is ongeldig.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometryLineCountMismatch):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie heeft meer lijnen dan verwacht.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometrySelfOverlaps):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie overlapt zichzelf.";
                        break;

                    case nameof(ShapeFileProblems.ShapeRecordGeometrySelfIntersects):
                        translation = $"De shape record {problem.Parameters[0].Value} geometrie kruist zichzelf.";
                        break;

                    default:
                        translation = $"'{problem.Reason}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };
    }
}
