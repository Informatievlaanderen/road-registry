namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Text;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Microsoft.IO;
    using Schema;
    using Schema.GradeSeparatedJunctions;

    public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<BackOfficeContext>
    {
        public GradeSeparatedJunctionRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
            {
                var translation = GradeSeparatedJunctionType.Parse(envelope.Message.Type).Translation;
                var junctionRecord = new GradeSeparatedJunctionRecord
                {
                    Id = envelope.Message.Id,
                    DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                    {
                        OK_OIDN = {Value = envelope.Message.Id},
                        TYPE = {Value = translation.Identifier},
                        LBLTYPE = {Value = translation.Name},
                        BO_WS_OIDN = {Value = envelope.Message.UpperRoadSegmentId},
                        ON_WS_OIDN = {Value = envelope.Message.LowerRoadSegmentId},
                        BEGINTIJD = {Value = envelope.Message.Origin.Since},
                        BEGINORG = {Value = envelope.Message.Origin.OrganizationId},
                        LBLBGNORG = {Value = envelope.Message.Origin.Organization},
                    }.ToBytes(manager, encoding)
                };

                await context.AddAsync(junctionRecord, token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case GradeSeparatedJunctionAdded junction:
                            var translation = GradeSeparatedJunctionType.Parse(junction.Type).Translation;
                            var junctionRecord = new GradeSeparatedJunctionRecord
                            {
                                Id = junction.Id,
                                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                                {
                                    OK_OIDN = {Value = junction.Id},
                                    TYPE = {Value = translation.Identifier},
                                    LBLTYPE = {Value = translation.Name},
                                    BO_WS_OIDN = {Value = junction.UpperRoadSegmentId},
                                    ON_WS_OIDN = {Value = junction.LowerRoadSegmentId},
                                    BEGINTIJD =
                                    {
                                        Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                                    },
                                    BEGINORG = {Value = envelope.Message.OrganizationId},
                                    LBLBGNORG = {Value = envelope.Message.Organization},
                                }.ToBytes(manager, encoding)
                            };

                            await context.AddAsync(junctionRecord, token);
                            break;
                    }
                }
            });
        }
    }
}
