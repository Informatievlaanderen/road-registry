namespace RoadRegistry.Projections.Tests.Projections;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using FluentAssertions;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// The position on the read model's projection-state row is the SQL-side idempotency guard: everything at or below it
// has been applied, so anything at or below it can be skipped when the daemon re-delivers a batch.
//
// A batch can reach past the page it was delivered: the base class pulls a correlation's later events into the batch
// that first mentions it, so the events it applies can sit above events a following page still has to deliver.
// Recording those as the position skipped that window of never-applied events for good.
public class ProjectionPositionTests
{
    [Fact]
    public async Task GivenABatchReachingPastItsPage_ThenTheNextPageIsStillApplied()
    {
        var recorder = new RecordingProjection();
        var projection = CreateProjection(recorder);

        // Page 1 ends at 10, but correlation A also has an event at 30, which the tail fetch pulls in.
        await projection.Dispatch(pageMaxSequence: 10, ("A", [Event(10), Event(30)]));

        // Page 2 delivers what follows page 1 - including 20, which the tail reached past but never applied.
        await projection.Dispatch(pageMaxSequence: 25, ("B", [Event(20)]));

        recorder.AppliedSequences.Should().Equal(10, 30, 20);
    }

    [Fact]
    public async Task GivenABatchReachingPastItsPage_ThenThePositionStaysAtThePage()
    {
        var projection = CreateProjection(new RecordingProjection());

        await projection.Dispatch(pageMaxSequence: 10, ("A", [Event(10), Event(30)]));

        (await projection.Position()).Should().Be(10);
    }

    // What the position is for: the daemon re-delivering a page whose read-model write already committed.
    [Fact]
    public async Task GivenAPageThatIsRedelivered_ThenItsEventsAreNotAppliedTwice()
    {
        var recorder = new RecordingProjection();
        var projection = CreateProjection(recorder);

        await projection.Dispatch(pageMaxSequence: 20, ("A", [Event(10), Event(20)]));
        await projection.Dispatch(pageMaxSequence: 20, ("A", [Event(10), Event(20)]));

        recorder.AppliedSequences.Should().Equal(10, 20);
    }

    [Fact]
    public async Task GivenAPageBelowThePosition_ThenThePositionDoesNotGoBackwards()
    {
        var projection = CreateProjection(new RecordingProjection());

        await projection.Dispatch(pageMaxSequence: 20, ("A", [Event(20)]));
        await projection.Dispatch(pageMaxSequence: 15, ("A", [Event(20)]));

        (await projection.Position()).Should().Be(20);
    }

    [Fact]
    public async Task GivenAPageWithNothingToApply_ThenThePositionStillMovesForward()
    {
        var projection = CreateProjection(new RecordingProjection());

        await projection.Dispatch(pageMaxSequence: 40);

        (await projection.Position()).Should().Be(40);
    }

    private static TestProjection CreateProjection(RecordingProjection recorder)
    {
        return new TestProjection(new TestDbContextFactory(Guid.NewGuid().ToString()), [recorder]);
    }

    private static IEvent Event(long sequence)
    {
        return new Event<TestEvent>(new TestEvent(sequence))
        {
            Sequence = sequence,
            EventTypeName = nameof(TestEvent)
        };
    }

    private sealed record TestEvent(long Sequence);

    private sealed class RecordingProjection : IRoadNetworkChangesProjection<TestDbContext>
    {
        public List<long> AppliedSequences { get; } = [];
        public bool IsCatchingUp { get; set; }
        public ILogger? Logger { get; set; }

        public Task Project(TestDbContext session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken)
        {
            AppliedSequences.AddRange(events.Select(x => x.Sequence));
            return Task.CompletedTask;
        }
    }

    // Reaches the protected dispatch entry point and the CorrelationWorkItem the base builds, so a test can hand the
    // driver the exact batch shape the daemon produces.
    private sealed class TestProjection : DbContextBackedRoadNetworkChangesProjection<TestDbContext>
    {
        private readonly IDbContextFactory<TestDbContext> _dbContextFactory;

        public TestProjection(IDbContextFactory<TestDbContext> dbContextFactory, IReadOnlyCollection<IRoadNetworkChangesProjection<TestDbContext>> projections)
            : base(dbContextFactory, projections, NullLoggerFactory.Instance)
        {
            _dbContextFactory = dbContextFactory;
        }

        public Task Dispatch(long pageMaxSequence, params (string CorrelationId, IEvent[] Events)[] work)
        {
            var correlationWork = work
                .Select(x => new CorrelationWorkItem(
                    x.CorrelationId,
                    $"{nameof(TestProjection)}-{x.CorrelationId}",
                    x.Events.Max(e => e.Sequence),
                    null,
                    x.Events))
                .ToList();

            return DispatchAsync(null!, correlationWork, pageMaxSequence, CancellationToken.None);
        }

        public async Task<long> Position()
        {
            await using var context = await _dbContextFactory.CreateDbContextAsync();
            var state = await context.ProjectionStates.FindAsync(nameof(TestProjection));
            return state!.Position;
        }
    }

    private sealed class TestDbContextFactory : IDbContextFactory<TestDbContext>
    {
        private readonly string _databaseName;

        public TestDbContextFactory(string databaseName)
        {
            _databaseName = databaseName;
        }

        public TestDbContext CreateDbContext()
        {
            return new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(_databaseName)
                .Options);
        }
    }

    public class TestDbContext : RunnerDbContext<TestDbContext>
    {
        public override string ProjectionStateSchema => "test";

        public TestDbContext(DbContextOptions<TestDbContext> options)
            : base(options)
        {
        }
    }
}
