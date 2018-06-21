namespace RoadRegistry.Projections
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;

    public class RoadSegmentNumberedRoadAttributeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly NumberedRoadSegmentDirectionTranslator _directionTranslator;

        public RoadSegmentNumberedRoadAttributeRecordProjection(NumberedRoadSegmentDirectionTranslator directionTranslator)
        {
            _directionTranslator = directionTranslator;

            When<Envelope<ImportedRoadSegment>>((context, message, token) => HandleImportedRoadSegment(context, message.Message, token));
        }

        private Task HandleImportedRoadSegment(ShapeContext context, ImportedRoadSegment @event, CancellationToken token)
        {
            if(@event.PartOfNumberedRoads.Length == 0)
                return Task.CompletedTask;

            var numberedRoadAttributes = @event
                .PartOfNumberedRoads
                .Select(numberedRoad => new RoadSegmentNumberedRoadAttributeRecord
                {
                    Id = numberedRoad.AttributeId,
                    RoadSegmentId = @event.Id,
                    DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                    {
                        GW_OIDN = { Value = numberedRoad.AttributeId },
                        WS_OIDN = { Value = @event.Id },
                        IDENT8 = { Value = numberedRoad.Ident8 },
                        RICHTING = { Value = _directionTranslator.TranslateToIdentifier(numberedRoad.Direction) },
                        LBLRICHT = { Value = _directionTranslator.TranslateToDutchName(numberedRoad.Direction) },
                        VOLGNUMMER = { Value = numberedRoad.Ordinal },
                        BEGINTIJD = { Value = numberedRoad.Origin.Since },
                        BEGINORG = { Value = numberedRoad.Origin.OrganizationId },
                        LBLBGNORG = { Value = numberedRoad.Origin.Organization },
                    }.ToBytes()
                });
            return context.AddRangeAsync(numberedRoadAttributes, token);
        }
    }
}
