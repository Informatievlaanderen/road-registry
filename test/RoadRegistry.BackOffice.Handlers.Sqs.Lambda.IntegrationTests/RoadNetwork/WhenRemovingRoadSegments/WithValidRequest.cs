namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenRemovingRoadSegments;

using Actions.RemoveRoadSegments;
using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction.Changes;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.Geometries;
using RoadNode.Changes;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.DomainV2;
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
    public async Task GivenRoadNetwork_ThenSucceeded()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var fixture = TestData.Fixture;

        var node1 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(1),
            Geometry = new Point(200000, 200000).ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };
        var node2 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(2),
            Geometry = new Point(200010, 200000).ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Schijnknoop,
            Grensknoop = false
        };
        var node3 = new AddRoadNodeChange
        {
            TemporaryId = new RoadNodeId(3),
            Geometry = new Point(200020, 200000).ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop,
            Grensknoop = false
        };
        var segment1_node_1_2 = fixture.Create<AddRoadSegmentChange>() with
        {
            TemporaryId = new RoadSegmentId(1),
            OriginalId = null,
            StartNodeId = node1.TemporaryId,
            EndNodeId = node2.TemporaryId,
            Geometry = BuildMultiLineString(node1.Geometry.Value, node2.Geometry.Value),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        segment1_node_1_2 = segment1_node_1_2 with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(RoadSegmentStatusV2.Gerealiseerd, segment1_node_1_2.Geometry),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.RegionaleWeg, segment1_node_1_2.Geometry)
        };
        var segment2_node_2_3 = fixture.Create<AddRoadSegmentChange>() with
        {
            TemporaryId = new RoadSegmentId(2),
            OriginalId = null,
            StartNodeId = node2.TemporaryId,
            EndNodeId = node3.TemporaryId,
            Geometry = BuildMultiLineString(node2.Geometry.Value, node3.Geometry.Value),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        segment2_node_2_3 = segment2_node_2_3 with
        {
            Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(RoadSegmentStatusV2.Gehistoreerd, segment2_node_2_3.Geometry),
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.RegionaleWeg, segment2_node_2_3.Geometry)
        };

        var node4 = TestData.AddSegment3StartNode with
        {
            TemporaryId = new RoadNodeId(4),
            Geometry = new Point(200090, 200000).ToRoadNodeGeometry(),
        };
        var node5 = TestData.AddSegment3EndNode with
        {
            TemporaryId = new RoadNodeId(5),
            Geometry = new Point(200095, 200000).ToRoadNodeGeometry(),
        };
        var segment3 = TestData.AddSegment3 with
        {
            TemporaryId = new RoadSegmentId(3),
            StartNodeId = node4.TemporaryId,
            EndNodeId = node5.TemporaryId,
            Geometry = BuildMultiLineString(node4.Geometry.Value, node5.Geometry.Value)
        };
        segment3 = segment3 with
        {
            Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>(RoadSegmentCategoryV2.RegionaleWeg, segment3.Geometry)
        };

        var junction = new AddGradeSeparatedJunctionChange
        {
            TemporaryId = new(1),
            Type = fixture.Create<GradeSeparatedJunctionTypeV2>(),
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

        var provenanceData = new RoadRegistryProvenanceData();
        var sqsRequest = new RemoveRoadSegmentsSqsRequest
        {
            RoadSegmentIds = [TestData.Segment1Added.RoadSegmentId],
            ProvenanceData = provenanceData,
            TicketId = Guid.NewGuid()
        };

        // Act
        var handler = sp.GetRequiredService<RemoveRoadSegmentsSqsLambdaRequestHandler>();
        await handler.Handle(new RemoveRoadSegmentsSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        // Assert
        var ticketResult = TicketingMock.Invocations
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
        roadSegments.Single(x => x.RoadSegmentId == TestData.Segment1Added.RoadSegmentId).IsRemoved.Should().BeTrue();
        roadSegments.Single(x => x.RoadSegmentId == TestData.Segment2Added.RoadSegmentId).IsRemoved.Should().BeFalse();
        roadSegments.Single(x => x.RoadSegmentId == TestData.Segment3Added.RoadSegmentId).IsRemoved.Should().BeFalse();

        var junctions = await session.LoadManyAsync([junction.TemporaryId]);
        junctions.Should().HaveCount(1);
        junctions.Single(x => x.GradeSeparatedJunctionId == junction.TemporaryId).IsRemoved.Should().BeTrue();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services
            .AddSingleton(_extractRequestsMock.Object)
            .AddScoped<RemoveRoadSegmentsSqsLambdaRequestHandler>();
    }

    private static RoadSegmentGeometry BuildMultiLineString(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates()
            .ToRoadSegmentGeometry();
    }
}
