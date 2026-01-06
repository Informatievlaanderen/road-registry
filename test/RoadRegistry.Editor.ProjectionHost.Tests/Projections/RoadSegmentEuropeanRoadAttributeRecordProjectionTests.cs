namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using Extracts.Schemas.ExtractV1.RoadSegments;
using Microsoft.IO;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentEuropeanRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentEuropeanRoadAttributeRecordProjectionTests(ProjectionTestServices services)
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

        _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
        _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
    }

    [Fact]
    public Task When_adding_road_nodes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var europeanRoad = change.RoadSegmentAddedToEuropeanRoad;

            return (object)new RoadSegmentEuropeanRoadAttributeRecord
            {
                Id = europeanRoad.AttributeId,
                RoadSegmentId = europeanRoad.SegmentId,
                DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = europeanRoad.AttributeId },
                    WS_OIDN = { Value = europeanRoad.SegmentId },
                    EUNUMMER = { Value = europeanRoad.Number },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            };
        });

        return new RoadSegmentEuropeanRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_node_without_european_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfEuropeanRoads = Array.Empty<ImportedRoadSegmentEuropeanRoadAttribute>();

        return new RoadSegmentEuropeanRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
            .Scenario()
            .Given(importedRoadSegment)
            .Expect();
    }

    [Fact]
    public Task When_importing_road_nodes()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(segment =>
            {
                segment.PartOfEuropeanRoads = _fixture
                    .CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .PartOfEuropeanRoads
                    .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                    {
                        Id = europeanRoad.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                        {
                            EU_OIDN = { Value = europeanRoad.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            EUNUMMER = { Value = europeanRoad.Number },
                            BEGINTIJD = { Value = europeanRoad.Origin.Since },
                            BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
                            LBLBGNORG = { Value = europeanRoad.Origin.Organization }
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentEuropeanRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
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
        var roadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
        var anotherRoadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();

        var acceptedRoadSegmentsAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                roadSegmentAddedToEuropeanRoad,
                anotherRoadSegmentAddedToEuropeanRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(new RoadSegmentRemoved
            {
                Id = anotherRoadSegmentAddedToEuropeanRoad.SegmentId
            });

        return new RoadSegmentEuropeanRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect(new RoadSegmentEuropeanRoadAttributeRecord
            {
                Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                {
                    EU_OIDN = { Value = roadSegmentAddedToEuropeanRoad.AttributeId },
                    WS_OIDN = { Value = roadSegmentAddedToEuropeanRoad.SegmentId },
                    EUNUMMER = { Value = roadSegmentAddedToEuropeanRoad.Number },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When) },
                    BEGINORG = { Value = acceptedRoadSegmentsAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentsAdded.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
    }

    [Fact]
    public Task When_removing_road_segments_from_european_roads()
    {
        _fixture.Freeze<AttributeId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());

        return new RoadSegmentEuropeanRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .ExpectNone();
    }
}
