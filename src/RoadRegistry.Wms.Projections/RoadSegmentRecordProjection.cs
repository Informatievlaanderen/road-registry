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
    using Schema.RoadSegments;

    public class RoadSegmentRecordProjection : ConnectedProjection<EditorContext>
    {
        public RoadSegmentRecordProjection(RecyclableMemoryStreamManager manager,
            Encoding encoding)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            When<Envelope<ImportedRoadSegment>>(async (context, envelope, token) =>
            {
                var geometry =
                    GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(envelope.Message.Geometry));
                var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
                var statusTranslation = RoadSegmentStatus.Parse(envelope.Message.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(envelope.Message.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(envelope.Message.Category).Translation;
                var geometryDrawMethodTranslation =
                    RoadSegmentGeometryDrawMethod.Parse(envelope.Message.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation =
                    RoadSegmentAccessRestriction.Parse(envelope.Message.AccessRestriction).Translation;
                await context.RoadSegments.AddAsync(
                    new RoadSegmentRecord
                    {
                        Id = envelope.Message.Id,
                        ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding),
                        ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                        BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                        DbaseRecord = new RoadSegmentDbaseRecord
                        {
                            WS_OIDN = {Value = envelope.Message.Id},
                            WS_UIDN = {Value = $"{envelope.Message.Id}_{envelope.Message.Version}"},
                            WS_GIDN = {Value = $"{envelope.Message.Id}_{envelope.Message.GeometryVersion}"},
                            B_WK_OIDN = {Value = envelope.Message.StartNodeId},
                            E_WK_OIDN = {Value = envelope.Message.EndNodeId},
                            STATUS = {Value = statusTranslation.Identifier},
                            LBLSTATUS = {Value = statusTranslation.Name},
                            MORF = {Value = morphologyTranslation.Identifier},
                            LBLMORF = {Value = morphologyTranslation.Name},
                            WEGCAT = {Value = categoryTranslation.Identifier},
                            LBLWEGCAT = {Value = categoryTranslation.Name},
                            LSTRNMID = {Value = envelope.Message.LeftSide.StreetNameId},
                            LSTRNM = {Value = envelope.Message.LeftSide.StreetName},
                            RSTRNMID = {Value = envelope.Message.RightSide.StreetNameId},
                            RSTRNM = {Value = envelope.Message.RightSide.StreetName},
                            BEHEER = {Value = envelope.Message.MaintenanceAuthority.Code},
                            LBLBEHEER = {Value = envelope.Message.MaintenanceAuthority.Name},
                            METHODE = {Value = geometryDrawMethodTranslation.Identifier},
                            LBLMETHOD = {Value = geometryDrawMethodTranslation.Name},
                            OPNDATUM = {Value = envelope.Message.RecordingDate},
                            BEGINTIJD = {Value = envelope.Message.Origin.Since},
                            BEGINORG = {Value = envelope.Message.Origin.OrganizationId},
                            LBLBGNORG = {Value = envelope.Message.Origin.Organization},
                            TGBEP = {Value = accessRestrictionTranslation.Identifier},
                            LBLTGBEP = {Value = accessRestrictionTranslation.Name}
                        }.ToBytes(manager, encoding)
                    },
                    token);
            });

            When<Envelope<RoadNetworkChangesBasedOnArchiveAccepted>>(async (context, envelope, token) =>
            {
                foreach (var message in envelope.Message.Changes.Flatten())
                {
                    switch (message)
                    {
                        case RoadSegmentAdded segment:
                            var geometry =
                                GeometryTranslator.FromGeometryMultiLineString(BackOffice.Core.GeometryTranslator.Translate(segment.Geometry));
                            var polyLineMShapeContent = new PolyLineMShapeContent(geometry);
                            var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
                            var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
                            var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
                            var geometryDrawMethodTranslation =
                                RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation;
                            var accessRestrictionTranslation =
                                RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;
                            await context.RoadSegments.AddAsync(
                                new RoadSegmentRecord
                                {
                                    Id = segment.Id,
                                    ShapeRecordContent = polyLineMShapeContent.ToBytes(manager, encoding),
                                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                                    BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                                    DbaseRecord = new RoadSegmentDbaseRecord
                                    {
                                        WS_OIDN = {Value = segment.Id},
                                        WS_UIDN = {Value = $"{segment.Id}_{segment.Version}"},
                                        WS_GIDN = {Value = $"{segment.Id}_{segment.GeometryVersion}"},
                                        B_WK_OIDN = {Value = segment.StartNodeId},
                                        E_WK_OIDN = {Value = segment.EndNodeId},
                                        STATUS = {Value = statusTranslation.Identifier},
                                        LBLSTATUS = {Value = statusTranslation.Name},
                                        MORF = {Value = morphologyTranslation.Identifier},
                                        LBLMORF = {Value = morphologyTranslation.Name},
                                        WEGCAT = {Value = categoryTranslation.Identifier},
                                        LBLWEGCAT = {Value = categoryTranslation.Name},
                                        LSTRNMID = {Value = segment.LeftSide.StreetNameId},
                                        // TODO: Where does this come from?
                                        LSTRNM = {Value = null},
                                        RSTRNMID = {Value = segment.RightSide.StreetNameId},
                                        // TODO: Where does this come from?
                                        RSTRNM = {Value = null},
                                        BEHEER = {Value = segment.MaintenanceAuthority.Code},
                                        LBLBEHEER = {Value = segment.MaintenanceAuthority.Name},
                                        METHODE = {Value = geometryDrawMethodTranslation.Identifier},
                                        LBLMETHOD = {Value = geometryDrawMethodTranslation.Name},
                                        OPNDATUM = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                        BEGINTIJD = {Value = LocalDateTimeTranslator.TranslateFromWhen(envelope.Message.When) },
                                        BEGINORG = {Value = envelope.Message.OrganizationId},
                                        LBLBGNORG = {Value = envelope.Message.Organization},
                                        TGBEP = {Value = accessRestrictionTranslation.Identifier},
                                        LBLTGBEP = {Value = accessRestrictionTranslation.Name}
                                    }.ToBytes(manager, encoding)
                                },
                                token);
                            break;
                    }
                }

            });
        }
    }
}
