namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework.Projections;
    using RoadRegistry.Projections;
    using Schema.RoadSegments;
    using Xunit;

    public class RoadSegmentNationalRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentNationalRoadAttributeRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
            _fixture.CustomizeArchiveId();
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

            _fixture.CustomizeRoadNetworkChangesAccepted();

            _fixture.CustomizeRoadSegmentAddedToNationalRoad();
            _fixture.CustomizeRoadSegmentRemovedFromNationalRoad();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
                .Select(segment =>
                {
                    segment.PartOfNationalRoads = _fixture
                        .CreateMany<ImportedRoadSegmentNationalRoadAttribute>(random.Next(1, 10))
                        .ToArray();

                    var expected = segment
                        .PartOfNationalRoads
                        .Select(nationalRoad => new RoadSegmentNationalRoadAttributeRecord
                        {
                            Id = nationalRoad.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                            {
                                NW_OIDN = { Value = nationalRoad.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                IDENT2 = { Value = nationalRoad.Number },
                                BEGINTIJD = { Value = nationalRoad.Origin.Since },
                                BEGINORG = { Value = nationalRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = nationalRoad.Origin.Organization }
                            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_national_road_links()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.PartOfNationalRoads = new ImportedRoadSegmentNationalRoadAttribute[0];

            return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .ExpectNone();
        }


        [Fact]
        public Task When_adding_road_segments()
        {
            var message = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNationalRoad>());

            var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var nationalRoad = change.RoadSegmentAddedToNationalRoad;

                return (object)new RoadSegmentNationalRoadAttributeRecord
                {
                    Id = nationalRoad.AttributeId,
                    RoadSegmentId = nationalRoad.SegmentId,
                    DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                    {
                        NW_OIDN = { Value = nationalRoad.AttributeId },
                        WS_OIDN = { Value = nationalRoad.SegmentId },
                        IDENT2 = { Value = nationalRoad.Ident2 },
                        BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                        BEGINORG = { Value = message.OrganizationId },
                        LBLBGNORG = { Value = message.Organization }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                };
            });

            return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(message)
                .Expect(expectedRecords);
        }

        [Fact]
        public Task When_removing_road_segments()
        {
            _fixture.Freeze<AttributeId>();

            var acceptedRoadSegmentAdded = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

            var acceptedRoadSegmentRemoved = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());

            return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
                .ExpectNone();
        }
    }
}
