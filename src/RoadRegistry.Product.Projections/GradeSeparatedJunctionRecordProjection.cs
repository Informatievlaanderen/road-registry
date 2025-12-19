namespace RoadRegistry.Product.Projections;

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extracts.Schemas.ExtractV1.GradeSeparatedJuntions;
using Microsoft.IO;
using Schema;
using Schema.GradeSeparatedJunctions;

public class GradeSeparatedJunctionRecordProjection : ConnectedProjection<ProductContext>
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
                    case GradeSeparatedJunctionAdded junctionAdded:
                        await AddJunction(manager, encoding, context, envelope, junctionAdded, token);
                        break;
                    case GradeSeparatedJunctionModified junctionModified:
                        await ModifyJunction(manager, encoding, context, envelope, junctionModified, token);
                        break;
                    case GradeSeparatedJunctionRemoved junctionRemoved:
                        await RemoveJunction(context, junctionRemoved, token);
                        break;
                }
        });
    }

    private static async Task AddJunction(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        GradeSeparatedJunctionAdded junction,
        CancellationToken token)
    {
        var translation = GradeSeparatedJunctionType.Parse(junction.Type).Translation;
        var junctionRecord = new GradeSeparatedJunctionRecord
        {
            Id = junction.Id,
            DbaseRecord = new GradeSeparatedJunctionDbaseRecord
            {
                OK_OIDN = { Value = junction.Id },
                TYPE = { Value = translation.Identifier },
                LBLTYPE = { Value = translation.Name },
                BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                BEGINTIJD =
                {
                    Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)
                },
                BEGINORG = { Value = envelope.Message.OrganizationId },
                LBLBGNORG = { Value = envelope.Message.Organization }
            }.ToBytes(manager, encoding)
        };

        await context.AddAsync(junctionRecord, token);
    }

    private static async Task ModifyJunction(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        GradeSeparatedJunctionModified junction,
        CancellationToken token)
    {
        var junctionRecord = await context.GradeSeparatedJunctions.FindAsync(junction.Id, cancellationToken: token).ConfigureAwait(false);

        var translation = GradeSeparatedJunctionType.Parse(junction.Type).Translation;
        if (junctionRecord != null)
            junctionRecord.DbaseRecord = new GradeSeparatedJunctionDbaseRecord
            {
                OK_OIDN = { Value = junction.Id },
                TYPE = { Value = translation.Identifier },
                LBLTYPE = { Value = translation.Name },
                BO_WS_OIDN = { Value = junction.UpperRoadSegmentId },
                ON_WS_OIDN = { Value = junction.LowerRoadSegmentId },
                BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                BEGINORG = { Value = envelope.Message.OrganizationId },
                LBLBGNORG = { Value = envelope.Message.Organization }
            }.ToBytes(manager, encoding);
    }

    private static async Task RemoveJunction(ProductContext context, GradeSeparatedJunctionRemoved junction, CancellationToken token)
    {
        var junctionRecord = await context.GradeSeparatedJunctions.FindAsync(junction.Id, cancellationToken: token).ConfigureAwait(false);

        if (junctionRecord is not null)
        {
            context.GradeSeparatedJunctions.Remove(junctionRecord);
        }
    }
}
