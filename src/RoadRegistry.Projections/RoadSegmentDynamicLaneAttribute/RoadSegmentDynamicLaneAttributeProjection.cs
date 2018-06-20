namespace RoadRegistry.Projections
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentDynamicLaneAttributeProjection : ConnectedProjection<ShapeContext>
    {
        private readonly LaneDirectionTranslator _laneDirectionTranslator;

        public RoadSegmentDynamicLaneAttributeProjection(LaneDirectionTranslator laneDirectionTranslator)
        {
            _laneDirectionTranslator = laneDirectionTranslator;

            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(null == @event?.Lanes || @event.Lanes.Length == 0)
                return Task.CompletedTask;

            var laneRecords = @event
                .Lanes
                .Select(lane => new RoadSegmentDynamicLaneAttributeRecord
                {
                    Id = lane.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentDynamicLaneAttributeDbaseRecord
                    {
                        RS_OIDN = { Value = lane.AttributeId },
                        WS_OIDN = { Value = @event.Id },
                        WS_GIDN = { Value = $"{@event.Id}_{@event.GeometryVersion}" },
                        AANTAL = { Value = lane.Count },
                        RICHTING = { Value = _laneDirectionTranslator.TranslateToIdentifier(lane.Direction) },
                        LBLRICHT = { Value = _laneDirectionTranslator.TranslateToDutchName(lane.Direction) },
                        VANPOS = { Value = (double)lane.FromPosition },
                        TOTPOS = { Value = (double)lane.ToPosition },
                        BEGINTIJD = { Value = lane.Origin.Since },
                        BEGINORG = { Value = lane.Origin.OrganizationId },
                        LBLBGNORG = { Value = lane.Origin.Organization },
                    }.ToBytes()
                });

            return context.AddRangeAsync(laneRecords, token);
        }
    }
}
