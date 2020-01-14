namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using Framework.Testing.Projections;
    using Messages;
    using Microsoft.IO;
    using Model;
    using Schema.RoadSegmentNumberedRoadAttributes;
    using Xunit;

    public class RoadSegmentNumberedRoadAttributeProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentNumberedRoadAttributeProjectionTests()
        {
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
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
                .Select(segment =>
                {
                    segment.PartOfNumberedRoads = _fixture
                        .CreateMany<ImportedRoadSegmentNumberedRoadAttributes>(random.Next(1, 10))
                        .ToArray();

                    var expected = segment
                        .PartOfNumberedRoads
                        .Select(numberedRoad => new RoadSegmentNumberedRoadAttributeRecord
                        {
                            Id = numberedRoad.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                            {
                                GW_OIDN = { Value = numberedRoad.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                IDENT8 = { Value = numberedRoad.Ident8 },
                                RICHTING = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier },
                                LBLRICHT = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name },
                                VOLGNUMMER = { Value = numberedRoad.Ordinal },
                                BEGINTIJD = { Value = numberedRoad.Origin.Since },
                                BEGINORG = { Value = numberedRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = numberedRoad.Origin.Organization }
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentNumberedRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_numbered_road_links()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.PartOfNumberedRoads = new ImportedRoadSegmentNumberedRoadAttributes[0];

            return new RoadSegmentNumberedRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
