namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Wms.Projections;

public class RoadSegmentNationalRoadAttributeRecordProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentNationalRoadAttributeRecordProjectionTests()
    {
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
                NW_OIDN = nationalRoad.AttributeId,
                WS_OIDN = nationalRoad.SegmentId,
                IDENT2 = nationalRoad.Number,
                BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                BEGINORG = message.OrganizationId,
                LBLBGNORG = message.Organization
            };
        });

        return new RoadSegmentNationalRoadAttributeRecordProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_node_without_national_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNationalRoads = Array.Empty<ImportedRoadSegmentNationalRoadAttribute>();

        return new RoadSegmentNationalRoadAttributeRecordProjection()
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
                        NW_OIDN = nationalRoad.AttributeId,
                        WS_OIDN = segment.Id,
                        IDENT2 = nationalRoad.Number,
                        BEGINTIJD = nationalRoad.Origin.Since,
                        BEGINORG = nationalRoad.Origin.OrganizationId,
                        LBLBGNORG = nationalRoad.Origin.Organization
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentNationalRoadAttributeRecordProjection()
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

        return new RoadSegmentNationalRoadAttributeRecordProjection()
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect(new RoadSegmentNationalRoadAttributeRecord
            {
                NW_OIDN = roadSegmentAddedToNationalRoad.AttributeId,
                WS_OIDN = roadSegmentAddedToNationalRoad.SegmentId,
                IDENT2 = roadSegmentAddedToNationalRoad.Number,
                BEGINTIJD = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                BEGINORG = acceptedRoadSegmentsAdded.OrganizationId,
                LBLBGNORG = acceptedRoadSegmentsAdded.Organization
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

        return new RoadSegmentNationalRoadAttributeRecordProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .ExpectNone();
    }
}
