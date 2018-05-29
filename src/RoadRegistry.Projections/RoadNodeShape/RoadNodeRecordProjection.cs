using Aiv.Vbr.ProjectionHandling.Connector;
using RoadRegistry.Events;
using Shaperon;

namespace RoadRegistry.Projections
{

    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private static readonly RoadNodeTypeTranslator RoadNodeTypeTranslator = new RoadNodeTypeTranslator();

        public RoadNodeRecordProjection()
        {
            When<ImportedRoadNode>((context, @event, token) =>
            {
                //TODO:
                //- Use pooled memory streams

                RoadNodeDbaseRecord dbaseRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = @event.Id },
                    WK_UIDN = { Value = @event.Id + "_" + @event.Version },
                    TYPE = { Value = RoadNodeTypeTranslator.TranslateToIdentifier(@event.Type) },
                    LBLTYPE = { Value = RoadNodeTypeTranslator.TranslateToDutchName(@event.Type) },
                    BEGINTIJD = { Value = @event.Origin.Since },
                    BEGINORG = { Value = @event.Origin.OrganizationId },
                    LBLBGNORG = { Value = @event.Origin.Organization }
                };
                return context.AddAsync(new RoadNodeRecord
                    {
                    Id = @event.Id,
                    ShapeRecordContent = new PointShapeContent(FromWellKnownBinary.ToPoint(@event.Geometry.WellKnownBinary)).ToBytes(),
                    DbaseRecord = dbaseRecord.ToBytes()
                }, token);
            });
        }
    }
}
