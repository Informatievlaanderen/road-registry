namespace RoadRegistry.Product.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Extracts.Schemas.ExtractV1.RoadSegments;
using Product.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentNationalRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentNationalRoadAttributeRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = FixtureFactory.Create();
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
                    IDENT2 = { Value = nationalRoad.Number },
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
    public Task When_importing_a_road_node_without_national_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>();

        return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(importedRoadSegment)
            .ExpectNone();
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
    public Task When_removing_road_segments()
    {
        var roadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNationalRoad>();
        var anotherRoadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNationalRoad>();

        var acceptedRoadSegmentsAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                roadSegmentAddedToNationalRoad,
                anotherRoadSegmentAddedToNationalRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(new RoadSegmentRemoved
            {
                Id = anotherRoadSegmentAddedToNationalRoad.SegmentId
            });

        return new RoadSegmentNationalRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect(new RoadSegmentNationalRoadAttributeRecord
            {
                Id = roadSegmentAddedToNationalRoad.AttributeId,
                RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                {
                    NW_OIDN = { Value = roadSegmentAddedToNationalRoad.AttributeId },
                    WS_OIDN = { Value = roadSegmentAddedToNationalRoad.SegmentId },
                    IDENT2 = { Value = roadSegmentAddedToNationalRoad.Number },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When) },
                    BEGINORG = { Value = acceptedRoadSegmentsAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentsAdded.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
    }

    [Fact]
    public Task When_removing_road_segments_from_national_roads()
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
