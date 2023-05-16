namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Extensions;
using Moq;
using NationalRoad;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;

public class NationalRoadRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public NationalRoadRecordProjectionTests(ProjectionTestServices services)
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
    public async Task When_adding_road_segments_to_national_roads()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNationalRoad>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var nationalRoadAdded = change.RoadSegmentAddedToNationalRoad;

            return (object)new NationalRoadRecord(
                nationalRoadAdded.AttributeId,
                nationalRoadAdded.SegmentId,
                nationalRoadAdded.Number,
                message.ToOrigin(),
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<NationalRoadSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<NationalRoadSnapshot>.Success(It.IsAny<NationalRoadSnapshot>()));

        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<NationalRoadRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new NationalRoadSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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
            .Select(nationalRoad => new NationalRoadRecord(
                nationalRoad.AttributeId,
                importedRoadSegment.Id,
                nationalRoad.Number,
                nationalRoad.Origin.ToOrigin(),
                created))
            .ToList();

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<NationalRoadSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<NationalRoadSnapshot>.Success(It.IsAny<NationalRoadSnapshot>()));

        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords)
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new NationalRoadSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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

        var expectedRecords = Array.ConvertAll(acceptedNationalRoadAdded.Changes, change =>
        {
            var nationalRoadAdded = change.RoadSegmentAddedToNationalRoad;

            return (object)new NationalRoadRecord(
                    nationalRoadAdded.AttributeId,
                    nationalRoadAdded.SegmentId,
                    nationalRoadAdded.Number,
                    acceptedNationalRoadAdded.ToOrigin(),
                    created.AddDays(-1))
                { IsRemoved = true };
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

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<NationalRoadSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<NationalRoadSnapshot>.Success(It.IsAny<NationalRoadSnapshot>()));

        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedNationalRoadAdded, acceptedRoadSegmentRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<NationalRoadRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new NationalRoadSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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

        var expectedRecords = Array.ConvertAll(acceptedNationalRoadAdded.Changes, change =>
        {
            var nationalRoadAdded = change.RoadSegmentAddedToNationalRoad;

            return (object)new NationalRoadRecord(
                    nationalRoadAdded.AttributeId,
                    nationalRoadAdded.SegmentId,
                    nationalRoadAdded.Number,
                    acceptedNationalRoadAdded.ToOrigin(),
                    created.AddDays(-1))
                { IsRemoved = true };
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

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<NationalRoadSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<NationalRoadSnapshot>.Success(It.IsAny<NationalRoadSnapshot>()));

        await new NationalRoadRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedNationalRoadAdded, acceptedNationalRoadRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<NationalRoadRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new NationalRoadSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}