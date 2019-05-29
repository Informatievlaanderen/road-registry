namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Schema;
    using Translation;

    public class RoadNetworkActivityProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNetworkActivityProjection()
        {
            When<Envelope<RoadNetworkChangesArchiveUploaded>>((context, envelope, ct) =>
                context.RoadNetworkActivities.AddAsync(
                    new RoadNetworkActivity
                    {
                        Title = "Oplading bestand ontvangen",
                        Type = nameof(RoadNetworkChangesArchiveUploaded),
                        RelatesToArchiveId = envelope.Message.ArchiveId,
                        Errors = null,
                        Warnings = null
                    }, ct));

            When<Envelope<RoadNetworkChangesArchiveAccepted>>((context, envelope, ct) =>
                context.RoadNetworkActivities.AddAsync(
                    new RoadNetworkActivity
                    {
                        Title = "Oplading bestand werd aanvaard",
                        Type = nameof(RoadNetworkChangesArchiveAccepted),
                        RelatesToArchiveId = envelope.Message.ArchiveId,
                        Errors = null,
                        Warnings =
                            Array.FindAll(
                                Array.ConvertAll(envelope.Message.Warnings,ProblemWithZipArchiveTranslator),
                                translation => translation != null
                            )
                    }, ct));


            When<Envelope<RoadNetworkChangesArchiveRejected>>((context, envelope, ct) =>
                context.RoadNetworkActivities.AddAsync(
                    new RoadNetworkActivity
                    {
                        Title = "Oplading bestand werd geweigerd",
                        Type = nameof(RoadNetworkChangesArchiveRejected),
                        RelatesToArchiveId = envelope.Message.ArchiveId,
                        Errors = Array.FindAll(
                            Array.ConvertAll(envelope.Message.Errors,ProblemWithZipArchiveTranslator),
                            translation => translation != null
                        ),
                        Warnings =
                            Array.FindAll(
                                Array.ConvertAll(envelope.Message.Warnings,ProblemWithZipArchiveTranslator),
                                translation => translation != null
                            )
                    }, ct));

            When<Envelope<RoadNetworkChangesAccepted>>((context, envelope, ct) =>
                context.RoadNetworkActivities.AddAsync(
                    new RoadNetworkActivity
                    {
                        Title = "Oplading werd aanvaard",
                        Type = nameof(RoadNetworkChangesAccepted),
                        Errors = null,
                        Warnings =
                            Array.FindAll(
                                Array.ConvertAll(envelope.Message.Changes, ProblemWithRequestedChangeTranslator),
                                translation => translation != null
                            )
                    }, ct));

            When<Envelope<RoadNetworkChangesRejected>>((context, envelope, ct) =>
                context.RoadNetworkActivities.AddAsync(
                    new RoadNetworkActivity
                    {
                        Title = "Oplading werd geweigerd",
                        Type = nameof(RoadNetworkChangesRejected),
                        Errors = null,
                        Warnings = null
                    }, ct));
        }

        private static readonly Converter<Messages.FileProblem, string> ProblemWithZipArchiveTranslator =
            problem =>
            {
                var result = string.Empty;
                switch (problem.Reason)
                {
                    case nameof(ZipArchiveProblems.RequiredFileMissing):
                        result = $"Het vereist bestand '{problem.Parameters[0].Value}' ontbreekt in het archief.";
                        break;

                    case nameof(ZipArchiveProblems.NoDbaseRecords):
                        result = $"Het bestand '{problem.Parameters[0].Value}' bevat geen rijen.";
                        break;

                    case nameof(ZipArchiveProblems.DbaseHeaderFormatError):
                        result = $"De hoofding van het bestand '{problem.Parameters[0].Value}' is niet correct geformateerd.";
                        break;

                    case nameof(ZipArchiveProblems.NoShapeRecords):
                        result = $"Het bestand '{problem.Parameters[0].Value}' bevat geen geometrien.";
                        break;

                    case nameof(ZipArchiveProblems.ShapeHeaderFormatError):
                        result = $"De hoofding van het bestand '{problem.Parameters[0].Value}' is niet correct geformateerd.";
                        break;
                }

                return result;
            };

        private static readonly Converter<AcceptedChange, string> ProblemWithRequestedChangeTranslator =
            change =>
            {
                var result = string.Empty;
                switch (change.Flatten())
                {

                }

                switch (change.Flatten())
                {

                }

                return result;
            };
    }
}
