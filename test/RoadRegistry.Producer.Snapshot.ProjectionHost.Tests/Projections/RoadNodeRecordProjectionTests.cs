namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using System.Linq;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Extensions;
using Moq;
using ProjectionHost.Projections;
using RoadNode;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;

public class RoadNodeRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadNodeRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = new Fixture();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizePoint();
        _fixture.CustomizeOriginProperties();
        _fixture.CustomizeImportedRoadNode();

        _fixture.CustomizeRoadNetworkChangesAccepted();

        _fixture.CustomizeRoadNodeAdded();
        _fixture.CustomizeRoadNodeModified();
        _fixture.CustomizeRoadNodeRemoved();
    }

    [Fact]
    public async Task When_adding_road_nodes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadNodeAdded>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Version,
                typeTranslation.Identifier,
                typeTranslation.Name,
                point,
                message.ToOrigin(),
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        await new RoadNodeRecordProjection(kafkaProducer.Object)
             .Scenario()
             .Given(message)
             .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<RoadNodeRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadNodeSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task When_modifying_road_nodes()
    {
        _fixture.Freeze<RoadNodeId>();

        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeAdded>());

        var acceptedRoadNodeModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeModified>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(acceptedRoadNodeModified.Changes, change =>
        {
            var roadNodeModified = change.RoadNodeModified;
            var typeTranslation = RoadNodeType.Parse(roadNodeModified.Type).Translation;
            var point = GeometryTranslator.Translate(roadNodeModified.Geometry);

            return (object)new RoadNodeRecord(
                roadNodeModified.Id,
                roadNodeModified.Version,
                typeTranslation.Identifier,
                typeTranslation.Name,
                point,
                acceptedRoadNodeModified.ToOrigin(),
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeModified)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<RoadNodeRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadNodeSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task When_removing_road_nodes()
    {
        _fixture.Freeze<RoadNodeId>();

        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeAdded>());

        var acceptedRoadNodeRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeRemoved>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(acceptedRoadNodeAdded.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Version,
                typeTranslation.Identifier,
                typeTranslation.Name,
                point,
                acceptedRoadNodeAdded.ToOrigin(),
                created.AddDays(-1))
            { IsRemoved = true };
        });

        expectedRecords = Array.ConvertAll(acceptedRoadNodeRemoved.Changes, change =>
        {
            var roadNodeRemoved = change.RoadNodeRemoved;

            var record = expectedRecords.Cast<RoadNodeRecord>().Single(x => x.Id == roadNodeRemoved.Id);
            record.Origin = acceptedRoadNodeRemoved.ToOrigin();
            record.IsRemoved = true;
            record.LastChangedTimestamp = created;

            return (object)record;
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<RoadNodeRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadNodeSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }

    [Fact]
    public async Task When_road_nodes_were_imported()
    {
        var created = DateTimeOffset.UtcNow;

        var data = _fixture
            .CreateMany<ImportedRoadNode>(new Random().Next(1, 100))
            .Select(@event =>
            {
                @event.When = LocalDateTimeTranslator.TranslateToWhen(_fixture.Create<DateTime>());
                var typeTranslation = RoadNodeType.Parse(@event.Type).Translation;
                var point = GeometryTranslator.Translate(@event.Geometry);

                var expectedRecord = new RoadNodeRecord(
                    @event.Id,
                    @event.Version,
                    typeTranslation.Identifier,
                    typeTranslation.Name,
                    point,
                    @event.Origin.ToOrigin(),
                    created);

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(created.UtcDateTime, data.Select(d => d.ExpectedRecord));

        foreach (var expectedRecord in data.AsReadOnly().Select(x => x.ExpectedRecord))
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadNodeSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
