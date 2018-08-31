namespace RoadRegistry.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using Events;
    using NetTopologySuite.Geometries;
    using Shaperon;

    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        private readonly RoadNodeTypeTranslator _roadNodeTypeTranslator;
        private readonly WellKnownBinaryReader _wkbReader;
        private readonly Encoding _encoding;

        public RoadNodeRecordProjection(
            WellKnownBinaryReader wkbReader,
            RoadNodeTypeTranslator roadNodeTypeTranslator,
            Encoding encoding)
        {
            _wkbReader = wkbReader ?? throw new ArgumentNullException(nameof(wkbReader));
            _roadNodeTypeTranslator = roadNodeTypeTranslator ?? throw new ArgumentNullException(nameof(roadNodeTypeTranslator));
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            When<Envelope<ImportedRoadNode>>((context, message, token) => HandleImportedRoadNode(context, message.Message, token));
        }

        private Task HandleImportedRoadNode(ShapeContext context, ImportedRoadNode @event, CancellationToken token)
        {
            //TODO:
            //- Use pooled memory streams

            var dbaseRecord = new RoadNodeDbaseRecord
            {
                WK_OIDN = {Value = @event.Id},
                WK_UIDN = {Value = @event.Id + "_" + @event.Version},
                TYPE = {Value = _roadNodeTypeTranslator.TranslateToIdentifier(@event.Type)},
                LBLTYPE = {Value = _roadNodeTypeTranslator.TranslateToDutchName(@event.Type)},
                BEGINTIJD = {Value = @event.Origin.Since},
                BEGINORG = {Value = @event.Origin.OrganizationId},
                LBLBGNORG = {Value = @event.Origin.Organization}
            };
            var pointShapeContent = new PointShapeContent(_wkbReader.ReadAs<Point>(@event.Geometry));

            return context.AddAsync(new RoadNodeRecord
            {
                Id = @event.Id,
                ShapeRecordContent = pointShapeContent.ToBytes(),
                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                DbaseRecord = dbaseRecord.ToBytes(_encoding)
            }, token);
        }
    }
}
