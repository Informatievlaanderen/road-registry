namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesMartenProjection;

using System.Collections.Concurrent;
using AutoFixture;
using BackOffice;
using FluentAssertions;
using Infrastructure;
using JasperFx.Events;
using Marten;
using Marten.Services;
using Npgsql;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment.Events.V2;
using Tests.AggregateTests;

// seq_id is handed out by nextval at INSERT time, inside the transaction, and the row stays invisible until that
// transaction commits. So within one correlation the order the sequence was taken in is not the order the events
// become readable: a slow append can sit on a lower seq_id than one that started later and committed first.
//
// The tail fetch reads that correlation's later events into the current batch, and ProcessEvents then records the
// highest sequence it saw as the correlation's watermark. Read without a ceiling it therefore writes a watermark over
// events it never saw, and `Sequence > LastSequenceId` drops them when they finally arrive - once, silently, for good.
// That is how 2004 road segment additions were lost on 2026-10-08.
//
// This test holds the three events apart in exactly that way, with a real transaction left open across the first
// batch, and asks for the only thing that matters afterwards: every event reached the sub-projection.
[Collection(nameof(DockerFixtureCollection))]
public class RoadNetworkChangesProjectionTailFetchTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly IFixture _fixture = new RoadNetworkTestDataV2().Fixture;

    public RoadNetworkChangesProjectionTailFetchTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task AnEventThatCommitsAfterItsCorrelationHasBeenBatchedIsStillApplied()
    {
        var recorder = new AppliedRoadSegmentsProjection();
        var projection = new DummyRoadNetworkChangesProjection([recorder]);

        var settled = _fixture.Create<RoadSegmentWasAdded>();
        var inFlight = _fixture.Create<RoadSegmentWasAdded>();
        var overtaking = _fixture.Create<RoadSegmentWasAdded>();

        await new MartenProjectionIntegrationTestRunner(_databaseFixture)
            .ConfigureRoadNetworkChangesProjection(projection)
            .Run(async (_, store) =>
            {
                var correlationId = Guid.NewGuid().ToString();

                // Committed and readable. This is what the daemon can see, and so the only thing its high water mark
                // may cover.
                await Append(store, correlationId, settled);
                var settledSequence = await MaxSequence(store, correlationId);

                // Appended, not committed: nextval has already handed out its seq_id, so it occupies a number between
                // the other two while being invisible to every reader. Held open across the first batch below.
                await using var connection = new NpgsqlConnection(_databaseFixture.ConnectionString);
                await connection.OpenAsync();
                await using var inFlightTransaction = await connection.BeginTransactionAsync();
                await using var inFlightSession = store.LightweightSession(SessionOptions.ForTransaction(inFlightTransaction));
                inFlightSession.CorrelationId = correlationId;
                inFlightSession.Events.StartStream(StreamKeyFor(inFlight), inFlight);
                await inFlightSession.SaveChangesAsync();

                // Same correlation, a higher seq_id than the one still in flight, and committed before it. This is the
                // event the tail fetch can reach past the gap for.
                await Append(store, correlationId, overtaking);

                // The mark sits at the last settled event: the sequence is contiguous up to there and gapped after it,
                // which is precisely the state Marten's detector would report while the append is in flight.
                await SetHighWaterMark(settledSequence);
                await ApplyBatch(store, projection, correlationId, fromExclusive: 0, toInclusive: settledSequence);

                await inFlightTransaction.CommitAsync();

                // Both remaining events are readable now, and the daemon delivers them in the next page.
                var tailSequence = await MaxSequence(store, correlationId);
                await SetHighWaterMark(tailSequence);
                await ApplyBatch(store, projection, correlationId, fromExclusive: settledSequence, toInclusive: tailSequence);
            });

        recorder.Applied.Should().BeEquivalentTo(
            new[] { settled.RoadSegmentId.ToInt32(), inFlight.RoadSegmentId.ToInt32(), overtaking.RoadSegmentId.ToInt32() },
            "an event the daemon delivers once may not be filtered out by a watermark written before it existed");
    }

    private static async Task ApplyBatch(
        IDocumentStore store,
        RoadNetworkChangesProjection projection,
        string correlationId,
        long fromExclusive,
        long toInclusive)
    {
        await using var session = store.LightweightSession();

        var page = await session.Events.QueryAllRawEvents()
            .Where(x => x.CorrelationId == correlationId && x.Sequence > fromExclusive && x.Sequence <= toInclusive)
            .ToListAsync();

        await projection.ApplyAsync(session, page, CancellationToken.None);
        await session.SaveChangesAsync();
    }

    private async Task SetHighWaterMark(long sequence)
    {
        await using var connection = new NpgsqlConnection(_databaseFixture.ConnectionString);
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText =
            $"select {WellKnownSchemas.MartenEventStore}.mt_mark_event_progression('{MartenConstants.HighWaterMarkName}', @sequence)";
        command.Parameters.AddWithValue("sequence", sequence);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task Append(IDocumentStore store, string correlationId, RoadSegmentWasAdded @event)
    {
        await using var session = store.LightweightSession();

        session.CorrelationId = correlationId;
        session.Events.StartStream(StreamKeyFor(@event), @event);

        await session.SaveChangesAsync();
    }

    private static async Task<long> MaxSequence(IDocumentStore store, string correlationId)
    {
        await using var session = store.LightweightSession();

        var events = await session.Events.QueryAllRawEvents()
            .Where(x => x.CorrelationId == correlationId)
            .ToListAsync();

        return events.Max(x => x.Sequence);
    }

    private static string StreamKeyFor(RoadSegmentWasAdded @event)
    {
        return StreamKeyFactory.Create(typeof(RoadRegistry.RoadSegment.RoadSegment), @event.RoadSegmentId);
    }

    // Writes nothing; it only records which road segments reached a sub-projection.
    private sealed class AppliedRoadSegmentsProjection : MartenRoadNetworkChangesProjection
    {
        private readonly ConcurrentQueue<int> _applied = new();

        public int[] Applied => _applied.ToArray();

        public AppliedRoadSegmentsProjection()
        {
            When<IEvent<RoadSegmentWasAdded>>((_, e, _) =>
            {
                _applied.Enqueue(e.Data.RoadSegmentId.ToInt32());
                return Task.CompletedTask;
            });
        }
    }
}
