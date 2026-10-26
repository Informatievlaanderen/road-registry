namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkRepositoryTests;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction;
using RoadRegistry.Infrastructure.MartenDb;
using RoadSegment;
using ScopedRoadNetwork;
using Xunit.Abstractions;

[Collection(nameof(DockerFixtureCollection))]
public class WhenGetUnderlyingIdsForExtract : RoadNetworkIntegrationTest
{
    public WhenGetUnderlyingIdsForExtract(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Fact]
    public async Task WithNoGeometry_ThenException()
    {
        // Arrange
        var sp = await BuildServiceProvider();

        // Act
        var act = () => GetUnderlyingIds(sp, null);

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
                EndNodeId = new RoadNodeId(2),
                // The fixture picks a status at random, and only a gepland or gerealiseerd road segment is part of
                // an extract.
                Status = RoadSegmentStatusV2.Gerealiseerd
            };
            var roadSegmentNotIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100),
                Status = RoadSegmentStatusV2.Gerealiseerd
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

        var requestGeometry = BuildRoadSegmentGeometry(5, -5, 5, 5).Value.Buffer(1).Envelope;

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
                EndNodeId = new RoadNodeId(2),
                // The fixture picks a status at random, and only a gepland or gerealiseerd road segment is part of
                // an extract.
                Status = RoadSegmentStatusV2.Gerealiseerd
            };
            var roadSegmentNotIntersecting = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
            {
                RoadSegmentId = new RoadSegmentId(2),
                Geometry = BuildRoadSegmentGeometry(0, 100, 10, 100),
                Status = RoadSegmentStatusV2.Gerealiseerd
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

    // What an extract is made of. The topology keeps every road segment whatever its status, removed ones included,
    // because an action that is handed a road network has to see them; an extract is the road network as it is on
    // the ground and as it can be handed back, so it takes the gepland and gerealiseerd road segments only.
    [Fact]
    public async Task GivenRoadSegmentsInEveryStatus_ThenOnlyGeplandAndGerealiseerd()
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadSegment(store, 1, RoadSegmentStatusV2.Gerealiseerd);
        await AddRoadSegment(store, 2, RoadSegmentStatusV2.Gepland);

        await AddRoadSegment(store, 3, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 3, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasHistorizedFromRealized>() with
        {
            RoadSegmentId = new RoadSegmentId(3)
        });

        await AddRoadSegment(store, 4, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 4, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasTakenOutOfUseFromRealized>() with
        {
            RoadSegmentId = new RoadSegmentId(4)
        });

        await AddRoadSegment(store, 5, RoadSegmentStatusV2.Gepland);
        await Append(store, 5, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasNotRealizedFromPlanned>() with
        {
            RoadSegmentId = new RoadSegmentId(5)
        });

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1), new RoadSegmentId(2)]);
    }

    // The staging sequence, end to end: realized, historized by a merger, then removed. Neither the retire nor the
    // removal takes the row out of the topology, and neither one belongs in an extract.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GivenARetiredRoadSegment_ThenNotInTheExtract(bool alsoRemoved)
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadSegment(store, 1, RoadSegmentStatusV2.Gerealiseerd);

        await AddRoadSegment(store, 2, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 2, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMerger>() with
        {
            RoadSegmentId = new RoadSegmentId(2),
            MergedRoadSegmentId = new RoadSegmentId(1)
        });

        if (alsoRemoved)
        {
            await Append(store, 2, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved>() with
            {
                RoadSegmentId = new RoadSegmentId(2)
            });
        }

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
    }

    [Fact]
    public async Task GivenARemovedRoadSegment_ThenNotInTheExtract()
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadSegment(store, 1, RoadSegmentStatusV2.Gerealiseerd);

        await AddRoadSegment(store, 2, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 2, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved>() with
        {
            RoadSegmentId = new RoadSegmentId(2)
        });

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
    }

    // A V1 road segment has no V2 status and is taken into an extract whatever state it is in; a V1 removal deletes
    // the topology row, so a removed one is not there to begin with.
    [Fact]
    public async Task GivenAV1RoadSegment_ThenInTheExtractWhateverItsStatus()
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        var v1RoadSegment = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment>();
        v1RoadSegment.RoadSegmentId = 1;
        v1RoadSegment.StartNodeId = 11;
        v1RoadSegment.EndNodeId = 12;
        v1RoadSegment.Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0);
        v1RoadSegment.Status = RoadSegmentStatus.OutOfUse;

        await Append(store, 1, v1RoadSegment);

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
    }

    private Task AddRoadSegment(IDocumentStore store, int roadSegmentId, RoadSegmentStatusV2 status)
    {
        return Append(store, roadSegmentId, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
            StartNodeId = new RoadNodeId(roadSegmentId * 10 + 1),
            EndNodeId = new RoadNodeId(roadSegmentId * 10 + 2),
            Status = status
        });
    }

    // One session per event: the topology functions compare event timestamps, and events saved together share one.
    private static async Task Append(IDocumentStore store, int roadSegmentId, object @event)
    {
        await using var session = store.LightweightSession();
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), new RoadSegmentId(roadSegmentId)), @event);
        await session.SaveChangesAsync();
    }

    private static Geometry RequestGeometry()
    {
        return BuildRoadSegmentGeometry(5, -5, 5, 5).Value.Buffer(1).Envelope;
    }

    private async Task<RoadNetworkIds> GetUnderlyingIds(IServiceProvider sp, Geometry? geometry)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repo = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return await repo.GetUnderlyingIdsForExtract(session, geometry);
    }
}
