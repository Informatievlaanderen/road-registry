namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using Actions.ChangeRoadNetwork;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using FluentAssertions;
using Hosts;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNetwork;
using ScopedRoadNetwork;
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
        var command = new ChangeRoadNetworkSqsRequest
        {
            DownloadId = testData.Fixture.Create<DownloadId>(),
            TicketId = testData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = testData.AddSegment1StartNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = testData.AddSegment1EndNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = testData.AddSegment1
                }
            ],
            ProvenanceData = provenanceData
        };

        // Act
        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = _ticketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([testData.Segment1StartNodeAdded.RoadNodeId, testData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadManyAsync([testData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().HaveCount(1);
    }

    //TODO-pr add test with Given data

    [Fact]
    public async Task WithMultipleRunsForTheSameDownloadId_ThenOnlyAppliesChangesOnce()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var testData = new RoadNetworkTestData();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new ChangeRoadNetworkSqsRequest
        {
            DownloadId = testData.Fixture.Create<DownloadId>(),
            TicketId = testData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = testData.AddSegment1StartNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = testData.AddSegment1EndNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = testData.AddSegment1
                }
            ],
            ProvenanceData = provenanceData
        };

        var uploadAcceptedCount = 0;
        _extractRequestsMock
            .Setup(x => x.UploadAcceptedAsync(It.IsAny<DownloadId>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                uploadAcceptedCount++;

                if (uploadAcceptedCount == 1)
                {
                    throw new Exception("Unexpected error");
                }

                return Task.CompletedTask;
            });
        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();

        // Act
        var actFirstRun = () => handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);
        await actFirstRun.Should().ThrowAsync<Exception>();

        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = _ticketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([testData.Segment1StartNodeAdded.RoadNodeId, testData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadManyAsync([testData.Segment1Added.RoadSegmentId]);
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
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>())
            .AddSingleton(_extractRequestsMock.Object);

        services
            .AddMartenRoad(options => options.AddRoadNetworkTopologyProjection().AddRoadAggregatesSnapshots())
            .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddScoped<ChangeRoadNetworkSqsLambdaRequestHandler>();

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
}
