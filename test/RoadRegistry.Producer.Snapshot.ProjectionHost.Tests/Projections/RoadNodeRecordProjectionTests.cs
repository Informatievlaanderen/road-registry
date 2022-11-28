namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Moq;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;

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
    public Task When_adding_road_nodes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadNodeAdded>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Type,
                point,
                LocalDateTimeTranslator.TranslateFromWhen(message.When),
                message.Organization,
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        return new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes()
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
            var roadNodeAdded = change.RoadNodeModified;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord(
                roadNodeAdded.Id,
                roadNodeAdded.Type,
                point,
                LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeModified.When),
                acceptedRoadNodeModified.Organization,
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));

        return new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeModified)
            .Expect(created.UtcDateTime, expectedRecords);
    }

    [Fact]
    public Task When_removing_road_nodes()
    {
        _fixture.Freeze<RoadNodeId>();

        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeAdded>());

        var acceptedRoadNodeRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeRemoved>());

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<string>(), CancellationToken.None))
            .ReturnsAsync(Result.Success());

        return new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .ExpectNone();
    }

    [Fact]
    public Task When_road_nodes_were_imported()
    {
        var created = DateTimeOffset.UtcNow;

        var data = _fixture
            .CreateMany<ImportedRoadNode>(new Random().Next(1, 100))
            .Select(@event =>
            {
                @event.When = _fixture.Create<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.ffffffZ");
                var point = GeometryTranslator.Translate(@event.Geometry);
                
                var expectedRecord = new RoadNodeRecord(
                    @event.Id,
                    @event.Type,
                    point,
                    @event.Origin.Since,
                    @event.Origin.Organization,
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

        return new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(created.UtcDateTime, data.Select(d => d.ExpectedRecord));
    }
}
