namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using Integration.Projections.Version;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Schema.RoadNodes;
using Point = NetTopologySuite.Geometries.Point;
using RoadNodeVersion = Schema.RoadNodes.Version.RoadNodeVersion;

public class RoadNodeVersionProjectionTests
{
    private const long InitialPosition = IntegrationContextScenarioExtensions.InitialPosition;

    private readonly Fixture _fixture;

    public RoadNodeVersionProjectionTests()
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

        var initialPosition = InitialPosition;

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeVersion
            {
                Position = initialPosition,
                Id = roadNodeAdded.Id,
                TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                Version = roadNodeAdded.Version,
                OrganizationId =  message.OrganizationId,
                OrganizationName =  message.Organization,
                IsRemoved = false,
                Geometry = point,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeVersionProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_nodes()
    {
        _fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = _fixture.Create<RoadNodeAdded>();
        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeAdded);

        var roadNodeModified = _fixture.Create<RoadNodeModified>();
        var acceptedRoadNodeModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeModified);

        var initialPosition = InitialPosition;

        var roadNodeAddedPoint = GeometryTranslator.Translate(roadNodeAdded.Geometry);
        var roadNodeAddedPointShapeContent = new PointShapeContent(
            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                new Point(roadNodeAddedPoint.X, roadNodeAddedPoint.Y)));

        var roadNodeModifiedPoint = GeometryTranslator.Translate(roadNodeModified.Geometry);
        var roadNodeModifiedPointShapeContent = new PointShapeContent(
            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                new Point(roadNodeModifiedPoint.X, roadNodeModifiedPoint.Y)));

        return new RoadNodeVersionProjection()
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeModified)
            .Expect([
                new RoadNodeVersion
                {
                    Position = initialPosition,
                    Id = roadNodeAdded.Id,
                    TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                    Version = roadNodeAdded.Version,
                    OrganizationId =  acceptedRoadNodeAdded.OrganizationId,
                    OrganizationName =  acceptedRoadNodeAdded.Organization,
                    IsRemoved = false,
                    Geometry = roadNodeAddedPoint,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                }.WithBoundingBox(RoadNodeBoundingBox.From(roadNodeAddedPointShapeContent.Shape)),
                new RoadNodeVersion
                {
                    Position = ++initialPosition,
                    Id = roadNodeModified.Id,
                    TypeId = RoadNodeType.Parse(roadNodeModified.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(roadNodeModified.Type).Translation.Name,
                    Version = roadNodeModified.Version,
                    OrganizationId =  acceptedRoadNodeModified.OrganizationId,
                    OrganizationName =  acceptedRoadNodeModified.Organization,
                    IsRemoved = false,
                    Geometry = roadNodeModifiedPoint,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeModified.When),
                }.WithBoundingBox(RoadNodeBoundingBox.From(roadNodeModifiedPointShapeContent.Shape))
            ]);
    }

    [Fact]
    public Task When_removing_road_nodes()
    {
        _fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = _fixture.Create<RoadNodeAdded>();
        var acceptedRoadNodeAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeAdded);

        var roadNodeRemoved = _fixture.Create<RoadNodeRemoved>();
        var acceptedRoadNodeRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadNodeRemoved);

        var initialPosition = InitialPosition;

        var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);
        var pointShapeContent = new PointShapeContent(
            Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                new Point(point.X, point.Y)));

        return new RoadNodeVersionProjection()
            .Scenario()
            .Given(acceptedRoadNodeAdded, acceptedRoadNodeRemoved)
            .Expect([
                new RoadNodeVersion
                {
                    Position = initialPosition,
                    Id = roadNodeAdded.Id,
                    TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                    Version = roadNodeAdded.Version,
                    OrganizationId =  acceptedRoadNodeAdded.OrganizationId,
                    OrganizationName =  acceptedRoadNodeAdded.Organization,
                    IsRemoved = false,
                    Geometry = point,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape)),
                new RoadNodeVersion
                {
                    Position = ++initialPosition,
                    Id = roadNodeAdded.Id,
                    TypeId = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name,
                    Version = roadNodeAdded.Version,
                    OrganizationId =  acceptedRoadNodeAdded.OrganizationId,
                    OrganizationName =  acceptedRoadNodeAdded.Organization,
                    IsRemoved = true,
                    Geometry = point,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeRemoved.When),
                }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape))
            ]);
    }

    [Fact]
    public Task When_road_nodes_were_imported()
    {
        var initialPosition = InitialPosition;

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

                var expectedRecord = new RoadNodeVersion
                {
                    Position = initialPosition++,
                    Id = @event.Id,
                    TypeId = RoadNodeType.Parse(@event.Type).Translation.Identifier,
                    TypeLabel = RoadNodeType.Parse(@event.Type).Translation.Name,
                    Version = @event.Version,
                    OrganizationId =  @event.Origin.OrganizationId,
                    OrganizationName =  @event.Origin.Organization,
                    IsRemoved = false,
                    Geometry = point,
                    CreatedOnTimestamp = @event.Origin.Since.ToBelgianInstant(),
                    VersionTimestamp = @event.Origin.Since.ToBelgianInstant()
                }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        return new RoadNodeVersionProjection()
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(data.Select(d => d.ExpectedRecord));
    }
}
