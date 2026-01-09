namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkRepositoryTests;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.Extensions;
using RoadRegistry.Infrastructure.MartenDb;
using RoadSegment;
using ScopedRoadNetwork;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WhenGetUnderlyingIdsWithConnectedSegments : RoadNetworkIntegrationTest
{
    public WhenGetUnderlyingIdsWithConnectedSegments(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task WithNoIds_ThenEmptyResult()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        // Act
        var ids = await GetUnderlyingIdsWithConnectedSegments(sp, []);

        // Assert
        ids.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task GivenV1AndV2_ThenOnlyV2()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession())
        {
            var v1RoadNode1 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V1.ImportedRoadNode>();
            v1RoadNode1.RoadNodeId = 1;
            var v1RoadNode2 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V1.ImportedRoadNode>();
            v1RoadNode2.RoadNodeId = 2;
            var v1RoadSegment = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment>();
            v1RoadSegment.RoadSegmentId = 1;
            v1RoadSegment.StartNodeId = v1RoadNode1.RoadNodeId;
            v1RoadSegment.EndNodeId = v1RoadNode2.RoadNodeId;
            var v1Junction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction>();
            v1Junction.Id = 1;
            v1Junction.LowerRoadSegmentId = v1RoadSegment.RoadSegmentId;
            v1Junction.UpperRoadSegmentId = v1RoadSegment.RoadSegmentId;

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), v1RoadNode1.RoadNodeId), v1RoadNode1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), v1RoadNode2.RoadNodeId), v1RoadNode2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), v1RoadSegment.RoadSegmentId), v1RoadSegment);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), v1Junction.Id), v1Junction);

            var v2RoadNode1 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
            {
                RoadNodeId = new RoadNodeId(11)
            };
            var v2RoadNode2 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
            {
                RoadNodeId = new RoadNodeId(12)
            };
            var v2RoadSegment = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(11),
                StartNodeId = v2RoadNode1.RoadNodeId,
                EndNodeId = v2RoadNode2.RoadNodeId
            };
            var v2Junction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(11),
                LowerRoadSegmentId = v2RoadSegment.RoadSegmentId,
                UpperRoadSegmentId = v2RoadSegment.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), v2RoadNode1.RoadNodeId), v2RoadNode1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), v2RoadNode2.RoadNodeId), v2RoadNode2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), v2RoadSegment.RoadSegmentId), v2RoadSegment);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), v2Junction.GradeSeparatedJunctionId), v2Junction);

            await session.SaveChangesAsync();
        }

        // Act
        var ids = await GetUnderlyingIdsWithConnectedSegments(sp, [new(1), new(11)]);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(11), new(12)],
            [new(11)],
            [new(11)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task GivenTwoConnectionLevels_ThenReturnUpToFirstLevelConnection()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();

        var roadNode1 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
        {
            RoadNodeId = new RoadNodeId(1)
        };
        var roadNode2 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
        {
            RoadNodeId = new RoadNodeId(2)
        };
        var roadNode3 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
        {
            RoadNodeId = new RoadNodeId(3)
        };
        var roadNode4 = TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
        {
            RoadNodeId = new RoadNodeId(4)
        };
        var roadSegment1 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(1),
            StartNodeId = roadNode1.RoadNodeId,
            EndNodeId = roadNode2.RoadNodeId
        };
        var roadSegment2 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(2),
            StartNodeId = roadNode2.RoadNodeId,
            EndNodeId = roadNode3.RoadNodeId
        };
        var roadSegment3 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(3),
            StartNodeId = roadNode3.RoadNodeId,
            EndNodeId = roadNode4.RoadNodeId
        };
        var junction1 = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = roadSegment1.RoadSegmentId,
            UpperRoadSegmentId = roadSegment2.RoadSegmentId
        };
        var junction2 = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(2),
            LowerRoadSegmentId = roadSegment2.RoadSegmentId,
            UpperRoadSegmentId = roadSegment3.RoadSegmentId
        };
        var junction3 = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(3),
            LowerRoadSegmentId = roadSegment3.RoadSegmentId,
            UpperRoadSegmentId = roadSegment3.RoadSegmentId
        };

        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), roadNode1.RoadNodeId), roadNode1);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), roadNode2.RoadNodeId), roadNode2);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), roadNode3.RoadNodeId), roadNode3);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadNode), roadNode4.RoadNodeId), roadNode4);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment3.RoadSegmentId), roadSegment3);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction1.GradeSeparatedJunctionId), junction1);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction2.GradeSeparatedJunctionId), junction2);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction3.GradeSeparatedJunctionId), junction3);

        await session.SaveChangesAsync();

        // Act
        var requestIds = new[] { roadSegment1.RoadSegmentId };
        var ids = await GetUnderlyingIdsWithConnectedSegments(sp, requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [roadNode1.RoadNodeId, roadNode2.RoadNodeId, roadNode3.RoadNodeId],
            [roadSegment1.RoadSegmentId, roadSegment2.RoadSegmentId],
            [junction1.GradeSeparatedJunctionId, junction2.GradeSeparatedJunctionId]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    private async Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IServiceProvider sp, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repo = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return await repo.GetUnderlyingIdsWithConnectedSegments(session, roadSegmentIds);
    }
}
