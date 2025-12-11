namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenRemovingRoadSegments;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling;
using CommandHandling.Actions.RemoveRoadSegments;
using CommandHandling.Extracts;
using FeatureCompare.V3;
using FluentAssertions;
using GradeSeparatedJunction.Changes;
using Handlers.RoadNetwork;
using Hosts;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.Geometries;
using Requests.RoadNetwork;
using RoadNode.Changes;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNetwork;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Sqs.RoadNetwork;
using Tests.AggregateTests;
using Tests.BackOffice;
using Tests.Framework;
using TicketingService.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WithValidRequest : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly Mock<ITicketing> _ticketingMock = new();
    private readonly Mock<IExtractRequests> _extractRequestsMock = new();

    public WithValidRequest(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var testData = new RoadNetworkTestData();
        var fixture = testData.Fixture;

        var node1 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(1),
            Geometry = new Point(200000, 200000),
            Type = RoadNodeType.EndNode
        };
        var node2 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(2),
            Geometry = new Point(200010, 200000),
            Type = RoadNodeType.FakeNode
        };
        var node3 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(3),
            Geometry = new Point(200020, 200000),
            Type = RoadNodeType.EndNode
        };
        var segment1_node_1_2 = fixture.Create<AddRoadSegmentChange>() with
        {
            TemporaryId = new RoadSegmentId(1),
            OriginalId = null,
            StartNodeId = node1.TemporaryId,
            EndNodeId = node2.TemporaryId,
            Geometry = BuildSegmentGeometry(node1.Geometry, node2.Geometry),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.InUse),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.RegionalRoad),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        var segment2_node_2_3 = fixture.Create<AddRoadSegmentChange>() with
        {
            TemporaryId = new RoadSegmentId(2),
            OriginalId = null,
            StartNodeId = node2.TemporaryId,
            EndNodeId = node3.TemporaryId,
            Geometry = BuildSegmentGeometry(node2.Geometry, node3.Geometry),
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.OutOfUse),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.RegionalRoad),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };

        var node4 = testData.AddSegment3StartNode with
        {
            TemporaryId = new RoadNodeId(4),
            Geometry = new Point(200090, 200000),
        };
        var node5 = testData.AddSegment3EndNode with
        {
            TemporaryId = new RoadNodeId(5),
            Geometry = new Point(200095, 200000),
        };
        var segment3 = testData.AddSegment3 with
        {
            TemporaryId = new RoadSegmentId(3),
            StartNodeId = node4.TemporaryId,
            EndNodeId = node5.TemporaryId,
            Geometry = BuildSegmentGeometry(node4.Geometry, node5.Geometry),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.RegionalRoad)
        };

        var junction = new AddGradeSeparatedJunctionChange
        {
            TemporaryId = new(1),
            Type = fixture.Create<GradeSeparatedJunctionType>(),
            LowerRoadSegmentId = segment1_node_1_2.TemporaryId,
            UpperRoadSegmentId = segment2_node_2_3.TemporaryId
        };

        await Given(sp, TranslatedChanges.Empty
            .AppendChange(node1)
            .AppendChange(node2)
            .AppendChange(node3)
            .AppendChange(segment1_node_1_2)
            .AppendChange(segment2_node_2_3)
            .AppendChange(node4)
            .AppendChange(node5)
            .AppendChange(segment3)
            .AppendChange(junction));

        var command = new RemoveRoadSegmentsCommand
        {
            RoadSegmentIds = [testData.Segment1Added.RoadSegmentId]
        };

        var provenanceData = new RoadRegistryProvenanceData();
        var sqsRequest = new RemoveRoadSegmentsCommandSqsRequest
        {
            Request = command,
            ProvenanceData = provenanceData,
            TicketId = Guid.NewGuid()
        };

        // Act
        var handler = sp.GetRequiredService<RemoveRoadSegmentsCommandSqsLambdaRequestHandler>();
        await handler.Handle(new RemoveRoadSegmentsCommandSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        // Assert
        var ticketResult = _ticketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(sqsRequest.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([node1.TemporaryId, node2.TemporaryId]);
        roadNodes.Should().HaveCount(2);
        roadNodes.Single(x => x.RoadNodeId == node1.TemporaryId).IsRemoved.Should().BeTrue();
        roadNodes.Single(x => x.RoadNodeId == node2.TemporaryId).IsRemoved.Should().BeFalse();
        roadNodes.Single(x => x.RoadNodeId == node2.TemporaryId).Type.Should().Be(RoadNodeType.EndNode);

        var roadSegments = await session.LoadManyAsync([segment1_node_1_2.TemporaryId, segment2_node_2_3.TemporaryId, segment3.TemporaryId]);
        roadSegments.Should().HaveCount(3);
        roadSegments.Single(x => x.RoadSegmentId == testData.Segment1Added.RoadSegmentId).IsRemoved.Should().BeTrue();
        roadSegments.Single(x => x.RoadSegmentId == testData.Segment2Added.RoadSegmentId).IsRemoved.Should().BeFalse();
        roadSegments.Single(x => x.RoadSegmentId == testData.Segment3Added.RoadSegmentId).IsRemoved.Should().BeFalse();

        var junctions = await session.LoadManyAsync([junction.TemporaryId]);
        junctions.Should().HaveCount(1);
        junctions.Single(x => x.GradeSeparatedJunctionId == junction.TemporaryId).IsRemoved.Should().BeTrue();
    }

    private async Task Given(IServiceProvider sp, TranslatedChanges changes)
    {
        var fixture = new RoadNetworkTestData().Fixture;
        var command = changes.ToChangeRoadNetworkCommand(fixture.Create<DownloadId>(), fixture.Create<TicketId>());

        var provenanceData = new RoadRegistryProvenanceData();
        var sqsRequest = new ChangeRoadNetworkCommandSqsRequest
        {
            Request = command,
            ProvenanceData = provenanceData
        };

        var handler = sp.GetRequiredService<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkCommandSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        var ticketResult = _ticketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();
    }

    private async Task<IServiceProvider> BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", _databaseFixture.ConnectionString)
            ])
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(_ticketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(_extractRequestsMock.Object);

        services
            .AddMartenRoad(options => options.AddRoadNetworkTopologyProjection().AddRoadAggregatesSnapshots())
            .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddRoadRegistryCommandHandlers()
            .AddScoped<ChangeRoadNetworkCommandSqsLambdaRequestHandler>()
            .AddScoped<RemoveRoadSegmentsCommandSqsLambdaRequestHandler>();

        var sp = services.BuildServiceProvider();

        // force create marten schema
        var store = sp.GetRequiredService<IDocumentStore>();
        await EnsureRoadNetworkTopologyProjectionExists(store);

        return sp;
    }

    private static async Task EnsureRoadNetworkTopologyProjectionExists(IDocumentStore store)
    {
        // build projection tables
        await store.BuildProjectionDaemonAsync();
    }

    private static MultiLineString BuildSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates();
    }
}
