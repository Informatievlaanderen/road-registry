namespace RoadRegistry.Editor.Projections;

using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Schema;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Schema.Extensions;

public class RoadSegmentLaneAttributeRecordProjection : ConnectedProjection<EditorContext>
{
    public RoadSegmentLaneAttributeRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        ArgumentNullException.ThrowIfNull(manager);
        ArgumentNullException.ThrowIfNull(encoding);

        When<Envelope<ImportedRoadSegment>>((context, envelope, token) =>
        {
            if (envelope.Message.Lanes.Length == 0)
                return Task.CompletedTask;

            var laneRecords = envelope.Message
                .Lanes
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = envelope.Message.Id,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = { Value = lane.AttributeId },
                            WS_OIDN = { Value = envelope.Message.Id },
                            WS_GIDN = { Value = $"{envelope.Message.Id}_{lane.AsOfGeometryVersion}" },
                            AANTAL = { Value = lane.Count },
                            RICHTING = { Value = laneDirectionTranslation.Identifier },
                            LBLRICHT = { Value = laneDirectionTranslation.Name },
                            VANPOS = { Value = (double)lane.FromPosition },
                            TOTPOS = { Value = (double)lane.ToPosition },
                            BEGINTIJD = { Value = lane.Origin.Since },
                            BEGINORG = { Value = lane.Origin.OrganizationId },
                            LBLBGNORG = { Value = lane.Origin.Organization }
                        }.ToBytes(manager, encoding)
                    };
                });

            return context.RoadSegmentLaneAttributes.AddRangeAsync(laneRecords, token);
        });

        When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
        {
            foreach (var change in envelope.Message.Changes.Flatten())
                switch (change)
                {
                    case RoadSegmentAdded segment:
                        await AddRoadSegment(manager, encoding, context, segment, envelope, token);
                        break;

                    case RoadSegmentModified segment:
                        await ModifyRoadSegment(manager, encoding, context, segment, envelope, token);
                        break;

                    case RoadSegmentAttributesModified segment:
                        await ModifyRoadSegmentAttributes(manager, encoding, context, segment, envelope, token);
                        break;

                    case RoadSegmentGeometryModified segment:
                        await ModifyRoadSegmentGeometry(manager, encoding, context, segment, envelope, token);
                        break;

                    case RoadSegmentRemoved segment:
                        await RemoveRoadSegment(context, segment, token);
                        break;
                }
        });
    }

    private static async Task AddRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentAdded segment,
        Envelope<RoadNetworkChangesAccepted> envelope, CancellationToken cancellationToken)
    {
        if (segment.Lanes.Length != 0)
        {
            var lanes = segment
                .Lanes
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = { Value = lane.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = $"{segment.Id}_{lane.AsOfGeometryVersion}" },
                            AANTAL = { Value = lane.Count },
                            RICHTING = { Value = laneDirectionTranslation.Identifier },
                            LBLRICHT = { Value = laneDirectionTranslation.Name },
                            VANPOS = { Value = (double)lane.FromPosition },
                            TOTPOS = { Value = (double)lane.ToPosition },
                            BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                            BEGINORG = { Value = envelope.Message.OrganizationId },
                            LBLBGNORG = { Value = envelope.Message.Organization }
                        }.ToBytes(manager, encoding)
                    };
                });

            await context.RoadSegmentLaneAttributes.AddRangeAsync(lanes, cancellationToken);
        }
    }

    private static async Task ModifyRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(manager, encoding, context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task ModifyRoadSegmentAttributes(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentAttributesModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        if (segment.Lanes is not null)
        {
            await UpdateLanes(manager, encoding, context, envelope, segment.Id, segment.Lanes, token);
        }
    }

    private static async Task ModifyRoadSegmentGeometry(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(manager, encoding, context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task RemoveRoadSegment(EditorContext context, RoadSegmentRemoved segment, CancellationToken token)
    {
        context.RoadSegmentLaneAttributes.RemoveRange(
            context
                .RoadSegmentLaneAttributes.Local
                .Where(a => a.RoadSegmentId == segment.Id)
                .Concat(await context
                    .RoadSegmentLaneAttributes
                    .Where(a => a.RoadSegmentId == segment.Id)
                    .ToArrayAsync(token)
                ));
    }

    private static async Task UpdateLanes(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        EditorContext context,
        Envelope<RoadNetworkChangesAccepted> envelope,
        int roadSegmentId,
        RoadSegmentLaneAttributes[] lanes,
        CancellationToken token)
    {
        if (lanes.Length == 0)
        {
            context.RoadSegmentLaneAttributes.RemoveRange(
            context.RoadSegmentLaneAttributes.Local
                    .Where(a => a.RoadSegmentId == roadSegmentId)
                    .Concat(await context.RoadSegmentLaneAttributes
                        .Where(a => a.RoadSegmentId == roadSegmentId)
                        .ToArrayAsync(token)
                    ));
        }
        else
        {
            //Causes all attributes to be loaded into Local
            await context
                .RoadSegmentLaneAttributes
                .Where(a => a.RoadSegmentId == roadSegmentId)
                .ToArrayAsync(token);
            var currentSet = context
                .RoadSegmentLaneAttributes
                .Local.Where(a => a.RoadSegmentId == roadSegmentId)
                .ToDictionary(a => a.Id);
            var nextSet = lanes
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = roadSegmentId,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = { Value = lane.AttributeId },
                            WS_OIDN = { Value = roadSegmentId },
                            WS_GIDN = { Value = $"{roadSegmentId}_{lane.AsOfGeometryVersion}" },
                            AANTAL = { Value = lane.Count },
                            RICHTING = { Value = laneDirectionTranslation.Identifier },
                            LBLRICHT = { Value = laneDirectionTranslation.Name },
                            VANPOS = { Value = (double)lane.FromPosition },
                            TOTPOS = { Value = (double)lane.ToPosition },
                            BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                            BEGINORG = { Value = envelope.Message.OrganizationId },
                            LBLBGNORG = { Value = envelope.Message.Organization }
                        }.ToBytes(manager, encoding)
                    };
                })
                .ToDictionary(a => a.Id);
            context.RoadSegmentLaneAttributes.Synchronize(currentSet, nextSet,
                (current, next) => { current.DbaseRecord = next.DbaseRecord; });
        }
    }
}
