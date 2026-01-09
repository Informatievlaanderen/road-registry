namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenMigratingRoadNetwork;

using Actions.MigrateRoadNetwork;
using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.IO;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using Sqs.RoadNetwork;
using Tests.AggregateTests;
using Tests.BackOffice;
using TicketingService.Abstractions;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WithValidRequest : RoadNetworkIntegrationTest
{
    private readonly Mock<IExtractRequests> _extractRequestsMock = new();

    public WithValidRequest(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task GivenEmptyRoadNetwork_ThenSucceeded()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new MigrateRoadNetworkSqsRequest
        {
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = TestData.AddSegment1StartNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = TestData.AddSegment1EndNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ],
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = "12345",
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().HaveCount(1);

        var inwinningsZone = extractsDbContext.Inwinningszones.Single(x => x.DownloadId == command.DownloadId.ToGuid());
        inwinningsZone.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task WithMultipleRunsForTheSameDownloadId_ThenOnlyAppliesChangesOnce()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new MigrateRoadNetworkSqsRequest
        {
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = TestData.AddSegment1StartNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadNode = TestData.AddSegment1EndNode
                },
                new ChangeRoadNetworkItem
                {
                    AddRoadSegment = TestData.AddSegment1
                }
            ],
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = "12345",
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

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
        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();

        // Act
        var actFirstRun = () => handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);
        await actFirstRun.Should().ThrowAsync<Exception>();

        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().HaveCount(1);

        var inwinningsZone = extractsDbContext.Inwinningszones.Single(x => x.DownloadId == command.DownloadId.ToGuid());
        inwinningsZone.Completed.Should().BeTrue();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_extractRequestsMock.Object)
            .AddSingleton(new FakeExtractsDbContextFactory().CreateDbContext())
            .AddScoped<MigrateRoadNetworkSqsLambdaRequestHandler>();
    }
}
