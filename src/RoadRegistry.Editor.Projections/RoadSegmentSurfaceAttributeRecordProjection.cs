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

public class RoadSegmentSurfaceAttributeRecordProjection : ConnectedProjection<EditorContext>
{
    public RoadSegmentSurfaceAttributeRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));
        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.Surfaces.Length == 0)
                return Task.CompletedTask;

            var surfaces = envelope.Message
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;
                    return new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = surface.AttributeId },
                            WS_OIDN = { Value = envelope.Message.Id },
                            WS_GIDN = { Value = $"{envelope.Message.Id}_{surface.AsOfGeometryVersion}" },
                            TYPE = { Value = typeTranslation.Identifier },
                            LBLTYPE = { Value = typeTranslation.Name },
                            VANPOS = { Value = (double)surface.FromPosition },
                            TOTPOS = { Value = (double)surface.ToPosition },
                            BEGINTIJD = { Value = surface.Origin.Since },
                            BEGINORG = { Value = surface.Origin.OrganizationId },
                            LBLBGNORG = { Value = surface.Origin.Organization }
                        }.ToBytes(manager, encoding)
                    };
                });

            return context.RoadSegmentSurfaceAttributes.AddRangeAsync(surfaces, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAdded segment:
                        await AddRoadSegment(manager, encoding, segment, envelope, context);
                        break;

                    case RoadSegmentModified segment:
                        await ModifyRoadSegment(manager, encoding, context, segment, envelope, token);

                        break;

                    case RoadSegmentRemoved segment:
                        await RemoveRoadSegment(context, segment, token);

                        break;
                }
        });
    }

    private static async Task AddRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        RoadSegmentAdded segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        EditorContext context)
    {
        if (segment.Surfaces.Length != 0)
        {
            var surfaces = segment
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;
                    return new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = surface.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = $"{segment.Id}_{surface.AsOfGeometryVersion}" },
                            TYPE = { Value = typeTranslation.Identifier },
                            LBLTYPE = { Value = typeTranslation.Name },
                            VANPOS = { Value = (double)surface.FromPosition },
                            TOTPOS = { Value = (double)surface.ToPosition },
                            BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                            BEGINORG = { Value = envelope.Message.OrganizationId },
                            LBLBGNORG = { Value = envelope.Message.Organization }
                        }.ToBytes(manager, encoding)
                    };
                });

            await context.RoadSegmentSurfaceAttributes.AddRangeAsync(surfaces);
        }
    }
    //TODO-rik add handler for RoadSegmentAttributesModified
    private static async Task ModifyRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        if (segment.Lanes.Length == 0)
        {
            context.RoadSegmentSurfaceAttributes.RemoveRange(
                context
                    .RoadSegmentSurfaceAttributes
                    .Local.Where(a => a.RoadSegmentId == segment.Id)
                    .Concat(await context
                        .RoadSegmentSurfaceAttributes
                        .Where(a => a.RoadSegmentId == segment.Id)
                        .ToArrayAsync(token)
                    ));
        }
        else
        {
            //Causes all attributes to be loaded into Local
            await context
                .RoadSegmentSurfaceAttributes
                .Where(a => a.RoadSegmentId == segment.Id)
                .ToArrayAsync(token);
            var currentSet = context
                .RoadSegmentSurfaceAttributes
                .Local.Where(a => a.RoadSegmentId == segment.Id)
                .ToDictionary(a => a.Id);
            var nextSet = segment
                .Surfaces
                .Select(surface =>
                {
                    var typeTranslation = RoadSegmentSurfaceType.Parse(surface.Type).Translation;
                    return new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = surface.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = $"{segment.Id}_{surface.AsOfGeometryVersion}" },
                            TYPE = { Value = typeTranslation.Identifier },
                            LBLTYPE = { Value = typeTranslation.Name },
                            VANPOS = { Value = (double)surface.FromPosition },
                            TOTPOS = { Value = (double)surface.ToPosition },
                            BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                            BEGINORG = { Value = envelope.Message.OrganizationId },
                            LBLBGNORG = { Value = envelope.Message.Organization }
                        }.ToBytes(manager, encoding)
                    };
                })
                .ToDictionary(a => a.Id);
            context.RoadSegmentSurfaceAttributes.Synchronize(currentSet, nextSet,
                (current, next) => { current.DbaseRecord = next.DbaseRecord; });
        }
    }

    private static async Task RemoveRoadSegment(EditorContext context, RoadSegmentRemoved segment,
        CancellationToken token)
    {
        context.RoadSegmentSurfaceAttributes.RemoveRange(
            context
                .RoadSegmentSurfaceAttributes
                .Local.Where(a => a.RoadSegmentId == segment.Id)
                .Concat(await context
                    .RoadSegmentSurfaceAttributes
                    .Where(a => a.RoadSegmentId == segment.Id)
                    .ToArrayAsync(token)
                ));
    }
}
