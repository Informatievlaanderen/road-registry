namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV1Inwinning;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Projections.Tests.Projections.WmsWfsV2;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadRegistry.WmsWfsV1Inwinning;
using RoadRegistry.WmsWfsV1Inwinning.Projections;
using RoadRegistry.WmsWfsV1Inwinning.Records;
using RoadNodeV1 = RoadRegistry.RoadNode.Events.V1;

public class WmsWfsV1InwinningProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();
    private readonly Scenario _scenario = new();

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // Only the events that make a road segment or road node V2 as part of an inwinning; everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadNode V1
            typeof(RoadNodeV1.ImportedRoadNode), typeof(RoadNodeV1.RoadNodeAdded), typeof(RoadNodeV1.RoadNodeModified), typeof(RoadNodeV1.RoadNodeRemoved),
            // RoadNode V2
            typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified), typeof(RoadNodeWasRemoved),
            // RoadSegment V1
            typeof(ImportedRoadSegment), typeof(OutlinedRoadSegmentRemoved), typeof(RoadSegmentAdded),
            typeof(RoadSegmentAddedToEuropeanRoad), typeof(RoadSegmentAddedToNationalRoad), typeof(RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentAttributesModified), typeof(RoadSegmentGeometryModified), typeof(RoadSegmentModified),
            typeof(RoadSegmentRemoved), typeof(RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentStreetNamesChanged),
            // RoadSegment V2
            typeof(OutlinedRoadSegmentWasAdded), typeof(RoadSegmentGeometryWasModified), typeof(RoadSegmentStreetNameIdWasChanged), typeof(RoadSegmentGeometryDrawMethodWasChanged),
            typeof(RoadSegmentWasAdded), typeof(RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentWasMerged), typeof(RoadSegmentWasModified),
            typeof(RoadSegmentWasRealizedFromPlanned),
            typeof(RoadSegmentWasRealizedFromOutOfUse),
            typeof(RoadSegmentWasCorrectedFromHistorizedToRealized),
            typeof(RoadSegmentWasCorrectedFromRealizedToPlanned),
            typeof(RoadSegmentWasTakenOutOfUseFromRealized),
            typeof(RoadSegmentWasHistorizedFromRealized),
            typeof(RoadSegmentWasHistorizedFromOutOfUse),
            typeof(RoadSegmentWasCorrectedFromNotRealizedToPlanned),
            typeof(RoadSegmentWasCorrectedFromHistorizedToOutOfUse),
            typeof(RoadSegmentWasNotRealizedFromPlanned),
            typeof(RoadSegmentWasRemoved), typeof(RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadSegmentWasRetiredBecauseOfSplit), typeof(RoadSegmentWasSplit),
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
            typeof(GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange), typeof(GradeJunctionWasChangedToGradeSeparatedJunction),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            typeof(GradeSeparatedJunctionGeometryModified),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionGeometryWasChanged),
            typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            typeof(GradeSeparatedJunctionWasAddedBecauseOfGradeJunctionChange), typeof(GradeSeparatedJunctionWasChangedToGradeJunction),
            // Organization V2
            typeof(OrganizationWasImported), typeof(OrganizationWasCreated),
            typeof(OrganizationWasModified), typeof(OrganizationWasRemoved),
            // StreetName V2
            typeof(StreetNameWasCreated), typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted),
            typeof(RoadNetworkWasChanged)
        };

        WmsWfsV2ProjectionEventCoverage.AssertHandledExactlyOnce(new WmsWfsV1InwinningProjection(), excludeEventTypes);
    }

    [Fact]
    public async Task WhenRoadSegmentWasMigrated_ThenCompletedAndItsV1RecordsMarkedV2()
    {
        await GivenV1RoadSegment(42, europeanRoadIds: [1, 2], nationalRoadIds: [3]);
        await GivenV1RoadSegment(43, europeanRoadIds: [4], nationalRoadIds: [5]);

        await _scenario.GivenAsync(_testData.Fixture.Create<RoadSegmentWasMigrated>() with { RoadSegmentId = new RoadSegmentId(42) });

        Assert.NotNull(await _scenario.Find<CompletedRoadSegmentRecord>(42));
        await AssertV1RoadSegmentIsV2(42, true);
        await AssertV1RoadSegmentIsV2(43, false);
    }

    [Fact]
    public async Task WhenRoadSegmentWasRemovedBecauseOfMigration_ThenCompletedAndItsV1RecordsMarkedV2()
    {
        await GivenV1RoadSegment(42, europeanRoadIds: [1], nationalRoadIds: [2]);

        await _scenario.GivenAsync(_testData.Fixture.Create<RoadSegmentWasRemovedBecauseOfMigration>() with { RoadSegmentId = new RoadSegmentId(42) });

        Assert.NotNull(await _scenario.Find<CompletedRoadSegmentRecord>(42));
        await AssertV1RoadSegmentIsV2(42, true);
    }

    [Fact]
    public async Task WhenRoadNodeWasMigrated_ThenCompletedAndItsV1RecordMarkedV2()
    {
        await GivenV1RoadNodes(7, 8);

        await _scenario.GivenAsync(_testData.Fixture.Create<RoadNodeWasMigrated>() with { RoadNodeId = new RoadNodeId(7) });

        Assert.NotNull(await _scenario.Find<CompletedRoadNodeRecord>(7));
        Assert.True((await _scenario.Find<V1WfsRoadNodeRecord>(7))!.IsV2);
        Assert.False((await _scenario.Find<V1WfsRoadNodeRecord>(8))!.IsV2);
    }

    [Fact]
    public async Task WhenRoadNodeWasRemovedBecauseOfMigration_ThenCompletedAndItsV1RecordMarkedV2()
    {
        await GivenV1RoadNodes(7);

        await _scenario.GivenAsync(_testData.Fixture.Create<RoadNodeWasRemovedBecauseOfMigration>() with { RoadNodeId = new RoadNodeId(7) });

        Assert.NotNull(await _scenario.Find<CompletedRoadNodeRecord>(7));
        Assert.True((await _scenario.Find<V1WfsRoadNodeRecord>(7))!.IsV2);
    }

    [Fact]
    public async Task WhenThereAreNoV1Records_ThenOnlyCompleted()
    {
        await _scenario.GivenAsync(
            _testData.Fixture.Create<RoadSegmentWasMigrated>() with { RoadSegmentId = new RoadSegmentId(42) },
            _testData.Fixture.Create<RoadNodeWasMigrated>() with { RoadNodeId = new RoadNodeId(7) });

        Assert.NotNull(await _scenario.Find<CompletedRoadSegmentRecord>(42));
        Assert.NotNull(await _scenario.Find<CompletedRoadNodeRecord>(7));
        Assert.Empty(await _scenario.Query<V1WmsRoadSegmentRecord>());
        Assert.Empty(await _scenario.Query<V1WfsRoadNodeRecord>());
    }

    [Fact]
    public async Task WhenCompletedByMoreThanOneEvent_ThenRecordedOnce()
    {
        await GivenV1RoadSegment(42, europeanRoadIds: [1], nationalRoadIds: []);

        // Twice within one batch, and once more in a later batch.
        await _scenario.GivenAsync(
            _testData.Fixture.Create<RoadSegmentWasMigrated>() with { RoadSegmentId = new RoadSegmentId(42) },
            _testData.Fixture.Create<RoadSegmentWasRemovedBecauseOfMigration>() with { RoadSegmentId = new RoadSegmentId(42) },
            _testData.Fixture.Create<RoadNodeWasMigrated>() with { RoadNodeId = new RoadNodeId(7) },
            _testData.Fixture.Create<RoadNodeWasRemovedBecauseOfMigration>() with { RoadNodeId = new RoadNodeId(7) });
        await _scenario.GivenAsync(_testData.Fixture.Create<RoadSegmentWasMigrated>() with { RoadSegmentId = new RoadSegmentId(42) });

        Assert.Equal([42], (await _scenario.Query<CompletedRoadSegmentRecord>()).Select(x => x.WS_OIDN));
        Assert.Equal([7], (await _scenario.Query<CompletedRoadNodeRecord>()).Select(x => x.WK_OIDN));
        await AssertV1RoadSegmentIsV2(42, true);
    }

    private Task GivenV1RoadSegment(int roadSegmentId, int[] europeanRoadIds, int[] nationalRoadIds)
    {
        return _scenario.SeedAsync(context =>
        {
            context.V1WmsRoadSegments.Add(new V1WmsRoadSegmentRecord { Id = roadSegmentId });
            context.V1WfsRoadSegments.Add(new V1WfsRoadSegmentRecord { Id = roadSegmentId });
            context.V1WmsEuropeanRoads.AddRange(europeanRoadIds.Select(id => new V1WmsEuropeanRoadRecord { EU_OIDN = id, WS_OIDN = roadSegmentId }));
            context.V1WmsNationalRoads.AddRange(nationalRoadIds.Select(id => new V1WmsNationalRoadRecord { NW_OIDN = id, WS_OIDN = roadSegmentId }));
            return Task.CompletedTask;
        });
    }

    private Task GivenV1RoadNodes(params int[] roadNodeIds)
    {
        return _scenario.SeedAsync(context =>
        {
            context.V1WfsRoadNodes.AddRange(roadNodeIds.Select(id => new V1WfsRoadNodeRecord { Id = id }));
            return Task.CompletedTask;
        });
    }

    private async Task AssertV1RoadSegmentIsV2(int roadSegmentId, bool isV2)
    {
        Assert.Equal(isV2, (await _scenario.Find<V1WmsRoadSegmentRecord>(roadSegmentId))!.IsV2);
        Assert.Equal(isV2, (await _scenario.Find<V1WfsRoadSegmentRecord>(roadSegmentId))!.IsV2);
        Assert.All(await _scenario.Query<V1WmsEuropeanRoadRecord>(q => q.Where(x => x.WS_OIDN == roadSegmentId)), x => Assert.Equal(isV2, x.IsV2));
        Assert.All(await _scenario.Query<V1WmsNationalRoadRecord>(q => q.Where(x => x.WS_OIDN == roadSegmentId)), x => Assert.Equal(isV2, x.IsV2));
    }

    // Mirrors DbContextBackedRoadNetworkChangesProjection against an in-memory WmsWfsV1InwinningContext, as
    // WmsWfsV2ProjectionScenario does for the WmsWfsV2 read model: one context per batch, the projection-state position
    // guarding re-delivery, changes detected and saved once.
    private sealed class Scenario
    {
        private const string ProjectionStateName = nameof(RoadNetworkChangesWmsWfsV1InwinningProjection);

        private readonly InMemoryDatabaseRoot _root = new();
        private readonly string _databaseName = Guid.NewGuid().ToString("N");
        private readonly WmsWfsV1InwinningProjection _projection = new();
        private long _position;

        public async Task GivenAsync(params object[] messages)
        {
            var events = messages.Select(BuildEvent).ToList();

            await using var context = CreateDbContext();
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            var projectionState = await context.ProjectionStates.FindAsync(ProjectionStateName);
            if (projectionState is null)
            {
                projectionState = new ProjectionStateItem { Name = ProjectionStateName };
                await context.ProjectionStates.AddAsync(projectionState);
            }

            foreach (var @event in events.Where(x => x.Sequence > projectionState.Position))
            {
                await _projection.Project(context, [@event], CancellationToken.None);
                projectionState.Position = @event.Sequence;
            }

            context.ChangeTracker.DetectChanges();
            await context.SaveChangesAsync();
        }

        public async Task SeedAsync(Func<WmsWfsV1InwinningContext, Task> seed)
        {
            await using var context = CreateDbContext();
            await seed(context);
            await context.SaveChangesAsync();
        }

        public async Task<T?> Find<T>(params object[] keyValues) where T : class
        {
            await using var context = CreateDbContext();
            return await context.Set<T>().FindAsync(keyValues);
        }

        public async Task<List<T>> Query<T>(Func<IQueryable<T>, IQueryable<T>>? filter = null) where T : class
        {
            await using var context = CreateDbContext();
            IQueryable<T> query = context.Set<T>().AsNoTracking();
            return await (filter is null ? query : filter(query)).ToListAsync();
        }

        private WmsWfsV1InwinningContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<WmsWfsV1InwinningContext>()
                .UseInMemoryDatabase(_databaseName, _root)
                .ConfigureWarnings(w => w.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning))
                .Options;
            return new WmsWfsV1InwinningContext(options);
        }

        private IEvent BuildEvent(object message)
        {
            var evt = (IEvent)Activator.CreateInstance(typeof(Event<>).MakeGenericType(message.GetType()), message)!;
            _position++;
            evt.Version = _position;
            evt.Sequence = _position;
            return evt;
        }
    }
}
