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

            return new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(when) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(when) },
                    BEGINORG = { Value = changedByOrganization },
                    LBLBGNORG = { Value = changedByOrganization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(new RecyclableMemoryStreamManager(), Encoding.UTF8),
                LastEventHash = segment.GetHash()
            };
        }
    }
}
