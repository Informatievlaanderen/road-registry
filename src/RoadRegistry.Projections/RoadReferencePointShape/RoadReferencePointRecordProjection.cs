namespace RoadRegistry.Projections
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Events;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Shaperon;

    public class RoadReferencePointRecordProjection: ConnectedProjection<ShapeContext>
    {
        private readonly WKBReader _wkbReader;
        private readonly ReferencePointTypeTranslator _referencePointTypeTranslator;

        public RoadReferencePointRecordProjection(
            WKBReader wkbReader,
            ReferencePointTypeTranslator referencePointTypeTranslator)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _referencePointTypeTranslator = referencePointTypeTranslator ?? throw new ArgumentNullException(nameof(referencePointTypeTranslator));

            When<ImportedReferencePoint>(HandleImportedRoadReferencePoint);
        }

        private Task HandleImportedRoadReferencePoint(
            ShapeContext context,
            ImportedReferencePoint @event,
            CancellationToken token)
        {
            var pointShapeContent = new PointShapeContent(_wkbReader.ReadAs<Point>(@event.Geometry));

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
