namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Text;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using Microsoft.IO;
    using Schema;
    using Schema.RoadNodes;

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

                var point = GeometryTranslator.FromGeometryPoint(BackOffice.Core.GeometryTranslator.Translate(envelope.Message.Geometry));
                var pointShapeContent = new PointShapeContent(point);

                await context.RoadNodes.AddAsync(new RoadNodeRecord
                {
                    Id = envelope.Message.Id,
                    ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    DbaseRecord = dbaseRecord.ToBytes(manager, encoding),
                    BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
                }, token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var message in envelope.Message.Changes.Flatten())
                {
                    switch (message)
                    {
                        case RoadNodeAdded node:
                            var typeTranslation = RoadNodeType.Parse(node.Type).Translation;
                            var dbaseRecord = new RoadNodeDbaseRecord
                            {
                                WK_OIDN = {Value = node.Id},
                                WK_UIDN = {Value = node.Id + "_0" }, // 1?
                                TYPE = {Value = typeTranslation.Identifier},
                                LBLTYPE = {Value = typeTranslation.Name},
                                BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                BEGINORG = {Value = envelope.Message.OrganizationId},
                                LBLBGNORG = {Value = envelope.Message.Organization}
                            };

                            var point = GeometryTranslator.FromGeometryPoint(BackOffice.Core.GeometryTranslator.Translate(node.Geometry));
                            var pointShapeContent = new PointShapeContent(point);

                            await context.RoadNodes.AddAsync(new RoadNodeRecord
                            {
                                Id = node.Id,
                                ShapeRecordContent = pointShapeContent.ToBytes(manager, encoding),
                                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                                DbaseRecord = dbaseRecord.ToBytes(manager, encoding),
                                BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
                            }, token);
                            break;
                    }
                }

            });
        }
    }
}
