namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Framework.Projections;
    using RoadRegistry.Projections;
    using Schema.RoadSegments;
    using Xunit;

    public class RoadSegmentRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizePolylineM();
            _fixture.CustomizeEuropeanRoadNumber();
            _fixture.CustomizeNationalRoadNumber();
            _fixture.CustomizeNumberedRoadNumber();
            _fixture.CustomizeRoadSegmentNumberedRoadDirection();
            _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            _fixture.CustomizeRoadSegmentLaneCount();
            _fixture.CustomizeRoadSegmentLaneDirection();
            _fixture.CustomizeRoadSegmentWidth();
            _fixture.CustomizeRoadSegmentSurfaceType();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentAccessRestriction();
            _fixture.CustomizeRoadSegmentGeometryVersion();

            _fixture.CustomizeImportedRoadSegment();
            _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentLaneAttributes();
            _fixture.CustomizeImportedRoadSegmentWidthAttributes();
            _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
            _fixture.CustomizeImportedRoadSegmentSideAttributes();
            _fixture.CustomizeOriginProperties();
        }

        [Fact]
        public Task When_road_segments_are_imported()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
                .Select(importedRoadSegment =>
                {
                    var geometry = BackOffice.Core.GeometryTranslator.Translate(importedRoadSegment.Geometry);
                    var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

                    var expected = new RoadSegmentRecord
                    {
                        Id = importedRoadSegment.Id,
                        ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                        ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                        BoundingBox = RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape),
                        DbaseRecord = new RoadSegmentDbaseRecord
                        {
                            WS_OIDN = { Value = importedRoadSegment.Id },
                            WS_UIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Version },
                            WS_GIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.GeometryVersion },
                            B_WK_OIDN = { Value = importedRoadSegment.StartNodeId },
                            E_WK_OIDN = { Value = importedRoadSegment.EndNodeId },
                            STATUS = { Value = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation.Identifier },
                            LBLSTATUS = { Value = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation.Name },
                            MORF = { Value = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation.Identifier },
                            LBLMORF = { Value = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation.Name },
                            WEGCAT = { Value = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation.Identifier },
                            LBLWEGCAT = { Value = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation.Name },
                            LSTRNMID = { Value = importedRoadSegment.LeftSide.StreetNameId },
                            LSTRNM = { Value = importedRoadSegment.LeftSide.StreetName },
                            RSTRNMID = { Value = importedRoadSegment.RightSide.StreetNameId },
                            RSTRNM = { Value = importedRoadSegment.RightSide.StreetName },
                            BEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Code },
                            LBLBEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Name },
                            METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation.Identifier },
                            LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation.Name },
                            OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                            BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                            BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                            LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                            TGBEP = { Value = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation.Identifier },
                            LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation.Name }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    };
                    return new { importedRoadSegment, expected};
                }).ToList();

            return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data.Select(d => d.expected));
        }
    }
}
