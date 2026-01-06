namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Extensions;
using Moq;
using NationalRoad;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;
using Shared;

public class NationalRoadRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public NationalRoadRecordProjectionTests(ProjectionTestServices services)
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

    private static Mock<IKafkaProducer> BuildKafkaProducer()
    {
        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<int>(), It.IsAny<NationalRoadSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result.Success(new Offset(0)));
        return kafkaProducer;
    }

    private static ICollection<object> ConvertToNationalRoadRecords(RoadNetworkChangesAccepted message, DateTimeOffset created, Action<NationalRoadRecord> modifier = null)
    {
        return Array.ConvertAll(message.Changes, change =>
        {
            var nationalRoadAdded = change.RoadSegmentAddedToNationalRoad;

            var record = new NationalRoadRecord
            {
                Id = nationalRoadAdded.AttributeId,
                RoadSegmentId = nationalRoadAdded.SegmentId,
                Number = nationalRoadAdded.Number,
                Origin = message.ToOrigin(),
                LastChangedTimestamp = created
            };

            modifier?.Invoke(record);
            return (object)record;
        });
    }

    private void KafkaVerify(Mock<IKafkaProducer> kafkaProducer, IEnumerable<object> expectedRecords, Times? times = null)
    {
        foreach (var expectedRecord in expectedRecords.Cast<NationalRoadRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id,
                    It.Is(expectedRecord.ToContract(), new NationalRoadSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }
    }

    [Fact]
    public async Task When_adding_road_segments_to_national_roads()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNationalRoad>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToNationalRoadRecords(message, created);

        var kafkaProducer = BuildKafkaProducer();
        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_national_roads_were_imported()
    {
        var created = DateTimeOffset.UtcNow;

        var importedRoadSegment = _fixture
            .Create<ImportedRoadSegment>();
        importedRoadSegment.When = LocalDateTimeTranslator.TranslateToWhen(created);
        importedRoadSegment.PartOfNationalRoads = _fixture.CreateMany<ImportedRoadSegmentNationalRoadAttribute>(2).ToArray();
        var expectedRecords = importedRoadSegment.PartOfNationalRoads
            .Select(nationalRoad => new NationalRoadRecord
            {
                Id = nationalRoad.AttributeId,
                RoadSegmentId = importedRoadSegment.Id,
                Number = nationalRoad.Number,
                Origin = nationalRoad.Origin.ToOrigin(),
                LastChangedTimestamp = created
            })
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_removing_road_segments()
    {
        var acceptedNationalRoadAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                acceptedNationalRoadAdded.Changes
                    .Select(change => change.RoadSegmentAddedToNationalRoad.SegmentId)
                    .Distinct()
                    .Select(roadSegmentId =>
                        new RoadSegmentRemoved
                        {
                            Id = roadSegmentId
                        }));

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToNationalRoadRecords(acceptedNationalRoadAdded, created.AddDays(-1), record =>
        {
            record.IsRemoved = true;
        });

        expectedRecords = Array.ConvertAll(acceptedRoadSegmentRemoved.Changes, change =>
        {
            var roadSegmentRemoved = change.RoadSegmentRemoved;

            var record = expectedRecords.Cast<NationalRoadRecord>().Single(x => x.RoadSegmentId == roadSegmentRemoved.Id);
            record.Origin = acceptedRoadSegmentRemoved.ToOrigin();
            record.IsRemoved = true;
            record.LastChangedTimestamp = created;

            return (object)record;
        });

        var kafkaProducer = BuildKafkaProducer();
        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedNationalRoadAdded, acceptedRoadSegmentRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_removing_road_segments_from_national_roads()
    {
        var acceptedNationalRoadAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var acceptedNationalRoadRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                acceptedNationalRoadAdded.Changes.Select(change =>
                    new RoadSegmentRemovedFromNationalRoad
                    {
                        AttributeId = change.RoadSegmentAddedToNationalRoad.AttributeId,
                        SegmentId = change.RoadSegmentAddedToNationalRoad.SegmentId,
                        Number = change.RoadSegmentAddedToNationalRoad.Number
                    }));

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToNationalRoadRecords(acceptedNationalRoadAdded, created.AddDays(-1), record =>
        {
            record.IsRemoved = true;
        });

        expectedRecords = Array.ConvertAll(acceptedNationalRoadRemoved.Changes, change =>
        {
            var nationalRoadRemoved = change.RoadSegmentRemovedFromNationalRoad;

            var record = expectedRecords.Cast<NationalRoadRecord>().Single(x => x.Id == nationalRoadRemoved.AttributeId);
            record.Origin = acceptedNationalRoadRemoved.ToOrigin();
            record.IsRemoved = true;
            record.LastChangedTimestamp = created;

            return (object)record;
        });

        var kafkaProducer = BuildKafkaProducer();
        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedNationalRoadAdded, acceptedNationalRoadRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_adding_national_roads_which_were_previously_removed()
    {
        var created = DateTimeOffset.UtcNow;

        var acceptedNationalRoadAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                acceptedNationalRoadAdded.Changes
                    .Select(change => change.RoadSegmentAddedToNationalRoad.SegmentId)
                    .Distinct()
                    .Select(roadSegmentId =>
                        new RoadSegmentRemoved
                        {
                            Id = roadSegmentId
                        }));

        var expectedRecords = ConvertToNationalRoadRecords(acceptedNationalRoadAdded, created);

        var kafkaProducer = BuildKafkaProducer();
        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedNationalRoadAdded, acceptedRoadSegmentRemoved, acceptedNationalRoadAdded)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords, Times.Exactly(2));
    }
}
