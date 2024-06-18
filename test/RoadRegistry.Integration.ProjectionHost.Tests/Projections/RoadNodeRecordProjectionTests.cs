namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using Integration.Schema.RoadNodes;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Point = NetTopologySuite.Geometries.Point;

public class RoadNodeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadNodeLatestItemProjectionTests()
    {
        _fixture = new RoadNetworkTestData().ObjectProvider;
        _fixture.CustomizeImportedRoadNode();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeRoadNodeId();
        _fixture.CustomizeRoadNodeType();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizeOrganizationName();
        _fixture.CustomizePoint();

        _fixture.CustomizeRoadNodeAdded();
        _fixture.CustomizeRoadNodeModified();
        _fixture.CustomizeRoadNodeRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();

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
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeLatestItem
            {
                Id = roadNodeAdded.Id,
                TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                Version = roadNodeAdded.Version,
                BeginOrganizationId =  message.OrganizationId,
                BeginOrganizationName =  message.Organization,
                IsRemoved = false,
                Geometry = point,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeLatestItemProjection()
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
            var roadNodeModified = change.RoadNodeModified;
            var point = GeometryTranslator.Translate(roadNodeModified.Geometry);
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeLatestItem
            {
                Id = roadNodeModified.Id,
                TypeId = RoadNodeType.Parse(roadNodeModified.Type).Translation.Identifier,
                TypeLabel = RoadNodeType.Parse(roadNodeModified.Type).Translation.Name,
                Version = roadNodeModified.Version,
                BeginOrganizationId =  acceptedRoadNodeModified.OrganizationId,
                BeginOrganizationName =  acceptedRoadNodeModified.Organization,
                IsRemoved = false,
                Geometry = point,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeModified.When),
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeLatestItemProjection()
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

        var expectedRecords = Array.ConvertAll(acceptedRoadNodeAdded.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeLatestItem
            {
                Id = roadNodeAdded.Id,
                TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                Version = roadNodeAdded.Version,
                BeginOrganizationId =  acceptedRoadNodeAdded.OrganizationId,
                BeginOrganizationName =  acceptedRoadNodeAdded.Organization,
                IsRemoved = true,
                Geometry = point,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeRemoved.When),
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_road_nodes_were_imported()
    {
        var data = _fixture
            .CreateMany<ImportedRoadNode>(new Random().Next(1, 100))
            .Select(@event =>
            {
                var point = GeometryTranslator.Translate(@event.Geometry);
                var pointShapeContent = new PointShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                        new Point(point.X, point.Y)
                    )
                );

                var expectedRecord = new RoadNodeLatestItem
                {
                    Id = @event.Id,
                    TypeId = RoadNodeType.Parse(@event.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(@event.Type).Translation.Name,
                    Version = @event.Version,
                    BeginOrganizationId =  @event.Origin.OrganizationId,
                    BeginOrganizationName =  @event.Origin.Organization,
                    IsRemoved = false,
                    Geometry = point,
                    CreatedOnTimestamp = @event.Origin.Since,
                    VersionTimestamp = @event.Origin.Since
                }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        return new RoadNodeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(data.Select(d => d.ExpectedRecord));
    }
}
