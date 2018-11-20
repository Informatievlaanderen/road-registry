namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using Model;
    using RoadNodeType = Model.RoadNodeType;

    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly Encoding _encoding;

        public RoadNodeRecordProjection(
            WellKnownBinaryReader wkbReader,
            Encoding encoding)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadNode>>((context, message, token) => HandleImportedRoadNode(context, message.Message, token));
        }

        private Task HandleImportedRoadNode(ShapeContext context, ImportedRoadNode @event, CancellationToken token)
        {
            //TODO:
            //- Use pooled memory streams

            var typeTranslation = RoadNodeType.Parse(@event.Type).Translation;
            var dbaseRecord = new RoadNodeDbaseRecord
            {
                WK_OIDN = {Value = @event.Id},
                WK_UIDN = {Value = @event.Id + "_" + @event.Version},
                TYPE = {Value = typeTranslation.Identifier},
                LBLTYPE = {Value = typeTranslation.Name},
                BEGINTIJD = {Value = @event.Origin.Since},
                BEGINORG = {Value = @event.Origin.OrganizationId},
                LBLBGNORG = {Value = @event.Origin.Organization}
            };

            var point = GeometryTranslator.Translate(@event.Geometry2);
            var pointShapeContent = new PointShapeContent(new PointM(point.X, point.Y)
            {
                SRID = point.SRID
            });

            return context.AddAsync(new RoadNodeRecord
            {
                Id = @event.Id,
                ShapeRecordContent = pointShapeContent.ToBytes(),
                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                DbaseRecord = dbaseRecord.ToBytes(_encoding),
                Envelope = BoundingBox2D.From(pointShapeContent.Shape.EnvelopeInternal)
            }, token);
        }
    }
}
