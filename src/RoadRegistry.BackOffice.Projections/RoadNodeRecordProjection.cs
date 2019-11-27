namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Messages;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadNodes;

    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNodeRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadNode>>((context, envelope, token) =>
            {
                var typeTranslation = Model.RoadNodeType.Parse(envelope.Message.Type).Translation;
                var dbaseRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = {Value = envelope.Message.Id},
                    WK_UIDN = {Value = envelope.Message.Id + "_" + envelope.Message.Version},
                    TYPE = {Value = typeTranslation.Identifier},
                    LBLTYPE = {Value = typeTranslation.Name},
                    BEGINTIJD = {Value = envelope.Message.Origin.Since},
                    BEGINORG = {Value = envelope.Message.Origin.OrganizationId},
                    LBLBGNORG = {Value = envelope.Message.Origin.Organization}
                };

                var point = GeometryTranslator.FromGeometryPoint(Model.GeometryTranslator.Translate(envelope.Message.Geometry));
                var pointShapeContent = new PointShapeContent(point);

                return context.AddAsync(new RoadNodeRecord
                {
                    Id = envelope.Message.Id,
                    ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = dbaseRecord.ToBytes(manager, encoding),
                    BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
                }, token);
            });
        }
    }
}
