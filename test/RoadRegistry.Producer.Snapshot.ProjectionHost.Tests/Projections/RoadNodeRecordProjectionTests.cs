namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Extensions;
using Moq;
using ProjectionHost.Projections;
using RoadNode;
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

    private static Mock<IKafkaProducer> BuildKafkaProducer()
    {
        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<RoadNodeSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<RoadNodeSnapshot>.Success(It.IsAny<RoadNodeSnapshot>()));
        return kafkaProducer;
    }

    private static ICollection<object> ConvertToRoadNodeRecords(RoadNetworkChangesAccepted message, DateTimeOffset created, Action<RoadNodeRecord> modifier = null)
    {
        return Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;

            var typeTranslation = RoadNodeType.Parse(roadNodeAdded.Type).Translation;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            var record = new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                Version = roadNodeAdded.Version,
                TypeId = typeTranslation.Identifier,
                TypeDutchName = typeTranslation.Name,
                Geometry = point,
                Origin = message.ToOrigin(),
                LastChangedTimestamp = created
            };

            modifier?.Invoke(record);
            return (object)record;
        });
    }

    private void KafkaVerify(Mock<IKafkaProducer> kafkaProducer, IEnumerable<object> expectedRecords, Times? times = null)
    {
        foreach (var expectedRecord in expectedRecords.Cast<RoadNodeRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new RoadNodeSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }
    }

    [Fact]
    public async Task When_adding_road_nodes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadNodeAdded>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToRoadNodeRecords(message, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
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

            return (object)new RoadNodeRecord
            {
                Id = roadNodeModified.Id,
                Version = roadNodeModified.Version,
                TypeId = typeTranslation.Identifier,
                TypeDutchName = typeTranslation.Name,
                Geometry = point,
                Origin = acceptedRoadNodeModified.ToOrigin(),
                LastChangedTimestamp = created
            };
        });

        var kafkaProducer = BuildKafkaProducer();
        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
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

        var expectedRecords = ConvertToRoadNodeRecords(acceptedRoadNodeAdded, created.AddDays(-1), record =>
        {
            record.IsRemoved = true;
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

        var kafkaProducer = BuildKafkaProducer();
        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
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

                var expectedRecord = new RoadNodeRecord
                {
                    Id = @event.Id,
                    Version = @event.Version,
                    TypeId = typeTranslation.Identifier,
                    TypeDutchName = typeTranslation.Name,
                    Geometry = point,
                    Origin = @event.Origin.ToOrigin(),
                    LastChangedTimestamp = created
                };

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(created.UtcDateTime, data.Select(d => d.ExpectedRecord));

        var expectedRecords = data.AsReadOnly().Select(x => x.ExpectedRecord).ToArray();
        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_adding_road_nodes_which_were_previously_removed()
    {
        _fixture.Freeze<RoadNodeId>();

        var created = DateTimeOffset.UtcNow;
        
        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeAdded>());

        var acceptedRoadNodeRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadNodeRemoved>());

        var expectedRecords = ConvertToRoadNodeRecords(acceptedRoadNodeAdded, created);

        var kafkaProducer = BuildKafkaProducer();
        await new RoadNodeRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved, acceptedRoadNodeAdded)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords, Times.Exactly(2));
    }
}
