namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkRepositoryTests;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadNode;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction;
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
            var gradeSeparatedJunctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId
            };
            var gradeJunctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegmentIntersecting.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentIntersecting.RoadSegmentId), roadSegmentIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentNotIntersecting.RoadSegmentId), roadSegmentNotIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionIntersecting.GradeSeparatedJunctionId), gradeSeparatedJunctionIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunctionIntersecting.GradeJunctionId), gradeJunctionIntersecting);

            await session.SaveChangesAsync();
        }

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).Value;

        // Act
        var ids = await GetUnderlyingIds(sp, requestGeometry);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task GivenJunctions_WithGeometryAndJunctionIsPartiallyOutside_ThenOnlyRetrieveSegmentsInsideGeometry()
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
            var gradeSeparatedJunctionCompleteInside = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId,
                UpperRoadSegmentId = roadSegmentIntersecting.RoadSegmentId
            };
            var gradeSeparatedJunctionPartiallyInside = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(2),
                LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId,
                UpperRoadSegmentId = roadSegmentNotIntersecting.RoadSegmentId
            };
            var gradeSeparatedJunctionCompleteOutside = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(3),
                LowerRoadSegmentId = roadSegmentNotIntersecting.RoadSegmentId,
                UpperRoadSegmentId = roadSegmentNotIntersecting.RoadSegmentId
            };
            var gradeJunctionCompleteInside = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegmentIntersecting.RoadSegmentId,
                RoadSegmentId2 = roadSegmentIntersecting.RoadSegmentId
            };
            var gradeJunctionPartiallyInside = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(2),
                RoadSegmentId1 = roadSegmentIntersecting.RoadSegmentId,
                RoadSegmentId2 = roadSegmentNotIntersecting.RoadSegmentId
            };
            var gradeJunctionCompleteOutside = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(3),
                RoadSegmentId1 = roadSegmentNotIntersecting.RoadSegmentId,
                RoadSegmentId2 = roadSegmentNotIntersecting.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentIntersecting.RoadSegmentId), roadSegmentIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentNotIntersecting.RoadSegmentId), roadSegmentNotIntersecting);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionCompleteInside.GradeSeparatedJunctionId), gradeSeparatedJunctionCompleteInside);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionPartiallyInside.GradeSeparatedJunctionId), gradeSeparatedJunctionPartiallyInside);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionCompleteOutside.GradeSeparatedJunctionId), gradeSeparatedJunctionCompleteOutside);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunctionCompleteInside.GradeJunctionId), gradeJunctionCompleteInside);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunctionPartiallyInside.GradeJunctionId), gradeJunctionPartiallyInside);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunctionCompleteOutside.GradeJunctionId), gradeJunctionCompleteOutside);

            await session.SaveChangesAsync();
        }

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).Value;

        // Act
        var ids = await GetUnderlyingIds(sp, requestGeometry);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1), new(2)],
            [new(1), new (2)]);
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
            var gradeSeparatedJunction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };
            var gradeJunction = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunction.GradeSeparatedJunctionId), gradeSeparatedJunction);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunction.GradeJunctionId), gradeJunction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [new(1)],
            [],
            []
            ,[]);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
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
            var gradeSeparatedJunction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };
            var gradeJunction = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunction.GradeSeparatedJunctionId), gradeSeparatedJunction);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunction.GradeJunctionId), gradeJunction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [],
            [new(1)],
            []
            ,[]);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
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
            var gradeSeparatedJunction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };
            var gradeJunction = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunction.GradeSeparatedJunctionId), gradeSeparatedJunction);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunction.GradeJunctionId), gradeJunction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [],
            [],
            [new(1)]
            ,[]);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    [Fact]
    public async Task WithGradeJunctionIds_ThenResult()
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
            var gradeSeparatedJunction = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
            {
                GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
                LowerRoadSegmentId = roadSegment1.RoadSegmentId
            };
            var gradeJunction = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
            {
                GradeJunctionId = new GradeJunctionId(1),
                RoadSegmentId1 = roadSegment1.RoadSegmentId
            };

            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment1.RoadSegmentId), roadSegment1);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegment2.RoadSegmentId), roadSegment2);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunction.GradeSeparatedJunctionId), gradeSeparatedJunction);
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunction.GradeJunctionId), gradeJunction);

            await session.SaveChangesAsync();
        }

        var requestIds = new RoadNetworkIds(
            [],
            [],
            [],
            [new(1)]);

        // Act
        var ids = await GetUnderlyingIds(sp, ids: requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2)],
            [new(1)],
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
        var gradeSeparatedJunctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = roadSegmentIntersecting.RoadSegmentId
        };
        var gradeJunctionIntersecting = TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = roadSegmentIntersecting.RoadSegmentId
        };

        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentIntersecting.RoadSegmentId), roadSegmentIntersecting);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), roadSegmentNotIntersecting.RoadSegmentId), roadSegmentNotIntersecting);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeSeparatedJunction), gradeSeparatedJunctionIntersecting.GradeSeparatedJunctionId), gradeSeparatedJunctionIntersecting);
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(GradeJunction), gradeJunctionIntersecting.GradeJunctionId), gradeJunctionIntersecting);

        await session.SaveChangesAsync();

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).Value;
        var requestIds = new RoadNetworkIds(
            [],
            [roadSegmentNotIntersecting.RoadSegmentId],
            [],
            []);

        // Act
        var ids = await GetUnderlyingIds(sp, requestGeometry, requestIds);

        // Assert
        var expectedIds = new RoadNetworkIds(
            [new(1), new(2), new(3), new(4)],
            [new(1), new(2)],
            [new(1)],
            [new(1)]);
        ids.Should().BeEquivalentTo(expectedIds);
    }

    private async Task<RoadNetworkIds> GetUnderlyingIds(IServiceProvider sp, Geometry? geometry = null, RoadNetworkIds? ids = null)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repo = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return await repo.GetUnderlyingIds(session, geometry, ids);
    }
}
