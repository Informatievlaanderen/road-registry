namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadNodes;
    using GeometryTranslator = Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator;

    public class RoadNodeRecordProjection : ConnectedProjection<EditorContext>
    {
        public RoadNodeRecordProjection(RecyclableMemoryStreamManager manager, Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadNode>>(async (context, envelope, token) =>
            {
                var typeTranslation = RoadNodeType.Parse(envelope.Message.Type).Translation;
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

                var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry));
                var pointShapeContent = new PointShapeContent(point);

                await context.RoadNodes.AddAsync(new RoadNodeRecord
                {
                    Id = envelope.Message.Id,
                    ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = dbaseRecord.ToBytes(manager, encoding),
                    Geometry = BackOffice.GeometryTranslator.Translate(envelope.Message.Geometry),
                    BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
                }, token);
            });

            When<Envelope<RoadNetworkChangesAccepted>>(async (context, envelope, token) =>
            {
                foreach (var message in envelope.Message.Changes.Flatten())
                {
                    switch (message)
                    {
                        case RoadNodeAdded node:
                            await AddRoadNode(manager, encoding, context, envelope, node, token);
                            break;

                        case RoadNodeModified node:
                            await ModifyRoadNode(manager, encoding, context, envelope, node);
                            break;

                        case RoadNodeRemoved node:
                            await RemoveRoadNode(context, node);
                            break;
                    }
                }
            });
        }

        private static async Task AddRoadNode(RecyclableMemoryStreamManager manager,
            Encoding encoding,
            EditorContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeAdded node,
            CancellationToken token)
        {
            var typeTranslation = RoadNodeType.Parse(node.Type).Translation;
            var dbaseRecord = new RoadNodeDbaseRecord
            {
                WK_OIDN = {Value = node.Id},
                WK_UIDN = {Value = node.Id + "_0"}, // 1?
                TYPE = {Value = typeTranslation.Identifier},
                LBLTYPE = {Value = typeTranslation.Name},
                BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)},
                BEGINORG = {Value = envelope.Message.OrganizationId},
                LBLBGNORG = {Value = envelope.Message.Organization}
            };

            var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(node.Geometry));
            var pointShapeContent = new PointShapeContent(point);

            await context.RoadNodes.AddAsync(new RoadNodeRecord
            {
                Id = node.Id,
                ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding),
                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                DbaseRecord = dbaseRecord.ToBytes(manager, encoding),
                Geometry = BackOffice.GeometryTranslator.Translate(node.Geometry),
                BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
            }, token);
        }

        private static async Task ModifyRoadNode(RecyclableMemoryStreamManager manager,
            Encoding encoding,
            EditorContext context,
            Envelope<RoadNetworkChangesAccepted> envelope,
            RoadNodeModified node)
        {
            var typeTranslation = RoadNodeType.Parse(node.Type).Translation;
            var dbaseRecord = new RoadNodeDbaseRecord
            {
                WK_OIDN = {Value = node.Id},
                WK_UIDN = {Value = node.Id + "_0"}, // 1?
                TYPE = {Value = typeTranslation.Identifier},
                LBLTYPE = {Value = typeTranslation.Name},
                BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When)},
                BEGINORG = {Value = envelope.Message.OrganizationId},
                LBLBGNORG = {Value = envelope.Message.Organization}
            };

            var point = GeometryTranslator.FromGeometryPoint(BackOffice.GeometryTranslator.Translate(node.Geometry));
            var pointShapeContent = new PointShapeContent(point);

            var roadNode = await context.RoadNodes.FindAsync(node.Id);

            roadNode.ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding);
            roadNode.ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32();
            roadNode.DbaseRecord = dbaseRecord.ToBytes(manager, encoding);
            roadNode.BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape);
            roadNode.Geometry = BackOffice.GeometryTranslator.Translate(node.Geometry);
        }

        private static async Task RemoveRoadNode(EditorContext context, RoadNodeRemoved node)
        {
            var roadNode = await context.RoadNodes.FindAsync(node.Id);

            context.RoadNodes.Remove(roadNode);
        }
    }
}
