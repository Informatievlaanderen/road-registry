namespace RoadRegistry.Wfs.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Wfs.Projections;

public class RoadNodeRecordProjectionTests
{
    private readonly Fixture _fixture;

    public RoadNodeRecordProjectionTests()
    {
        _fixture = FixtureFactory.Create();

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

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                Type = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                Geometry = point
            };
        });

        return new RoadNodeRecordProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
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

        var expectedRecords = Array.ConvertAll(acceptedRoadNodeModified.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeModified;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);

            return (object)new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeModified.When),
                Type = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                Geometry = point
            };
        });

        return new RoadNodeRecordProjection()
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeModified)
            .Expect(expectedRecords);
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

        return new RoadNodeRecordProjection()
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .ExpectNone();
    }

    [Fact]
    public Task When_road_nodes_were_imported()
    {
        var data = _fixture
            .CreateMany<ImportedRoadNode>(new Random().Next(1, 100))
            .Select(@event =>
            {
                var point = GeometryTranslator.Translate(@event.Geometry);

                var expectedRecord = new RoadNodeRecord
                {
                    Id = @event.Id,
                    BeginTime = @event.Origin.Since,
                    Type = RoadNodeType.Parse(@event.Type).Translation.Name,
                    Geometry = point
                };

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        return new RoadNodeRecordProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(data.Select(d => d.ExpectedRecord));
    }
}
