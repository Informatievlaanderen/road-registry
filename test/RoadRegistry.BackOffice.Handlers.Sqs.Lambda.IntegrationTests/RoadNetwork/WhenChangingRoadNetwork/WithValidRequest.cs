namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using TicketingService.Abstractions;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WithValidRequest : RoadNetworkIntegrationTest
{
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
            UploadId = TestData.Fixture.Create<UploadId>(),
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

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

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

        var inwinningRoadSegment = extractsDbContext.InwinningRoadSegments
            .Single(x => x.RoadSegmentId == TestData.Segment1Added.RoadSegmentId);
        inwinningRoadSegment.NisCode.Should().BeNull();
        inwinningRoadSegment.Completed.Should().BeTrue();
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

        var changedCategory = TestData.Fixture.Create<RoadSegmentCategoryV2>();
        var provenanceData = new RoadRegistryProvenanceData();
        var command = new ChangeRoadNetworkSqsRequest
        {
            DownloadId = TestData.Fixture.Create<DownloadId>(),
            UploadId = TestData.Fixture.Create<UploadId>(),
            TicketId = TestData.Fixture.Create<TicketId>(),
            Changes = [
                new ChangeRoadNetworkItem
                {
                    ModifyRoadSegment = new ModifyRoadSegmentChange
                    {
                        RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
                        Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(changedCategory, TestData.Segment1Added.Geometry)
                    }
                }
            ],
            ProvenanceData = provenanceData
        };

        using var scope = sp.CreateScope();
        await using var extractsDbContext = scope.ServiceProvider.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

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
            UploadId = TestData.Fixture.Create<UploadId>(),
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

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();

        // Act
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
        PrintTicketErrorsIfAvailable(command.TicketId);

        var ticketResults = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(command.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .ToArray();
        ticketResults.Should().HaveCount(2);

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNodes = await session.LoadManyAsync([TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId]);
        roadNodes.Should().HaveCount(2);

        var roadSegments = await session.LoadManyAsync([TestData.Segment1Added.RoadSegmentId]);
        roadSegments.Should().HaveCount(1);
    }
}
