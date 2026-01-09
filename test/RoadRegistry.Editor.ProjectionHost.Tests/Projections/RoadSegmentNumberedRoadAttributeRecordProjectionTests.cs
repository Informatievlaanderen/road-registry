namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Editor.Projections;
using Editor.Schema.Extensions;
using Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentNumberedRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentNumberedRoadAttributeRecordProjectionTests(ProjectionTestServices services)
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

        _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
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
                    IDENT8 = { Value = numberedRoad.Number },
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
    public Task When_importing_a_road_node_without_numbered_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNumberedRoads = Array.Empty<ImportedRoadSegmentNumberedRoadAttribute>();

        return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
    public Task When_removing_road_segments()
    {
        var roadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNumberedRoad>();
        var anotherRoadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNumberedRoad>();

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

        return new RoadSegmentNumberedRoadAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect(new RoadSegmentNumberedRoadAttributeRecord
            {
                Id = roadSegmentAddedToNationalRoad.AttributeId,
                RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                {
                    GW_OIDN = { Value = roadSegmentAddedToNationalRoad.AttributeId },
                    WS_OIDN = { Value = roadSegmentAddedToNationalRoad.SegmentId },
                    IDENT8 = { Value = roadSegmentAddedToNationalRoad.Number },
                    RICHTING = { Value = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNationalRoad.Direction).Translation.Identifier },
                    LBLRICHT = { Value = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNationalRoad.Direction).Translation.Name },
                    VOLGNUMMER = { Value = roadSegmentAddedToNationalRoad.Ordinal },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When) },
                    BEGINORG = { Value = acceptedRoadSegmentsAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentsAdded.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
            });
    }

    [Fact]
    public Task When_removing_road_segments_from_numbered_roads()
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
