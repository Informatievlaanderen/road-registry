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

    public class RoadSegmentNumberedRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentNumberedRoadAttributeRecordProjectionTests(ProjectionTestServices services)
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

            _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
            _fixture.CustomizeRoadSegmentOnNumberedRoadModified();
            _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
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
                        .CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(random.Next(1, 10))
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
                                IDENT8 = { Value = numberedRoad.Number },
                                RICHTING = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier },
                                LBLRICHT = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name },
                                VOLGNUMMER = { Value = numberedRoad.Ordinal },
                                BEGINTIJD = { Value = numberedRoad.Origin.Since },
                                BEGINORG = { Value = numberedRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = numberedRoad.Origin.Organization }
                            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
            importedRoadSegment.PartOfNumberedRoads = new ImportedRoadSegmentNumberedRoadAttribute[0];

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }

                [Fact]
        public Task When_adding_road_segments()
        {
            var message = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNumberedRoad>());

            var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var numberedRoad = change.RoadSegmentAddedToNumberedRoad;

                return (object)new RoadSegmentNumberedRoadAttributeRecord
                {
                    Id = numberedRoad.AttributeId,
                    RoadSegmentId = numberedRoad.SegmentId,
                    DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                    {
                        GW_OIDN = { Value = numberedRoad.AttributeId },
                        WS_OIDN = { Value = numberedRoad.SegmentId },
                        IDENT8 = { Value = numberedRoad.Ident8 },
                        RICHTING = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier },
                        LBLRICHT = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name },
                        VOLGNUMMER = { Value = numberedRoad.Ordinal },
                        BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                        BEGINORG = { Value = message.OrganizationId },
                        LBLBGNORG = { Value = message.Organization }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                };
            });

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(message)
                .Expect(expectedRecords);
        }

        [Fact]
        public Task When_modifying_road_segments()
        {
            _fixture.Freeze<AttributeId>();

            var acceptedRoadSegmentAdded = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

            var acceptedRoadSegmentModified = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentOnNumberedRoadModified>());

            var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
            {
                var numberedRoad = change.RoadSegmentOnNumberedRoadModified;

                return (object)new RoadSegmentNumberedRoadAttributeRecord
                {
                    Id = numberedRoad.AttributeId,
                    RoadSegmentId = numberedRoad.SegmentId,
                    DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                    {
                        GW_OIDN = { Value = numberedRoad.AttributeId },
                        WS_OIDN = { Value = numberedRoad.SegmentId },
                        IDENT8 = { Value = numberedRoad.Ident8 },
                        RICHTING = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier },
                        LBLRICHT = { Value = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name },
                        VOLGNUMMER = { Value = numberedRoad.Ordinal },
                        BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                        BEGINORG = { Value = acceptedRoadSegmentModified.OrganizationId },
                        LBLBGNORG = { Value = acceptedRoadSegmentModified.Organization }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                };
            });

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
                .Expect(expectedRecords);
        }

        [Fact]
        public Task When_removing_road_segments()
        {
            _fixture.Freeze<AttributeId>();

            var acceptedRoadSegmentAdded = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

            var acceptedRoadSegmentRemoved = _fixture
                .Create<RoadNetworkChangesAccepted>()
                .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
                .ExpectNone();
        }
    }
}
