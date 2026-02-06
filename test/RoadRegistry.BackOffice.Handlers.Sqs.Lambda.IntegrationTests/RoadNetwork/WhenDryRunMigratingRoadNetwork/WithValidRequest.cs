namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenDryRunMigratingRoadNetwork;

using Actions.MigrateDryRunRoadNetwork;
using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Tests.BackOffice;
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
        var command = new MigrateDryRunRoadNetworkSqsRequest
        {
            MigrateRoadNetworkSqsRequest = new MigrateRoadNetworkSqsRequest
            {
                UploadId = TestData.Fixture.Create<UploadId>(),
                DownloadId = TestData.Fixture.Create<DownloadId>(),
                Changes =
                [
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
                ]
            },
            TicketId = TestData.Fixture.Create<TicketId>(),
            ProvenanceData = provenanceData
        };

        var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.MigrateRoadNetworkSqsRequest.DownloadId,
            NisCode = "12345",
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        // Act
        var handler = sp.GetRequiredService<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new MigrateDryRunRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().BeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().BeEmpty();

        var roadSegments = await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().BeEmpty();

        var inwinningsZone = extractsDbContext.Inwinningszones.Single(x => x.DownloadId == command.MigrateRoadNetworkSqsRequest.DownloadId.ToGuid());
        inwinningsZone.Completed.Should().BeFalse();

        MediatorMock.Verify(x => x.Send(
            It.Is<DataValidationSqsRequest>(r => r.MigrateRoadNetworkSqsRequest == command.MigrateRoadNetworkSqsRequest && r.TicketId == command.TicketId),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_extractRequestsMock.Object)
            .AddSingleton(new FakeExtractsDbContextFactory().CreateDbContext())
            .AddScoped<MigrateDryRunRoadNetworkSqsLambdaRequestHandler>();
    }
}
