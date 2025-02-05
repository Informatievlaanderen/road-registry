namespace RoadRegistry.Tests.BackOffice.Extracts
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Schema.RoadSegments;
    using Microsoft.IO;
    using NodaTime.Text;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
    using RoadRegistry.BackOffice.Messages;
    using System.Text;
    using Editor.Projections;
    using Editor.Schema.Extensions;
    using NodaTime;

    public static class DbExtensions
    {
        public static RoadSegmentRecord ToRoadSegmentRecord(this RoadSegmentAdded segment, OrganizationId changedByOrganization, IClock clock)
        {
            return ToRoadSegmentRecord(segment, changedByOrganization, InstantPattern.ExtendedIso.Format(clock.GetCurrentInstant()));
        }

        public static RoadSegmentRecord ToRoadSegmentRecord(this RoadSegmentAdded segment, OrganizationId changedByOrganization, string when)
        {
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            var status = RoadSegmentStatus.Parse(segment.Status);
            var morphology = RoadSegmentMorphology.Parse(segment.Morphology);
            var category = RoadSegmentCategory.Parse(segment.Category);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction);

            return new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = segment.Version,
                GeometryVersion = segment.GeometryVersion,
                StatusId = status.Translation.Identifier,
                MorphologyId = morphology.Translation.Identifier,
                CategoryId = category.Translation.Identifier,
                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                RightSideStreetNameId = segment.RightSide.StreetNameId,
                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(segment.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethod.Translation.Identifier,
                AccessRestrictionId = accessRestriction.Translation.Identifier,

                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(when),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(when),
                BeginOrganizationId = changedByOrganization,
                BeginOrganizationName = changedByOrganization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = status.Translation.Identifier },
                    LBLSTATUS = { Value = status.Translation.Name },
                    MORF = { Value = morphology.Translation.Identifier },
                    LBLMORF = { Value = morphology.Translation.Name },
                    WEGCAT = { Value = category.Translation.Identifier },
                    LBLWEGCAT = { Value = category.Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = geometryDrawMethod.Translation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethod.Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(when) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(when) },
                    BEGINORG = { Value = changedByOrganization },
                    LBLBGNORG = { Value = changedByOrganization },
                    TGBEP = { Value = accessRestriction.Translation.Identifier },
                    LBLTGBEP = { Value = accessRestriction.Translation.Name }
                }.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        }
    }
}
