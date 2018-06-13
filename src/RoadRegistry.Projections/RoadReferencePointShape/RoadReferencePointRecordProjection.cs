namespace RoadRegistry.Projections
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Events;
    using NetTopologySuite.Geometries;
    using Shaperon;

    public class RoadReferencePointRecordProjection: ConnectedProjection<ShapeContext>
    {
        private readonly ReferencePointTypeTranslator _referencePointTypeTranslator;

        public RoadReferencePointRecordProjection()
        {
            _referencePointTypeTranslator = new ReferencePointTypeTranslator();

            When<ImportedReferencePoint>(HandleImportedRoadReferencePoint);
        }

        private Task HandleImportedRoadReferencePoint(
            ShapeContext context,
            ImportedReferencePoint @event,
            CancellationToken token)
        {
            var pointShapeContent = new PointShapeContent(From.WellKnownBinary(@event.Geometry.WellKnownBinary).To<Point>());

            return context.AddAsync(
                new RoadReferencePointRecord
                {
                    Id = @event.Id,
                    ShapeRecordContent = pointShapeContent.ToBytes(),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = new RoadReferencePointDbaseRecord
                    {
                        RP_OIDN = { Value = @event.Id },
                        RP_UIDN = { Value = $"{@event.Id}_{@event.Version}" },
                        IDENT8 = { Value = @event.Ident8 },
                        OPSCHRIFT = { Value = @event.Caption },
                        TYPE = { Value = _referencePointTypeTranslator.TranslateToIdentifier(@event.Type) },
                        LBLTYPE = { Value = _referencePointTypeTranslator.TranslateToDutchName(@event.Type) },
                        BEGINTIJD = { Value = @event.Origin.Since },
                        BEGINORG = { Value = @event.Origin.OrganizationId },
                        LBLBGNORG = { Value = @event.Origin.Organization },
                    }.ToBytes()
                },
                token);
        }
    }
}
