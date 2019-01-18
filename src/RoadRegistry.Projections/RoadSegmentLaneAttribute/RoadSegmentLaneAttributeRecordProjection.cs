namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using BackOfficeSchema;
    using BackOfficeSchema.RoadSegmentLaneAttributes;
    using Messages;
    using Model;

    public class RoadSegmentLaneAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;

        public RoadSegmentLaneAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.Lanes.Length == 0)
                return Task.CompletedTask;

            var laneRecords = @event
                .Lanes
                .Select(lane =>
                {
                    var laneDirectionTranslation = RoadSegmentLaneDirection.Parse(lane.Direction).Translation;
                    return new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = @event.Id,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = {Value = lane.AttributeId},
                            WS_OIDN = {Value = @event.Id},
                            WS_GIDN = {Value = $"{@event.Id}_{lane.AsOfGeometryVersion}"},
                            AANTAL = {Value = lane.Count},
                            RICHTING = {Value = laneDirectionTranslation.Identifier},
                            LBLRICHT = {Value = laneDirectionTranslation.Name},
                            VANPOS = {Value = (double) lane.FromPosition},
                            TOTPOS = {Value = (double) lane.ToPosition},
                            BEGINTIJD = {Value = lane.Origin.Since},
                            BEGINORG = {Value = lane.Origin.OrganizationId},
                            LBLBGNORG = {Value = lane.Origin.Organization},
                        }.ToBytes(_encoding)
                    };
                });

            return context.AddRangeAsync(laneRecords, token);
        }
    }
}
