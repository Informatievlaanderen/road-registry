namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Aiv.Vbr.Shaperon;
    using Xunit;
    using NetTopologySuite.Geometries;
    using System.Text;
    using Messages;
    using RoadSegmentAccessRestriction = Model.RoadSegmentAccessRestriction;
    using RoadSegmentCategory = Model.RoadSegmentCategory;
    using RoadSegmentGeometryDrawMethod = Model.RoadSegmentGeometryDrawMethod;
    using RoadSegmentMorphology = Model.RoadSegmentMorphology;
    using RoadSegmentStatus = Model.RoadSegmentStatus;

    public class RoadSegmentProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadSegmentProjectionTests()
        {
            _fixture = new ScenarioFixture();
        }

        [Fact]
        public Task When_road_segments_are_imported()
        {
            var wkbWriter = new WellKnownBinaryWriter();
            var data = _fixture
                .CreateMany<MultiLineString>(new Random().Next(1, 10))
                .Select(multiLineString =>
                {
                    var polyLineMShapeContent = new PolyLineMShapeContent(multiLineString);
                    var importedRoadSegment = _fixture
                        .Build<ImportedRoadSegment>()
                        .With(segment => segment.Geometry, wkbWriter.Write(multiLineString))
                        .Create();

                    var expected = new RoadSegmentRecord
                    {
                        Id = importedRoadSegment.Id,
                        ShapeRecordContent = polyLineMShapeContent.ToBytes(),
                        ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                        Envelope = BoundingBox2D.From(polyLineMShapeContent.Shape.EnvelopeInternal),
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
                        }.ToBytes(Encoding.UTF8),
                    };
                    return new {importedRoadSegment, expected};
                }).ToList();

            return new RoadSegmentRecordProjection(
                    new WellKnownBinaryReader(),
                    Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data.Select(d => d.expected));
        }
    }
}
