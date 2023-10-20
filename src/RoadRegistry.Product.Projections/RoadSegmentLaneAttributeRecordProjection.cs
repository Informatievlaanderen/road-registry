namespace RoadRegistry.Product.Projections;

using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extensions;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using Schema;

public class RoadSegmentLaneAttributeRecordProjection : ConnectedProjection<ProductContext>
{
    public RoadSegmentLaneAttributeRecordProjection(RecyclableMemoryStreamManager manager,
        Encoding encoding)
    {
        if (manager == null) throw new ArgumentNullException(nameof(manager));
        if (encoding == null) throw new ArgumentNullException(nameof(encoding));

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

        When<Envelope<RenameOrganizationAccepted>>(async (context, envelope, token) =>
        {
            await RenameOrganization(manager, encoding, context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
        });

        When<Envelope<ChangeOrganizationAccepted>>(async (context, envelope, token) =>
        {
            if (envelope.Message.NameChanged)
            {
                await RenameOrganization(manager, encoding, context, new OrganizationId(envelope.Message.Code), new OrganizationName(envelope.Message.Name), token);
            }
        });
    }

    private static async Task AddRoadSegment(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
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
        ProductContext context,
        RoadSegmentModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(manager, encoding, context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task ModifyRoadSegmentAttributes(RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
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
        ProductContext context,
        RoadSegmentGeometryModified segment,
        Envelope<RoadNetworkChangesAccepted> envelope,
        CancellationToken token)
    {
        await UpdateLanes(manager, encoding, context, envelope, segment.Id, segment.Lanes, token);
    }

    private static async Task RemoveRoadSegment(ProductContext context, RoadSegmentRemoved segment, CancellationToken token)
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
        ProductContext context,
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
                    .Concat(await context
                        .RoadSegmentLaneAttributes
                        .Where(a => a.RoadSegmentId == roadSegmentId)
                        .ToArrayAsync(token)
                    ));
        }
        else
        {
            await context
                .RoadSegmentLaneAttributes
                .Where(a => a.RoadSegmentId == roadSegmentId)
                .ToArrayAsync(token);
            var currentSet = context
                .RoadSegmentLaneAttributes
                .Local
                .Where(a => a.RoadSegmentId == roadSegmentId)
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

    private async Task RenameOrganization(
        RecyclableMemoryStreamManager manager,
        Encoding encoding,
        ProductContext context,
        OrganizationId organizationId,
        OrganizationName organizationName,
        CancellationToken cancellationToken)
    {
        await context.RoadSegmentLaneAttributes
            .ForEachBatchAsync(10000, dbRecords =>
            {
                foreach (var dbRecord in dbRecords)
                {
                    var dbaseRecord = new RoadSegmentLaneAttributeDbaseRecord().FromBytes(dbRecord.DbaseRecord, manager, encoding);
                    var dataChanged = false;

                    if (dbaseRecord.BEGINORG.Value == organizationId)
                    {
                        dbaseRecord.LBLBGNORG.Value = organizationName;
                        dataChanged = true;
                    }

                    if (dataChanged)
                    {
                        dbRecord.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
                    }
                }

                return Task.CompletedTask;
            }, cancellationToken);
    }
}
