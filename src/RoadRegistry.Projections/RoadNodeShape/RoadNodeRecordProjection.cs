namespace RoadRegistry.Projections
{
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Events;
    using Shaperon;
    using Wkx;

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
                var pointShapeContent = new PointShapeContent(From.WellKnownBinary(@event.Geometry.WellKnownBinary).To<Point>());

                return context.AddAsync(new RoadNodeRecord
                    {
                    Id = @event.Id,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = dbaseRecord.ToBytes()
                }, token);
            });
        }
    }
}
