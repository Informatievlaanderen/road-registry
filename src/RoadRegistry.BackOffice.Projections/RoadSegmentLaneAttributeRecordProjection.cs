namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Messages;
    using Microsoft.IO;
    using Model;
    using Schema;
    using Schema.RoadSegmentLaneAttributes;

    public class RoadSegmentLaneAttributeRecordProjection : ConnectedProjection<ShapeContext>
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

                return context.AddRangeAsync(laneRecords, token);
            });
        }
    }
}
