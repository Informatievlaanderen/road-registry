namespace RoadRegistry.Editor.Projections;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Schema;
using Schema.Extensions;

public class RoadSegmentNumberedRoadAttributeRecordProjection : ConnectedProjection<EditorContext>
{
    public RoadSegmentNumberedRoadAttributeRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.PartOfNumberedRoads.Length == 0)
                return Task.CompletedTask;

            var numberedRoadAttributes = envelope.Message
                .PartOfNumberedRoads
                .Select(numberedRoad =>
                {
                    var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
                    return new RoadSegmentNumberedRoadAttributeRecord
                    {
                        Id = numberedRoad.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                        {
                            GW_OIDN = { Value = numberedRoad.AttributeId },
                            WS_OIDN = { Value = envelope.Message.Id },
                            IDENT8 = { Value = numberedRoad.Number },
                            RICHTING = { Value = directionTranslation.Identifier },
                            LBLRICHT = { Value = directionTranslation.Name },
                            VOLGNUMMER = { Value = numberedRoad.Ordinal },
                            BEGINTIJD = { Value = numberedRoad.Origin.Since },
                            BEGINORG = { Value = numberedRoad.Origin.OrganizationId },
                            LBLBGNORG = { Value = numberedRoad.Origin.Organization }
                        }.ToBytes(manager, encoding)
                    };
                });
            return context.RoadSegmentNumberedRoadAttributes.AddRangeAsync(numberedRoadAttributes, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAddedToNumberedRoad numberedRoad:
                        await RoadSegmentAdded(manager, encoding, context, envelope, numberedRoad);
                        break;
                    case RoadSegmentOnNumberedRoadModified numberedRoad:
                        await RoadSegmentModified(manager, encoding, context, envelope, numberedRoad, token);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad numberedRoad:
                        await RoadSegmentRemoved(context, numberedRoad, token);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        await RoadSegmentRemoved(context, roadSegmentRemoved, token);
                        break;
                }
        });
    }

    private static async Task RoadSegmentAdded(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentAddedToNumberedRoad numberedRoad)
    {
        var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;
        await context.RoadSegmentNumberedRoadAttributes.AddAsync(new RoadSegmentNumberedRoadAttributeRecord
        {
            Id = numberedRoad.AttributeId,
            RoadSegmentId = numberedRoad.SegmentId,
            DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
            {
                GW_OIDN = { Value = numberedRoad.AttributeId },
                WS_OIDN = { Value = numberedRoad.SegmentId },
                IDENT8 = { Value = numberedRoad.Number },
                RICHTING = { Value = directionTranslation.Identifier },
                LBLRICHT = { Value = directionTranslation.Name },
                VOLGNUMMER = { Value = numberedRoad.Ordinal },
                BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                BEGINORG = { Value = envelope.Message.OrganizationId },
                LBLBGNORG = { Value = envelope.Message.Organization }
            }.ToBytes(manager, encoding)
        });
    }

    private static async Task RoadSegmentModified(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        RoadSegmentOnNumberedRoadModified numberedRoad,
        CancellationToken token)
    {
        var directionTranslation = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation;

        var roadSegment = await context.RoadSegmentNumberedRoadAttributes
            .FindAsync(numberedRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);

        roadSegment.Id = numberedRoad.AttributeId;
        roadSegment.RoadSegmentId = numberedRoad.SegmentId;
        roadSegment.DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
        {
            GW_OIDN = { Value = numberedRoad.AttributeId },
            WS_OIDN = { Value = numberedRoad.SegmentId },
            IDENT8 = { Value = numberedRoad.Number },
            RICHTING = { Value = directionTranslation.Identifier },
            LBLRICHT = { Value = directionTranslation.Name },
            VOLGNUMMER = { Value = numberedRoad.Ordinal },
            BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
            BEGINORG = { Value = envelope.Message.OrganizationId },
            LBLBGNORG = { Value = envelope.Message.Organization }
        }.ToBytes(manager, encoding);
    }

    private static async Task RoadSegmentRemoved(EditorContext context,
        RoadSegmentRemovedFromNumberedRoad numberedRoad,
        CancellationToken token)
    {
        var roadSegment = await context.RoadSegmentNumberedRoadAttributes
            .FindAsync(numberedRoad.AttributeId, cancellationToken: token)
            .ConfigureAwait(false);
        if (roadSegment is not null)
        {
            context.RoadSegmentNumberedRoadAttributes.Remove(roadSegment);
        }
    }

    private static async Task RoadSegmentRemoved(EditorContext context, RoadSegmentRemoved roadSegmentRemoved, CancellationToken token)
    {
        var segmentNumberedRoadAttributeRecords =
            context.RoadSegmentNumberedRoadAttributes.Local
                .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id)
                .Concat(await context.RoadSegmentNumberedRoadAttributes
                    .Where(x => x.RoadSegmentId == roadSegmentRemoved.Id)
                    .ToArrayAsync(token));

        context.RoadSegmentNumberedRoadAttributes.RemoveRange(segmentNumberedRoadAttributeRecords);
    }
}
