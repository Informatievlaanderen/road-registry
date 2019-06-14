namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Newtonsoft.Json;
    using Schema;
    using Translation;

    public class RoadNetworkChangeFeedProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNetworkChangeFeedProjection()
        {
            When<Envelope<BeganRoadNetworkImport>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Begonnen met importeren",
                        Type = nameof(BeganRoadNetworkImport),
                        Content = null
                    }, ct));

            When<Envelope<CompletedRoadNetworkImport>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Klaar met importeren",
                        Type = nameof(CompletedRoadNetworkImport),
                        Content = null
                    }, ct));

            When<Envelope<RoadNetworkChangesArchiveUploaded>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Oplading bestand ontvangen",
                        Type = nameof(RoadNetworkChangesArchiveUploaded),
                        Content = JsonConvert.SerializeObject(
                            new RoadNetworkChangesArchiveUploadedEntry
                            {
                                ArchiveId = envelope.Message.ArchiveId
                            })
                    }, ct));

            When<Envelope<RoadNetworkChangesArchiveAccepted>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Oplading bestand werd aanvaard",
                        Type = nameof(RoadNetworkChangesArchiveAccepted),
                        Content = JsonConvert.SerializeObject(
                            new RoadNetworkChangesArchiveAcceptedEntry
                            {
                                ArchiveId = envelope.Message.ArchiveId,
                                Files = envelope.Message.Problems
                                    .GroupBy(problem => problem.File)
                                    .Select(group => new RoadNetworkChangesArchiveFile
                                    {
                                        File = group.Key,
                                        Problems = group
                                            .Select(problem => ProblemWithZipArchiveTranslator(problem))
                                            .Where(translation => translation != null)
                                            .ToArray()
                                    })
                                    .ToArray()
                            })
                    }, ct));


            When<Envelope<RoadNetworkChangesArchiveRejected>>((context, envelope, ct) =>
                context.RoadNetworkChanges.AddAsync(
                    new RoadNetworkChange
                    {
                        Id = envelope.Position,
                        Title = "Oplading bestand werd geweigerd",
                        Type = nameof(RoadNetworkChangesArchiveRejected),
                        Content = JsonConvert.SerializeObject(
                            new RoadNetworkChangesArchiveRejectedEntry
                            {
                                ArchiveId = envelope.Message.ArchiveId,
                                Files = envelope.Message.Problems
                                    .GroupBy(problem => problem.File)
                                    .Select(group => new RoadNetworkChangesArchiveFile
                                    {
                                        File = group.Key,
                                        Problems = group
                                            .Select(problem => ProblemWithZipArchiveTranslator(problem))
                                            .Where(translation => translation != null)
                                            .ToArray()
                                    })
                                    .ToArray()
                            })
                    }, ct));
        }

        private static readonly Converter<Messages.FileProblem, string> ProblemWithZipArchiveTranslator =
            problem =>
            {
                var result = string.Empty;
                switch (problem.Reason)
                {
                    case nameof(ZipArchiveProblems.RequiredFileMissing):
                        result = $"Het bestand ontbreekt in het archief.";
                        break;

                    case nameof(ZipArchiveProblems.NoDbaseRecords):
                        result = $"Het bestand bevat geen rijen.";
                        break;

                    case nameof(ZipArchiveProblems.DbaseHeaderFormatError):
                        result = $"De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(ZipArchiveProblems.DbaseRecordFormatError):
                        result =
                            $"De dbase record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(ZipArchiveProblems.NoShapeRecords):
                        result = $"Het bestand bevat geen enkele geometrie.";
                        break;

                    case nameof(ZipArchiveProblems.ShapeHeaderFormatError):
                        result = $"De hoofding van het bestand is niet correct geformateerd.";
                        break;

                    case nameof(ZipArchiveProblems.ShapeRecordFormatError):
                        result =
                            $"De shape record na record {problem.Parameters[0].Value} is niet correct geformateerd.";
                        break;

                    case nameof(ZipArchiveProblems.ShapeRecordShapeTypeMismatch):
                        result =
                            $"De shape record {problem.Parameters[0].Value} bevat geen {problem.Parameters[1].Value} maar een {problem.Parameters[2].Value}.";
                        break;

                    case nameof(ZipArchiveProblems.ShapeRecordGeometryMismatch):
                        result = $"De shape record {problem.Parameters[0].Value} geometrie is ongeldig.";
                        break;

                    case nameof(ZipArchiveProblems.IdentifierZero):
                        result = $"De dbase record {problem.Parameters[0].Value} bevat een identificator die 0 is.";
                        break;

                    case nameof(ZipArchiveProblems.IdentifierMissing):
                        result = $"De dbase record {problem.Parameters[0].Value} ontbreekt een identificator.";
                        break;

                    case nameof(ZipArchiveProblems.IdentifierNotUnique):
                        result =
                            $"De dbase record {problem.Parameters[1].Value} bevat dezelfde identifier {problem.Parameters[0].Value} als dbase record {problem.Parameters[2].Value}.";
                        break;

                    case nameof(ZipArchiveProblems.NotEuropeanRoadNumber):
                        result =
                            $"De dbase record {problem.Parameters[1].Value} bevat een nummer {problem.Parameters[0].Value} dat geen europees wegnummer is.";
                        break;

                    case nameof(ZipArchiveProblems.DbaseSchemaMismatch):
                        result =
                            $"Het verwachte dbase schema {problem.Parameters[0].Value} stemt niet overeen met het eigenlijke dbase schema {problem.Parameters[1].Value}.";
                        break;
                }

                return result;
            };
    }
}
