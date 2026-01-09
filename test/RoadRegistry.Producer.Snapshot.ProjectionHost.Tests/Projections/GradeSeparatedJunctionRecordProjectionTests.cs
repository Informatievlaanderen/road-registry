namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Extensions;
using GradeSeparatedJunction;
using Moq;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;
using Shared;

public class GradeSeparatedJunctionRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public GradeSeparatedJunctionRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = FixtureFactory.Create();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeOriginProperties();
        _fixture.CustomizeImportedGradeSeparatedJunction();

        _fixture.CustomizeRoadNetworkChangesAccepted();

        _fixture.CustomizeGradeSeparatedJunctionAdded();
        _fixture.CustomizeGradeSeparatedJunctionModified();
        _fixture.CustomizeGradeSeparatedJunctionRemoved();
    }

    private static Mock<IKafkaProducer> BuildKafkaProducer()
    {
        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<int>(), It.IsAny<GradeSeparatedJunctionSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result.Success(new Offset(0)));
        return kafkaProducer;
    }

    private static ICollection<object> ConvertToGradeSeparatedJunctionRecords(RoadNetworkChangesAccepted message, DateTimeOffset created, Action<GradeSeparatedJunctionRecord> modifier = null)
    {
        return Array.ConvertAll(message.Changes, change =>
            {
                var gradeSeparatedJunctionAdded = change.GradeSeparatedJunctionAdded;
                var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation;

                var record = new GradeSeparatedJunctionRecord
                {
                    Id = gradeSeparatedJunctionAdded.Id,
                    LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    TypeId = typeTranslation.Identifier,
                    TypeDutchName = typeTranslation.Name,
                    Origin = message.ToOrigin(),
                    LastChangedTimestamp = created
                };
                modifier?.Invoke(record);
                return (object)record;
            });
    }

    private void KafkaVerify(Mock<IKafkaProducer> kafkaProducer, IEnumerable<object> expectedRecords, Times? times = null)
    {
        foreach (var expectedRecord in expectedRecords.Cast<GradeSeparatedJunctionRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id,
                    It.Is(expectedRecord.ToContract(), new GradeSeparatedJunctionSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                times ?? Times.Once());
        }
    }

    [Fact]
    public async Task When_adding_grade_separated_junctions()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<GradeSeparatedJunctionAdded>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToGradeSeparatedJunctionRecords(message, created);

        var kafkaProducer = BuildKafkaProducer();
        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_grade_separated_junctions_were_imported()
    {
        var created = DateTimeOffset.UtcNow;

        var data = _fixture
            .CreateMany<ImportedGradeSeparatedJunction>(new Random().Next(1, 100))
            .Select(@event =>
            {
                @event.When = LocalDateTimeTranslator.TranslateToWhen(_fixture.Create<DateTime>());
                var typeTranslation = GradeSeparatedJunctionType.Parse(@event.Type).Translation;

                var expectedRecord = new GradeSeparatedJunctionRecord
                {
                    Id = @event.Id,
                    LowerRoadSegmentId = @event.LowerRoadSegmentId,
                    UpperRoadSegmentId = @event.UpperRoadSegmentId,
                    TypeId = typeTranslation.Identifier,
                    TypeDutchName = typeTranslation.Name,
                    Origin = @event.Origin.ToOrigin(),
                    LastChangedTimestamp = created
                };

                return new
                {
                    ImportedGradeSeparatedJunction = @event,
                    ExpectedRecord = expectedRecord
                };
            })
            .ToList();

        var kafkaProducer = BuildKafkaProducer();
        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(d => d.ImportedGradeSeparatedJunction))
            .Expect(created.UtcDateTime, data.Select(d => d.ExpectedRecord));

        var expectedRecords = data.AsReadOnly().Select(x => x.ExpectedRecord).ToArray();
        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_modifying_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionModified>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionModified.Changes, change =>
        {
            var gradeSeparatedJunctionModified = change.GradeSeparatedJunctionModified;
            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionModified.Type).Translation;

            return (object)new GradeSeparatedJunctionRecord
            {
                Id = gradeSeparatedJunctionModified.Id,
                LowerRoadSegmentId = gradeSeparatedJunctionModified.LowerRoadSegmentId,
                UpperRoadSegmentId = gradeSeparatedJunctionModified.UpperRoadSegmentId,
                TypeId = typeTranslation.Identifier,
                TypeDutchName = typeTranslation.Name,
                Origin = acceptedGradeSeparatedJunctionModified.ToOrigin(),
                LastChangedTimestamp = created
            };
        });

        var kafkaProducer = BuildKafkaProducer();
        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionModified)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_removing_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionRemoved>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = ConvertToGradeSeparatedJunctionRecords(acceptedGradeSeparatedJunctionAdded, created.AddDays(-1), record =>
        {
            record.IsRemoved = true;
        });

        expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionRemoved.Changes, change =>
        {
            var gradeSeparatedJunctionRemoved = change.GradeSeparatedJunctionRemoved;

            var record = expectedRecords.Cast<GradeSeparatedJunctionRecord>().Single(x => x.Id == gradeSeparatedJunctionRemoved.Id);
            record.Origin = acceptedGradeSeparatedJunctionRemoved.ToOrigin();
            record.IsRemoved = true;
            record.LastChangedTimestamp = created;

            return (object)record;
        });

        var kafkaProducer = BuildKafkaProducer();
        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords);
    }

    [Fact]
    public async Task When_adding_grade_separated_junctions_which_were_previously_removed()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var created = DateTimeOffset.UtcNow;

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionRemoved>());

        var expectedRecords = ConvertToGradeSeparatedJunctionRecords(acceptedGradeSeparatedJunctionAdded, created);

        var kafkaProducer = BuildKafkaProducer();
        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved, acceptedGradeSeparatedJunctionAdded)
            .Expect(created.UtcDateTime, expectedRecords);

        KafkaVerify(kafkaProducer, expectedRecords, Times.Exactly(2));
    }
}
