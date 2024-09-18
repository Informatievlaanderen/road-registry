namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections.Version;
using RoadRegistry.Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.GradeSeparatedJunctions.Version;
using LocalDateTimeTranslator = Editor.Projections.LocalDateTimeTranslator;

public class GradeSeparatedJunctionVersionProjectionTests
{
    private const long InitialPosition = IntegrationContextScenarioExtensions.InitialPosition;

    private readonly Fixture _fixture;

    public GradeSeparatedJunctionVersionProjectionTests()
    {
        _fixture = new Fixture();
        _fixture.CustomizeGradeSeparatedJunctionType();
        _fixture.CustomizeGradeSeparatedJunctionId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizeReason();
        _fixture.CustomizeOperatorName();
        _fixture.CustomizeArchiveId();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeRoadNetworkChangesAccepted();
        _fixture.CustomizeImportedGradeSeparatedJunction();
        _fixture.CustomizeGradeSeparatedJunctionAdded();
        _fixture.CustomizeGradeSeparatedJunctionModified();
        _fixture.CustomizeGradeSeparatedJunctionRemoved();
    }

    [Fact]
    public Task When_adding_grade_separated_junctions()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<GradeSeparatedJunctionAdded>());

        var initialPosition = InitialPosition;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionAdded;
            return (object)new GradeSeparatedJunctionVersion
            {
                Position = initialPosition,
                Id = junction.Id,
                UpperRoadSegmentId = junction.UpperRoadSegmentId,
                LowerRoadSegmentId = junction.LowerRoadSegmentId,
                TypeId = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier,
                TypeLabel = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When).ToBelgianInstant(),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When).ToBelgianInstant(),
                OrganizationId = message.OrganizationId,
                OrganizationName = message.Organization
            };
        });

        return new GradeSeparatedJunctionVersionProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_grade_separated_junctions()
    {
        var initialPosition = InitialPosition;

        var data = _fixture
            .CreateMany<ImportedGradeSeparatedJunction>()
            .Select(junction =>
            {
                var expected = new GradeSeparatedJunctionVersion
                {
                    Position = initialPosition++,
                    Id = junction.Id,
                    UpperRoadSegmentId = junction.UpperRoadSegmentId,
                    LowerRoadSegmentId = junction.LowerRoadSegmentId,
                    TypeId = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier,
                    TypeLabel = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name,
                    CreatedOnTimestamp = junction.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = junction.Origin.Since.ToBelgianInstant(),
                    OrganizationId = junction.Origin.OrganizationId,
                    OrganizationName = junction.Origin.Organization
                };

                return new
                {
                    junction,
                    expected
                };
            }).ToList();

        return new GradeSeparatedJunctionVersionProjection()
            .Scenario()
            .Given(data.Select(d => d.junction))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var gradeSeparatedJunctionAdded = _fixture.Create<GradeSeparatedJunctionAdded>();
        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(gradeSeparatedJunctionAdded);

        var gradeSeparatedJunctionModified = _fixture.Create<GradeSeparatedJunctionModified>();
        var acceptedGradeSeparatedJunctionModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(gradeSeparatedJunctionModified);

        var initialPosition = InitialPosition;

        return new GradeSeparatedJunctionVersionProjection()
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionModified)
            .Expect([
                new GradeSeparatedJunctionVersion
                {
                    Position = initialPosition,
                    Id = gradeSeparatedJunctionAdded.Id,
                    UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    TypeId = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Identifier,
                    TypeLabel = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Name,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                    OrganizationId = acceptedGradeSeparatedJunctionAdded.OrganizationId,
                    OrganizationName = acceptedGradeSeparatedJunctionAdded.Organization
                },
                new GradeSeparatedJunctionVersion
                {
                    Position = ++initialPosition,
                    Id = gradeSeparatedJunctionModified.Id,
                    UpperRoadSegmentId = gradeSeparatedJunctionModified.UpperRoadSegmentId,
                    LowerRoadSegmentId = gradeSeparatedJunctionModified.LowerRoadSegmentId,
                    TypeId = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionModified.Type).Translation.Identifier,
                    TypeLabel = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionModified.Type).Translation.Name,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionModified.When).ToBelgianInstant(),
                    OrganizationId = acceptedGradeSeparatedJunctionModified.OrganizationId,
                    OrganizationName = acceptedGradeSeparatedJunctionModified.Organization
                }
            ]);
    }

    [Fact]
    public Task When_removing_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var gradeSeparatedJunctionAdded = _fixture.Create<GradeSeparatedJunctionAdded>();
        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(gradeSeparatedJunctionAdded);

        var gradeSeparatedJunctionRemoved = _fixture.Create<GradeSeparatedJunctionRemoved>();
        var acceptedGradeSeparatedJunctionRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(gradeSeparatedJunctionRemoved);

        var initialPosition = InitialPosition;

        return new GradeSeparatedJunctionVersionProjection()
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved)
            .Expect([
                new GradeSeparatedJunctionVersion
                {
                    Position = initialPosition,
                    Id = gradeSeparatedJunctionAdded.Id,
                    UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    TypeId = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Identifier,
                    TypeLabel = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Name,
                    OrganizationId = acceptedGradeSeparatedJunctionAdded.OrganizationId,
                    OrganizationName = acceptedGradeSeparatedJunctionAdded.Organization,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant()
                },
                new GradeSeparatedJunctionVersion
                {
                    Position = ++initialPosition,
                    Id = gradeSeparatedJunctionAdded.Id,
                    UpperRoadSegmentId = gradeSeparatedJunctionAdded.UpperRoadSegmentId,
                    LowerRoadSegmentId = gradeSeparatedJunctionAdded.LowerRoadSegmentId,
                    TypeId = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Identifier,
                    TypeLabel = GradeSeparatedJunctionType.Parse(gradeSeparatedJunctionAdded.Type).Translation.Name,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionRemoved.When).ToBelgianInstant(),
                    OrganizationId = acceptedGradeSeparatedJunctionRemoved.OrganizationId,
                    OrganizationName = acceptedGradeSeparatedJunctionRemoved.Organization,
                    IsRemoved = true
                }
            ]);
    }
}
