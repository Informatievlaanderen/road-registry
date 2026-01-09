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
public class WhenGetUnderlyingIds : RoadNetworkIntegrationTest
{
    public WhenGetUnderlyingIds(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task WithNoGeometryAndNoIds_ThenException()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        // Act
        var act = () => GetUnderlyingIds(sp);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task WithoutV2Filter_ThenV1AndV2()
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
        var requestedIds = new RoadNetworkIds(
            [],
            [new(1), new(11)],
            []);
        var ids = await GetUnderlyingIds(sp, ids: requestedIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2), new(11), new(12)],
            [new(1), new(11)],
            [new(1), new(11)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithV2Filter_ThenOnlyV2()
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

        var requestIds = new RoadNetworkIds(
            [],
            [new(11)],
            []);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds, onlyV2: true);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(11), new(12)],
            [new(11)],
            [new(11)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithGeometry_ThenIntersecting()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            var roadSegmentIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(1),
                Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
                StartNodeId = new RoadNodeId(1),
                EndNodeId = new RoadNodeId(2)
            };
            var roadSegmentNotIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100)
            };
            var junctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentIntersecting.RoadSegmentId), roadSegmentIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentNotIntersecting.RoadSegmentId), roadSegmentNotIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junctionIntersecting.GradeSeparatedJunctionId), junctionIntersecting);

            await session.SaveChangesAsync();
        }

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).ToGeometry();

        // Act
        var ids = await GetUnderlyingIds(sp, requestGeometry);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithRoadNodeIds_ThenResult()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            var roadSegment1 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(1),
                Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
                StartNodeId = new RoadNodeId(1),
                EndNodeId = new RoadNodeId(2)
            };
            var roadSegment2 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100)
            };
            var junction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction.GradeSeparatedJunctionId), junction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [new(1)],
            [],
            []);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithRoadSegmentIds_ThenResult()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            var roadSegment1 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(1),
                Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
                StartNodeId = new RoadNodeId(1),
                EndNodeId = new RoadNodeId(2)
            };
            var roadSegment2 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100)
            };
            var junction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction.GradeSeparatedJunctionId), junction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [],
            [new(1)],
            []);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithGradeSeparatedJunctionIds_ThenResult()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();

        await using (var session = store.LightweightSession())
        {
            var roadSegment1 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(1),
                Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
                StartNodeId = new RoadNodeId(1),
                EndNodeId = new RoadNodeId(2)
            };
            var roadSegment2 = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100)
            };
            var junction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junction.GradeSeparatedJunctionId), junction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [],
            [],
            [new(1)]);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithGeometryAndIds_ThenCombinedResult()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        var store = sp.GetRequiredService<IDocumentStore>();

        await using var session = store.LightweightSession();

        var roadSegmentIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(1),
            Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
            StartNodeId = new RoadNodeId(1),
            EndNodeId = new RoadNodeId(2)
        };
        var roadSegmentNotIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(2),
            Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100),
            StartNodeId = new  RoadNodeId(3),
            EndNodeId = new RoadNodeId(4)
        };
        var junctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId
        };

        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentIntersecting.RoadSegmentId), roadSegmentIntersecting);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentNotIntersecting.RoadSegmentId), roadSegmentNotIntersecting);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), junctionIntersecting.GradeSeparatedJunctionId), junctionIntersecting);

        await session.SaveChangesAsync();

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).ToGeometry();
        var requestIds = new RoadNetworkIds([], [roadSegmentNotIntersecting.RoadSegmentId], []);

        // Act
        var ids = await GetUnderlyingIds(sp, requestGeometry, requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2), new(3), new(4)],
            [new(1), new(2)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    private async Task<RoadNetworkIds> GetUnderlyingIds(IServiceProvider sp, Geometry? geometry = null, RoadNetworkIds? ids = null, bool onlyV2 = false)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repo = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return await repo.GetUnderlyingIds(session, geometry ?? Polygon.Empty, ids, onlyV2);
    }
}
