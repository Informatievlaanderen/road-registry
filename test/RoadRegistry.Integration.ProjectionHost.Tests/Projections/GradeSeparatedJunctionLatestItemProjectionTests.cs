namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using Integration.Projections;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema.GradeSeparatedJunctions;
using LocalDateTimeTranslator = Editor.Projections.LocalDateTimeTranslator;

public class GradeSeparatedJunctionLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public GradeSeparatedJunctionLatestItemProjectionTests()
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

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionAdded;
            return (object)new GradeSeparatedJunctionLatestItem
            {
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

        return new GradeSeparatedJunctionLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_grade_separated_junctions()
    {
        var data = _fixture
            .CreateMany<ImportedGradeSeparatedJunction>()
            .Select(junction =>
            {
                var expected = new GradeSeparatedJunctionLatestItem
                {
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

        return new GradeSeparatedJunctionLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.junction))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_modifying_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionModified>());

        var expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionModified.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionModified;
            return (object)new GradeSeparatedJunctionLatestItem
            {
                Id = junction.Id,
                UpperRoadSegmentId = junction.UpperRoadSegmentId,
                LowerRoadSegmentId = junction.LowerRoadSegmentId,
                TypeId = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier,
                TypeLabel = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionModified.When).ToBelgianInstant(),
                OrganizationId = acceptedGradeSeparatedJunctionModified.OrganizationId,
                OrganizationName = acceptedGradeSeparatedJunctionModified.Organization
            };
        });

        return new GradeSeparatedJunctionLatestItemProjection()
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_grade_separated_junctions()
    {
        _fixture.Freeze<GradeSeparatedJunctionId>();

        var acceptedGradeSeparatedJunctionAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionAdded>());

        var acceptedGradeSeparatedJunctionRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<GradeSeparatedJunctionRemoved>());
        
        var expectedRecords = Array.ConvertAll(acceptedGradeSeparatedJunctionAdded.Changes, change =>
        {
            var junction = change.GradeSeparatedJunctionAdded;

            return (object)new GradeSeparatedJunctionLatestItem
            {
                Id = junction.Id,
                UpperRoadSegmentId = junction.UpperRoadSegmentId,
                LowerRoadSegmentId = junction.LowerRoadSegmentId,
                TypeId = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Identifier,
                TypeLabel = GradeSeparatedJunctionType.Parse(junction.Type).Translation.Name,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionAdded.When).ToBelgianInstant(),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedGradeSeparatedJunctionRemoved.When).ToBelgianInstant(),
                OrganizationId = acceptedGradeSeparatedJunctionRemoved.OrganizationId,
                OrganizationName = acceptedGradeSeparatedJunctionRemoved.Organization,
                IsRemoved = true
            };
        });

        return new GradeSeparatedJunctionLatestItemProjection()
            .Scenario()
            .Given(acceptedGradeSeparatedJunctionAdded, acceptedGradeSeparatedJunctionRemoved)
            .Expect(expectedRecords);
    }
}
