namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice.Projections;
    using BackOffice.Schema.RoadSegmentLaneAttributes;
    using Messages;
    using Model;
    using Xunit;

    public class RoadSegmentLaneAttributeProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentLaneAttributeProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
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
                    segment.Lanes = _fixture
                        .CreateMany<ImportedRoadSegmentLaneAttributes>(random.Next(1, 10))
                        .ToArray();

                    var expected = segment
                        .Lanes
                        .Select(lane => new RoadSegmentLaneAttributeRecord
                        {
                            Id = lane.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                            {
                                RS_OIDN = { Value = lane.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                                AANTAL =  { Value = lane.Count },
                                RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                                LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                                VANPOS = { Value = (double)lane.FromPosition },
                                TOTPOS = { Value = (double)lane.ToPosition },
                                BEGINTIJD = { Value = lane.Origin.Since },
                                BEGINORG = { Value = lane.Origin.OrganizationId },
                                LBLBGNORG = { Value = lane.Origin.Organization }
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentLaneAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_lanes()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Lanes = new ImportedRoadSegmentLaneAttributes[0];

            return new RoadSegmentLaneAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
