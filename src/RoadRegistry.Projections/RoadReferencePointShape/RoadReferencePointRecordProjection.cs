namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using Shaperon;

    public class RoadReferencePointRecordProjection: ConnectedProjection<ShapeContext>
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly ReferencePointTypeTranslator _referencePointTypeTranslator;
        private readonly Encoding _encoding;

        public RoadReferencePointRecordProjection(
            WellKnownBinaryReader wkbReader,
            ReferencePointTypeTranslator referencePointTypeTranslator,
            Encoding encoding)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _referencePointTypeTranslator = referencePointTypeTranslator ?? throw new ArgumentNullException(nameof(referencePointTypeTranslator));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedReferencePoint>>((context, message, token) => HandleImportedRoadReferencePoint(context, message.Message, token));
        }

        private Task HandleImportedRoadReferencePoint(
            ShapeContext context,
            ImportedReferencePoint @event,
            CancellationToken token)
        {
            var pointShapeContent = new PointShapeContent(_wkbReader.ReadAs<PointM>(@event.Geometry));

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
                    }.ToBytes(_encoding)
                },
                token);
        }
    }
}
