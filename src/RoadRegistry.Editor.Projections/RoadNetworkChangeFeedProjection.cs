namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Core;
    using BackOffice.Messages;
    using BackOffice.Uploads;
    using BackOffice.Uploads.Schema;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Newtonsoft.Json;
    using Schema;
    using AcceptedChange = BackOffice.Messages.AcceptedChange;
    using AddGradeSeparatedJunction = BackOffice.Messages.AddGradeSeparatedJunction;
    using AddRoadNode = BackOffice.Messages.AddRoadNode;
    using AddRoadSegment = BackOffice.Messages.AddRoadSegment;
    using AddRoadSegmentToEuropeanRoad = BackOffice.Messages.AddRoadSegmentToEuropeanRoad;
    using AddRoadSegmentToNationalRoad = BackOffice.Messages.AddRoadSegmentToNationalRoad;
    using AddRoadSegmentToNumberedRoad = BackOffice.Messages.AddRoadSegmentToNumberedRoad;
    using FileProblem = BackOffice.Messages.FileProblem;
    using Problem = BackOffice.Messages.Problem;
    using RejectedChange = BackOffice.Messages.RejectedChange;

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
                    Changes = envelope.Message.Changes
                        .Select(change => new RoadNetworkAcceptedChange
                        {
                            Change = AcceptedChangeTranslator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = ProblemWithChangeTranslator(problem)
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
                        Type = nameof(RoadNetworkChangesAccepted),
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
                            Change = RejectedChangeTranslator(change),
                            Problems = change.Problems
                                .Select(problem => new RoadNetworkChangeProblem
                                {
                                    Severity = problem.Severity.ToString(),
                                    Text = ProblemWithChangeTranslator(problem)
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

        private static readonly Converter<RejectedChange, string> RejectedChangeTranslator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case AddRoadNode m:
                        translation = $"Voeg wegknoop {m.TemporaryId} toe.";
                        break;
                    case AddRoadSegment m:
                        translation = $"Voeg wegsegment {m.TemporaryId} toe.";
                        break;
                    case AddRoadSegmentToEuropeanRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan europese weg {m.Number}.";
                        break;
                    case AddRoadSegmentToNationalRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.";
                        break;
                    case AddRoadSegmentToNumberedRoad m:
                        translation = $"Voeg wegsegment {m.SegmentId} toe aan nationale weg {m.Number}.";
                        break;
                    case AddGradeSeparatedJunction m:
                        translation = $"Voeg ongelijkgrondse kruising {m.TemporaryId} toe.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };

        private static readonly Converter<AcceptedChange, string> AcceptedChangeTranslator =
            change =>
            {
                string translation;
                switch (change.Flatten())
                {
                    case RoadNodeAdded m:
                        translation = $"Wegknoop {m.TemporaryId} toegevoegd.";
                        break;
                    case RoadSegmentAdded m:
                        translation = $"Wegsegment {m.TemporaryId} toegevoegd.";
                        break;
                    case RoadSegmentAddedToEuropeanRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan europese weg {m.Number}.";
                        break;
                    case RoadSegmentAddedToNationalRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Number}.";
                        break;
                    case RoadSegmentAddedToNumberedRoad m:
                        translation = $"Wegsegment {m.SegmentId} toegevoegd aan nationale weg {m.Number}.";
                        break;
                    case GradeSeparatedJunctionAdded m:
                        translation = $"Ongelijkgrondse kruising {m.TemporaryId} toegevoegd.";
                        break;

                    default:
                        translation = $"'{change.Flatten().GetType().Name}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };

        private static readonly Converter<Problem, string> ProblemWithChangeTranslator =
            problem =>
            {
                string translation;
                switch (problem.Reason)
                {
                    case nameof(RoadNodeGeometryTaken):
                        translation = $"De geometrie werd reeds ingenomen door een andere wegknoop met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadNodeTooClose):
                        translation = $"De geometrie ligt te dicht bij wegsegment met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadNodeNotConnectedToAnySegment):
                        translation = "De wegknoop is met geen enkel wegsegment verbonden.";
                        break;
                    case nameof(RoadNodeTypeMismatch):
                        translation = $"Het opgegeven wegknoop type {RoadNodeType.Parse(problem.Parameters[1].Value).Translation.Name} komt niet overeen met een van de verwachte wegknoop types:{string.Join(',', problem.Parameters.Skip(2).Select(parameter => RoadNodeType.Parse(parameter.Value).Translation.Name))}. De wegknoop is verbonden met {problem.Parameters[0].Value} wegsegment(-en).";
                        break;
                    case nameof(FakeRoadNodeConnectedSegmentsDoNotDiffer):
                        translation = $"De attributen van de verbonden wegsegmenten ({problem.Parameters[0].Value} en {problem.Parameters[1].Value}) verschillen onvoldoende voor deze schijnknoop.";
                        break;
                    case nameof(RoadSegmentGeometryTaken):
                        translation = $"De geometrie werd reeds ingenomen door een ander wegsegment met id {problem.Parameters[0].Value}.";
                        break;
                    case nameof(RoadSegmentStartNodeMissing):
                        translation = "De start wegknoop van het wegsegment ontbreekt.";
                        break;
                    case nameof(RoadSegmentEndNodeMissing):
                        translation = "De eind wegknoop van het wegsegment ontbreekt.";
                        break;
                    case nameof(RoadSegmentGeometryLengthIsZero):
                        translation = "De lengte van het wegsegment is 0.";
                        break;
                    case nameof(RoadSegmentStartPointDoesNotMatchNodeGeometry):
                        translation = "De positie van het start punt van het wegsegment stemt niet overeen met de geometrie van de start wegknoop.";
                        break;
                    case nameof(RoadSegmentEndPointDoesNotMatchNodeGeometry):
                        translation = "De positie van het eind punt van het wegsegment stemt niet overeen met de geometrie van de eind wegknoop.";
                        break;
                    case nameof(RoadSegmentGeometrySelfOverlaps):
                        translation = "De geometrie van het wegsegment overlapt zichzelf.";
                        break;
                    case nameof(RoadSegmentGeometrySelfIntersects):
                        translation = "De geometrie van het wegsegment kruist zichzelf.";
                        break;
                    case nameof(RoadSegmentMissing):
                        translation = $"Het wegsegment met id {problem.Parameters[0].Value} ontbreekt.";
                        break;
                    case nameof(RoadSegmentLaneAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[2].Value}) van het eerste rijstroken attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentLaneAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het rijstroken attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentLaneAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste rijstroken attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentWidthAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[2].Value}) van het eerste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentWidthAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het wegbreedte attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentWidthAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegbreedte attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentSurfaceAttributeFromPositionNotEqualToZero):
                        translation = $"De van positie ({problem.Parameters[2].Value}) van het eerste wegverharding attribuut met id {problem.Parameters[0].Value} is niet 0.0.";
                        break;
                    case nameof(RoadSegmentSurfaceAttributesNotAdjacent):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het wegverharding attribuut met id {problem.Parameters[0].Value} sluit niet aan op de van positie ({problem.Parameters[3].Value}) van het rijstroken attribuut met id {problem.Parameters[2].Value}.";
                        break;
                    case nameof(RoadSegmentSurfaceAttributeToPositionNotEqualToLength):
                        translation = $"De tot positie ({problem.Parameters[1].Value}) van het laatste wegverharding attribuut met id {problem.Parameters[0].Value} is niet de lengte van het wegsegment ({problem.Parameters[2].Value}).";
                        break;
                    case nameof(RoadSegmentPointMeasureValueOutOfRange):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] ligt niet binnen de verwachte grenzen [{problem.Parameters[2].Value}-{problem.Parameters[2].Value}].";
                        break;
                    case nameof(RoadSegmentStartPointMeasureValueNotEqualToZero):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het start punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet 0.0.";
                        break;
                    case nameof(RoadSegmentEndPointMeasureValueNotEqualToLength):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het eind punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet gelijk aan de lengte ({problem.Parameters[3].Value}).";
                        break;
                    case nameof(RoadSegmentPointMeasureValueDoesNotIncrease):
                        translation = $"De meting ({problem.Parameters[2].Value}) op het punt [X={problem.Parameters[0].Value},Y={problem.Parameters[1].Value}] is niet groter dan de meting ({problem.Parameters[3].Value}) op het vorige punt.";
                        break;
                    case nameof(GradeSeparatedJunctionNotFound):
                        translation = "De ongelijkgrondse kruising kon niet worden gevonden.";
                        break;
                    case nameof(UpperRoadSegmentMissing):
                        translation = "Het bovenste wegsegment ontbreekt.";
                        break;
                    case nameof(LowerRoadSegmentMissing):
                        translation = "Het onderste wegsegment ontbreekt.";
                        break;
                    case nameof(UpperAndLowerRoadSegmentDoNotIntersect):
                        translation = "Het bovenste en onderste wegsegment kruisen elkaar niet.";
                        break;
                    default:
                        translation = $"'{problem.Reason}' has no translation. Please fix it.";
                        break;
                }

                return translation;
            };

        private static readonly Converter<FileProblem, string> ProblemWithZipArchiveTranslator =
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
                            $"De dbase record {problem.Parameters[0].Value} bevat een ongeldige wegcategorie in veld {nameof(RoadSegmentChangeDbaseRecord.Schema.CATEGORIE)}: {problem.Parameters[2].Value}. Verwachte 1 van volgende waarden: {problem.Parameters[1].Value}.";
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
