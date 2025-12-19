namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema.Extensions;
using Editor.Schema.RoadNodes;
using Extracts.Schemas.ExtractV1.RoadNodes;
using Microsoft.IO;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Point = NetTopologySuite.Geometries.Point;

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

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadNodeAdded = change.RoadNodeAdded;
            var point = GeometryTranslator.Translate(roadNodeAdded.Geometry);
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                DbaseRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = roadNodeAdded.Id },
                    WK_UIDN = { Value = $"{roadNodeAdded.Id}_{roadNodeAdded.Version}" },
                    TYPE = { Value = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier },
                    LBLTYPE =
                    {
                        Value = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name
                    },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContent = pointShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                Geometry = point
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
            var pointShapeContent = new PointShapeContent(
                Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                    new Point(point.X, point.Y)));

            return (object)new RoadNodeRecord
            {
                Id = roadNodeAdded.Id,
                DbaseRecord = new RoadNodeDbaseRecord
                {
                    WK_OIDN = { Value = roadNodeAdded.Id },
                    WK_UIDN = { Value = $"{roadNodeAdded.Id}_{roadNodeAdded.Version}" },
                    TYPE = { Value = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Identifier },
                    LBLTYPE =
                    {
                        Value = RoadNodeType.Parse(roadNodeAdded.Type).Translation.Name
                    },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadNodeModified.When) },
                    BEGINORG = { Value = acceptedRoadNodeModified.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadNodeModified.Organization }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContent = pointShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                Geometry = point
            }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));
        });

        return new RoadNodeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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

        return new RoadNodeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
                var pointShapeContent = new PointShapeContent(
                    Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                        new Point(point.X, point.Y)
                    )
                );
                var expectedRecord = new RoadNodeRecord
                {
                    Id = @event.Id,
                    DbaseRecord = new RoadNodeDbaseRecord
                    {
                        WK_OIDN = { Value = @event.Id },
                        WK_UIDN = { Value = @event.Id + "_" + @event.Version },
                        TYPE = { Value = RoadNodeType.Parse(@event.Type).Translation.Identifier },
                        LBLTYPE =
                        {
                            Value = RoadNodeType.Parse(@event.Type).Translation.Name
                        },
                        BEGINTIJD = { Value = @event.Origin.Since },
                        BEGINORG = { Value = @event.Origin.OrganizationId },
                        LBLBGNORG = { Value = @event.Origin.Organization }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    ShapeRecordContent = pointShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                    Geometry = point
                }.WithBoundingBox(RoadNodeBoundingBox.From(pointShapeContent.Shape));

                return new
                {
                    ImportedRoadNode = @event,
                    ExpectedRecord = expectedRecord
                };
            }).ToList();

        return new RoadNodeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.ImportedRoadNode))
            .Expect(data.Select(d => d.ExpectedRecord));
    }
}
