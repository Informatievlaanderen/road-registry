namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Projections;

using System.Globalization;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Extensions;
using GradeSeparatedJunction;
using Moq;
using ProjectionHost.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Uploads;
using RoadRegistry.Tests.Framework.Projections;

public class GradeSeparatedJunctionRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public GradeSeparatedJunctionRecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        _fixture = new Fixture();

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

    [Fact]
    public async Task When_adding_grade_separated_junctions()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<GradeSeparatedJunctionAdded>());

        var created = DateTimeOffset.UtcNow;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var gradeSeparatedJunctionAdded = change.GradeSeparatedJunctionAdded;
            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation;

            return (object)new GradeSeparatedJunctionRecord(
                gradeSeparatedJunctionAdded.Id,
                gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                typeTranslation.Identifier,
                typeTranslation.Name,
                message.ToOrigin(),
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<GradeSeparatedJunctionSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<GradeSeparatedJunctionSnapshot>.Success(It.IsAny<GradeSeparatedJunctionSnapshot>()));

        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(message)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<GradeSeparatedJunctionRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new GradeSeparatedJunctionSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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

                var expectedRecord = new GradeSeparatedJunctionRecord(
                    @event.Id,
                    @event.LowerRoadSegmentId,
                    @event.UpperRoadSegmentId,
                    typeTranslation.Identifier,
                    typeTranslation.Name,
                    @event.Origin.ToOrigin(),
                    created);

                return new
                {
                    ImportedGradeSeparatedJunction = @event,
                    ExpectedRecord = expectedRecord
                };
            })
            .ToList();

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<GradeSeparatedJunctionSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<GradeSeparatedJunctionSnapshot>.Success(It.IsAny<GradeSeparatedJunctionSnapshot>()));

        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(data.Select(d => d.ImportedGradeSeparatedJunction))
            .Expect(created.UtcDateTime, data.Select(d => d.ExpectedRecord));

        foreach (var expectedRecord in data.AsReadOnly().Select(x => x.ExpectedRecord))
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new GradeSeparatedJunctionSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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

            return (object)new GradeSeparatedJunctionRecord(
                gradeSeparatedJunctionModified.Id,
                gradeSeparatedJunctionModified.LowerRoadSegmentId,
                gradeSeparatedJunctionModified.UpperRoadSegmentId,
                typeTranslation.Identifier,
                typeTranslation.Name,
                acceptedGradeSeparatedJunctionModified.ToOrigin(),
                created);
        });

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<GradeSeparatedJunctionSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<GradeSeparatedJunctionSnapshot>.Success(It.IsAny<GradeSeparatedJunctionSnapshot>()));

        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionModified)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<GradeSeparatedJunctionRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new GradeSeparatedJunctionSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
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

        var expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionAdded.Changes, change =>
        {
            var gradeSeparatedJunctionAdded = change.GradeSeparatedJunctionAdded;
            var typeTranslation = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation;

            return (object)new GradeSeparatedJunctionRecord(
                    gradeSeparatedJunctionAdded.Id,
                    gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    typeTranslation.Identifier,
                    typeTranslation.Name,
                    acceptedGradeSeparatedJunctionAdded.ToOrigin(),
                    created.AddDays(-1))
                { IsRemoved = true };
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

        var kafkaProducer = new Mock<IKafkaProducer>();
        kafkaProducer
            .Setup(x => x.Produce(It.IsAny<string>(), It.IsAny<GradeSeparatedJunctionSnapshot>(), CancellationToken.None))
            .ReturnsAsync(Result<GradeSeparatedJunctionSnapshot>.Success(It.IsAny<GradeSeparatedJunctionSnapshot>()));

        await new GradeSeparatedJunctionRecordProjection(kafkaProducer.Object)
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved)
            .Expect(created.UtcDateTime, expectedRecords);

        foreach (var expectedRecord in expectedRecords.Cast<GradeSeparatedJunctionRecord>())
        {
            kafkaProducer.Verify(
                x => x.Produce(
                    expectedRecord.Id.ToString(CultureInfo.InvariantCulture),
                    It.Is(expectedRecord.ToContract(), new GradeSeparatedJunctionSnapshotEqualityComparer()),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}