namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenMigratingRoadNetwork;

using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.MigrateRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Tests.BackOffice;
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
        var command = new MigrateRoadNetworkSqsRequest
        {
            UploadId = TestData.Fixture.Create<UploadId>(),
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

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);

        var nisCode = "12345";
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = nisCode,
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        var existingRoadSegmentId = new RoadSegmentId(2);
        extractsDbContext.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            RoadSegmentId = existingRoadSegmentId,
            NisCode = nisCode,
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

        var existingInwinningRoadSegment = extractsDbContext.InwinningRoadSegments
            .Single(x => x.RoadSegmentId == existingRoadSegmentId);
        existingInwinningRoadSegment.Completed.Should().BeTrue();

        var newInwinningRoadSegment = extractsDbContext.InwinningRoadSegments
            .Single(x => x.RoadSegmentId == TestData.Segment1Added.RoadSegmentId);
        newInwinningRoadSegment.NisCode.Should().Be(nisCode);
        newInwinningRoadSegment.Completed.Should().BeTrue();
    }

    [Fact]
    public async Task WithMultipleRunsForTheSameDownloadId_ThenOnlyAppliesChangesOnce()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var provenanceData = new RoadRegistryProvenanceData();
        var command = new MigrateRoadNetworkSqsRequest
        {
            UploadId = TestData.Fixture.Create<UploadId>(),
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

        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();
        await AddExtractDownloadAndUpload(extractsDbContext, command);
        extractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            DownloadId = command.DownloadId,
            NisCode = "12345",
            Contour = new WKTReader().Read(GeometryTranslatorTestCases.ValidPolygon),
            Operator = provenanceData.Operator,
            Completed = false
        });
        await extractsDbContext.SaveChangesAsync(CancellationToken.None);

        var handler = sp.GetRequiredService<MigrateRoadNetworkSqsLambdaRequestHandler>();

        // Act
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);
        await handler.Handle(new MigrateRoadNetworkSqsLambdaRequest(string.Empty, command), CancellationToken.None);

        // Assert
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

        var inwinningsZone = extractsDbContext.Inwinningszones.Single(x => x.DownloadId == command.DownloadId.ToGuid());
        inwinningsZone.Completed.Should().BeTrue();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(new FakeExtractsDbContextFactory().CreateDbContext())
            .AddScoped<MigrateRoadNetworkSqsLambdaRequestHandler>();
    }

    private async Task AddExtractDownloadAndUpload(ExtractsDbContext extractsDbContext, MigrateRoadNetworkSqsRequest command)
    {
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = command.DownloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = TestData.Fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = command.UploadId,
            DownloadId = command.DownloadId,
            UploadedOn = DateTimeOffset.UtcNow,
            Status = ExtractUploadStatus.Processing,
            TicketId = command.TicketId
        });
        await extractsDbContext.SaveChangesAsync();
    }
}
