namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork;
using Core;
using FluentAssertions;
using Handlers.RoadNetwork;
using Hosts;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Requests.RoadNetwork;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
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

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new ChangeRoadNetworkCommand
        {
            DownloadId = Guid.NewGuid(),
            TicketId = Guid.NewGuid(),
            Changes = [
                new ChangeRoadNetworkCommandItem
                {
                    AddRoadNode = testData.AddSegment1StartNode
                },
                new ChangeRoadNetworkCommandItem
                {
                    AddRoadNode = testData.AddSegment1EndNode
                },
                new ChangeRoadNetworkCommandItem
                {
                    AddRoadSegment = testData.AddSegment1
                }
            ]
        };
        var sqsRequest = new ChangeRoadNetworkCommandSqsRequest
        {
            Request = command,
            ProvenanceData = provenanceData
        };

        // Act
        var handler = sp.GetRequiredService<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkCommandSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        // Assert
        var ticketResult = _ticketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadRoadNodesAsync([testData.Segment1StartNodeAdded.RoadNodeId, testData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadRoadSegmentsAsync([testData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().HaveCount(1);
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
            .AddMartenRoad(options => options.AddRoadNetworkTopologyProjection())
            .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddChangeRoadNetworkCommandHandler()
            .AddScoped<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();

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

        // await projectionDaemon.RebuildProjectionAsync<RoadSegment>(CancellationToken.None);
        // await projectionDaemon.RebuildProjectionAsync<RoadNode>(CancellationToken.None);
        // await projectionDaemon.RebuildProjectionAsync<GradeSeparatedJunction>(CancellationToken.None);
    }
}
