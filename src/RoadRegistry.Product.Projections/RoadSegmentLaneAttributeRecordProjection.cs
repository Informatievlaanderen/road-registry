namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadSegments;

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
                                RS_OIDN = {Value = lane.AttributeId},
                                WS_OIDN = {Value = envelope.Message.Id},
                                WS_GIDN = {Value = $"{envelope.Message.Id}_{lane.AsOfGeometryVersion}"},
                                AANTAL = {Value = lane.Count},
                                RICHTING = {Value = laneDirectionTranslation.Identifier},
                                LBLRICHT = {Value = laneDirectionTranslation.Name},
                                VANPOS = {Value = (double) lane.FromPosition},
                                TOTPOS = {Value = (double) lane.ToPosition},
                                BEGINTIJD = {Value = lane.Origin.Since},
                                BEGINORG = {Value = lane.Origin.OrganizationId},
                                LBLBGNORG = {Value = lane.Origin.Organization},
                            }.ToBytes(manager, encoding)
                        };
                    });

                return context.RoadSegmentLaneAttributes.AddRangeAsync(laneRecords, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var change in envelope.Message.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadSegmentAdded segment:
                            if (segment.Lanes.Length != 0)
                            {
                                var lanes = segment
                                    .Lanes
                                    .Select(lane =>
                                    {
                                        var laneDirectionTranslation =
                                            RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                                        return new RoadSegmentLaneAttributeRecord
                                        {
                                            Id = lane.AttributeId,
                                            RoadSegmentId = segment.Id,
                                            DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                                            {
                                                RS_OIDN = {Value = lane.AttributeId},
                                                WS_OIDN = {Value = segment.Id},
                                                WS_GIDN = {Value = $"{segment.Id}_{lane.AsOfGeometryVersion}"},
                                                AANTAL = {Value = lane.Count},
                                                RICHTING = {Value = laneDirectionTranslation.Identifier},
                                                LBLRICHT = {Value = laneDirectionTranslation.Name},
                                                VANPOS = {Value = (double) lane.FromPosition},
                                                TOTPOS = {Value = (double) lane.ToPosition},
                                                BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                                BEGINORG = {Value = envelope.Message.OrganizationId},
                                                LBLBGNORG = {Value = envelope.Message.Organization}
                                            }.ToBytes(manager, encoding)
                                        };
                                    });

                                await context.RoadSegmentLaneAttributes.AddRangeAsync(lanes);
                            }
//                        case RoadSegmentModified segment:
//                            if (segment.Lanes.Length == 0)
//                            {
//                                context.RoadSegmentLaneAttributes.RemoveRange(
//                                    context
//                                        .RoadSegmentLaneAttributes
//                                        .Local.Where(a => a.RoadSegmentId == segment.Id)
//                                        .Concat(await context
//                                            .RoadSegmentLaneAttributes
//                                            .Where(a => a.RoadSegmentId == segment.Id)
//                                            .ToArrayAsync(token)
//                                        ));
//                            }
//                            else
//                            {
//                                var currentSet = context
//                                    .RoadSegmentLaneAttributes
//                                    .Local.Where(a => a.RoadSegmentId == segment.Id)
//                                    .Concat(await context
//                                        .RoadSegmentLaneAttributes
//                                        .Where(a => a.RoadSegmentId == segment.Id)
//                                        .ToArrayAsync(token)
//                                    ).ToDictionary(a => a.Id);
//                                var nextSet = segment
//                                    .Lanes
//                                    .Select(lane =>
//                                    {
//                                        var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
//                                        return new RoadSegmentLaneAttributeRecord
//                                        {
//                                            Id = lane.AttributeId,
//                                            RoadSegmentId = segment.Id,
//                                            DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
//                                            {
//                                                RS_OIDN = {Value = lane.AttributeId},
//                                                WS_OIDN = {Value = segment.Id},
//                                                WS_GIDN = {Value = $"{segment.Id}_{lane.AsOfGeometryVersion}"},
//                                                AANTAL = {Value = lane.Count},
//                                                RICHTING = {Value = laneDirectionTranslation.Identifier},
//                                                LBLRICHT = {Value = laneDirectionTranslation.Name},
//                                                VANPOS = {Value = (double) lane.FromPosition},
//                                                TOTPOS = {Value = (double) lane.ToPosition},
//                                                // TODO: This should come from the event
//                                                BEGINTIJD = {Value = null},
//                                                BEGINORG = {Value = null},
//                                                LBLBGNORG = {Value = null}
//                                            }.ToBytes(manager, encoding)
//                                        };
//                                    })
//                                    .ToDictionary(a => a.Id);
//                                context.RoadSegmentLaneAttributes.Synchronize(currentSet, nextSet, (current, next) =>
//                                    {
//                                        current.DbaseRecord = next.DbaseRecord;
//                                    });
//                            }
                            break;
                    }
                }
            });
        }
    }
}
