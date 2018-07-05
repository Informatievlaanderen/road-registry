namespace RoadRegistry.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentEuropeanRoadAttributeRecordProjection: ConnectedProjection<ShapeContext>
    {
        private readonly Encoding _encoding;
        public RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.PartOfEuropeanRoads.Length == 0)
                return Task.CompletedTask;

            var europeanRoadAttibutes = @event
                .PartOfEuropeanRoads
                .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                {
                    Id = europeanRoad.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                    {
                        EU_OIDN = { Value = europeanRoad.AttributeId },
                        WS_OIDN = { Value = @event.Id },
                        EUNUMMER = { Value = europeanRoad.RoadNumber },
                        BEGINTIJD = { Value = europeanRoad.Origin.Since },
                        BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
                        LBLBGNORG = { Value = europeanRoad.Origin.Organization },
                    }.ToBytes(_encoding)
                });

            return context.AddRangeAsync(europeanRoadAttibutes, token);
        }
    }
}
