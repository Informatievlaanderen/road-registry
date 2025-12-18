namespace RoadRegistry.Editor.Projections;

using System;
using System.Text;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;
using Microsoft.IO;
using Schema;
using Schema.Extensions;
using Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<EditorContext>
{
    public GradeSeparatedJunctionRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        When<Envelope<ImportedGradeSeparatedJunction>>(async (context, envelope, token) =>
        {
            var translation = GradeSeparatedJunctionType.Parse(envelope.Message.Type).Translation;
            var junctionRecord = new GradeSeparatedJunctionRecord
            {
                Id = envelope.Message.Id,
                UpperRoadSegmentId = envelope.Message.UpperRoadSegmentId,
                LowerRoadSegmentId = envelope.Message.LowerRoadSegmentId,
                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                {
                    OK_OIDN = { Value = envelope.Message.Id },
                    TYPE = { Value = translation.Identifier },
                    LBLTYPE = { Value = translation.Name },
                    BO_WS_OIDN = { Value = envelope.Message.UpperRoadSegmentId },
                    ON_WS_OIDN = { Value = envelope.Message.LowerRoadSegmentId },
                    BEGINTIJD = { Value = envelope.Message.Origin.Since },
                    BEGINORG = { Value = envelope.Message.Origin.OrganizationId },
                    LBLBGNORG = { Value = envelope.Message.Origin.Organization }
                }.ToBytes(manager, encoding)
            };

            await context.AddAsync(junctionRecord, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case GradeSeparatedJunctionAdded added:
                        {
                            var translation = GradeSeparatedJunctionType.Parse(added.Type).Translation;
                            var junction = new GradeSeparatedJunctionRecord
                            {
                                Id = added.Id,
                                UpperRoadSegmentId = added.UpperRoadSegmentId,
                                LowerRoadSegmentId = added.LowerRoadSegmentId,
                                DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                                {
                                    OK_OIDN = { Value = added.Id },
                                    TYPE = { Value = translation.Identifier },
                                    LBLTYPE = { Value = translation.Name },
                                    BO_WS_OIDN = { Value = added.UpperRoadSegmentId },
                                    ON_WS_OIDN = { Value = added.LowerRoadSegmentId },
                                    BEGINTIJD =
                                {
                                    Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                                },
                                    BEGINORG = { Value = envelope.Message.OrganizationId },
                                    LBLBGNORG = { Value = envelope.Message.Organization }
                                }.ToBytes(manager, encoding)
                            };

                            await context.AddAsync(junction, token);
                        }
                        break;
                    case GradeSeparatedJunctionModified modified:
                        {
                            var junction =
                                await context.GradeSeparatedJunctions.FindAsync(modified.Id, cancellationToken: token);

                            var translation = GradeSeparatedJunctionType.Parse(modified.Type).Translation;
                            junction.UpperRoadSegmentId = modified.UpperRoadSegmentId;
                            junction.LowerRoadSegmentId = modified.LowerRoadSegmentId;
                            junction.DbaseRecord = new GradeSeparatedJunctionDbaseRecord
                            {
                                OK_OIDN = { Value = modified.Id },
                                TYPE = { Value = translation.Identifier },
                                LBLTYPE = { Value = translation.Name },
                                BO_WS_OIDN = { Value = modified.UpperRoadSegmentId },
                                ON_WS_OIDN = { Value = modified.LowerRoadSegmentId },
                                BEGINTIJD =
                            {
                                Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                            },
                                BEGINORG = { Value = envelope.Message.OrganizationId },
                                LBLBGNORG = { Value = envelope.Message.Organization }
                            }.ToBytes(manager, encoding);
                        }
                        break;
                    case GradeSeparatedJunctionRemoved removed:
                        {
                            var junctionRecord = await context.GradeSeparatedJunctions.FindAsync(removed.Id, cancellationToken: token);
                            if (junctionRecord is not null)
                            {
                                context.GradeSeparatedJunctions.Remove(junctionRecord);
                            }
                        }
                        break;
                }
        });
    }
}
