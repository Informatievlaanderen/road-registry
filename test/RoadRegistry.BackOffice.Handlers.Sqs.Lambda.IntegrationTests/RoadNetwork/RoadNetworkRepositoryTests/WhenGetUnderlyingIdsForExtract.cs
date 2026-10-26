namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkRepositoryTests;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
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

            AppendRoadNodes(session, 1, 2);
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

            AppendRoadNodes(session, 1, 2);
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
    //
    // The last two are the sequence that got the extract projection stuck: a road segment retired by a merger, and
    // one that was removed after that. Neither belongs in an extract, and neither leaves the topology.
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

        await AddRoadSegment(store, 6, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 6, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMerger>() with
        {
            RoadSegmentId = new RoadSegmentId(6),
            MergedRoadSegmentId = new RoadSegmentId(1)
        });

        await AddRoadSegment(store, 7, RoadSegmentStatusV2.Gerealiseerd);
        await Append(store, 7, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMerger>() with
        {
            RoadSegmentId = new RoadSegmentId(7),
            MergedRoadSegmentId = new RoadSegmentId(1)
        });
        await Append(store, 7, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved>() with
        {
            RoadSegmentId = new RoadSegmentId(7)
        });

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1), new RoadSegmentId(2)]);
    }

    // Nothing that is marked removed lands in an extract, whatever kind of entity it is. The topology keeps the rows
    // - see RoadNetworkTopologyProjection-EventsV2 - so every kind is filtered on its own mark.
    [Fact]
    public async Task GivenRemovedEntities_ThenNoneOfThemInTheExtract()
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadNode(store, 11);
        await AddRoadNode(store, 12);
        await AddRoadSegment(store, 1, RoadSegmentStatusV2.Gerealiseerd);

        await AddRoadNode(store, 21);
        await AddRoadNode(store, 22);
        await AddRoadSegment(store, 2, RoadSegmentStatusV2.Gerealiseerd);

        await AddGradeSeparatedJunction(store, 1);
        await AddGradeSeparatedJunction(store, 2);
        await AddGradeJunction(store, 1);
        await AddGradeJunction(store, 2);

        await Append(store, 2, TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved>() with
        {
            RoadSegmentId = new RoadSegmentId(2)
        });
        await Append(store, typeof(RoadRegistry.RoadNode.RoadNode), 12, TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasRemoved>() with
        {
            RoadNodeId = new RoadNodeId(12)
        });
        await Append(store, typeof(GradeSeparatedJunction), 2, TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasRemoved>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(2)
        });
        await Append(store, typeof(GradeJunction), 2, TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasRemoved>() with
        {
            GradeJunctionId = new GradeJunctionId(2)
        });

        // Act
        var ids = await GetUnderlyingIds(sp, RequestGeometry());

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
        ids.RoadNodeIds.Should().BeEquivalentTo([new RoadNodeId(11)]);
        ids.GradeSeparatedJunctionIds.Should().BeEquivalentTo([new GradeSeparatedJunctionId(1)]);
        ids.GradeJunctionIds.Should().BeEquivalentTo([new GradeJunctionId(1)]);
    }

    // A V1 road segment has no V2 status and is taken into an extract whatever state it is in; a V1 removal deletes
    // the topology row, so a removed one is not there to begin with.
    [Fact]
    public async Task GivenAV1RoadSegment_ThenInTheExtractWhateverItsStatus()
    {
        // Arrange
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        var geometry = BuildRoadSegmentGeometry(0, 0, 10, 0);

        var v1RoadSegment = TestData.Fixture.Create<RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment>();
        v1RoadSegment.RoadSegmentId = 1;
        v1RoadSegment.StartNodeId = 11;
        v1RoadSegment.EndNodeId = 12;
        v1RoadSegment.Geometry = geometry;
        v1RoadSegment.Status = RoadSegmentStatus.OutOfUse;

        await Append(store, 1, v1RoadSegment);

        // A V1 road segment is stored in Lambert08 - the projection converts it - while the rest of this file works
        // in the Lambert72 the test data is built in, so the contour has to be asked for in the same coordinates.
        var contour = geometry.EnsureLambert08().Value.Buffer(1).Envelope;

        // Act
        var ids = await GetUnderlyingIds(sp, contour);

        // Assert
        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
    }

    private Task AddRoadNode(IDocumentStore store, int roadNodeId)
    {
        return Append(store, typeof(RoadRegistry.RoadNode.RoadNode), roadNodeId, TestData.Fixture.Create<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>() with
        {
            RoadNodeId = new RoadNodeId(roadNodeId)
        });
    }

    private Task AddGradeSeparatedJunction(IDocumentStore store, int gradeSeparatedJunctionId)
    {
        return Append(store, typeof(GradeSeparatedJunction), gradeSeparatedJunctionId, TestData.Fixture.Create<RoadRegistry.GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded>() with
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(gradeSeparatedJunctionId),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(1)
        });
    }

    private Task AddGradeJunction(IDocumentStore store, int gradeJunctionId)
    {
        return Append(store, typeof(GradeJunction), gradeJunctionId, TestData.Fixture.Create<RoadRegistry.GradeJunction.Events.V2.GradeJunctionWasAdded>() with
        {
            GradeJunctionId = new GradeJunctionId(gradeJunctionId),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(1)
        });
    }

    private static void AppendRoadNodes(IDocumentSession session, params int[] roadNodeIds)
    {
        foreach (var roadNodeId in roadNodeIds)
        {
            session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadRegistry.RoadNode.RoadNode), new RoadNodeId(roadNodeId)),
                new RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded
                {
                    RoadNodeId = new RoadNodeId(roadNodeId),
                    Geometry = RoadNodeGeometry.Create(new Point(roadNodeId, 0) { SRID = WellknownSrids.Lambert72 }),
                    Type = RoadNodeTypeV2.EchteKnoop,
                    Grensknoop = false,
                    Provenance = new RoadRegistryProvenanceData()
                });
        }
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
    private static Task Append(IDocumentStore store, int roadSegmentId, object @event)
    {
        return Append(store, typeof(RoadSegment), roadSegmentId, @event);
    }

    private static async Task Append(IDocumentStore store, Type entityType, int id, object @event)
    {
        await using var session = store.LightweightSession();
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(entityType, id), @event);
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
