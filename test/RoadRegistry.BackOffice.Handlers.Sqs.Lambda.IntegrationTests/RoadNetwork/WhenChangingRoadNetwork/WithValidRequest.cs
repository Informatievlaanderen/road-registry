namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using Actions.ChangeRoadNetwork;
using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoadRegistry.Extracts.FeatureCompare.V3;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadSegment.Changes;
using RoadSegment.ValueObjects;
using Sqs.RoadNetwork;
using Tests.AggregateTests;
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
        var command = new ChangeRoadNetworkSqsRequest
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

        // Act
        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

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
    }

    [Fact]
    public async Task GivenRoadNetwork_ThenSucceeded()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        await Given(sp, TranslatedChanges.Empty
            .AppendChange(TestData.AddSegment1StartNode)
            .AppendChange(TestData.AddSegment1EndNode)
            .AppendChange(TestData.AddSegment1));

        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();

        var changedCategory = TestData.Fixture.Create<RoadSegmentCategory>();
        var provenanceData = new RoadRegistryProvenanceData();
        var command = new ChangeRoadNetworkSqsRequest
        {
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    ModifyRoadSegment = new ModifyRoadSegmentChange
                    {
                        RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(changedCategory)
                    }
                }
            ],
            ProvenanceData = provenanceData
        };

        // Act
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadSegment = (await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId])).Single();
        roadSegment.Attributes.Category.Values.Single().Value.Should().Be(changedCategory);
    }

    [Fact]
    public async Task WithMultipleRunsForTheSameDownloadId_ThenOnlyAppliesChangesOnce()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new ChangeRoadNetworkSqsRequest
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
        PrintTicketErrorsIfAvailable(command.TicketId);

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
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_extractRequestsMock.Object);
    }
}
