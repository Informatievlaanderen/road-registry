namespace RoadRegistry.Projections.Tests.Projections.MartenMigration;

using AutoFixture;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Extensions;
using Infrastructure.MartenDb.Setup;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.MartenMigration.Projections;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class MartenMigrationProjectionTests
{
    private readonly Fixture _fixture;

    public MartenMigrationProjectionTests()
    {
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public Task WhenEventIsProjectedMoreThanOnce_ThenOnlyOnceProcessed()
    {
        // e.g. when projection crashes after marten data is saved

        var @event = _fixture.Create<RoadNodeAdded>();
        @event.OriginalId = 1;
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(@event);

        var envelope = new Envelope(message, new Dictionary<string, object>
        {
            { Envelope.PositionMetadataKey, 0L },
            { Envelope.StreamIdMetadataKey, RoadNetworkStreamNameProvider.Default.ToString() },
            { Envelope.CreatedUtcMetadataKey, Moment.EnvelopeCreatedUtc.ToUniversalTime() },
            { Envelope.EventNameMetadataKey, message.GetType().Name },
        });

        return BuildProjection()
            .Scenario()
            .Given(envelope, envelope)
            .Expect(new RoadRegistry.RoadNode.Events.V1.RoadNodeAdded
                {
                    RoadNodeId = @event.Id,
                    TemporaryId = @event.TemporaryId,
                    OriginalId = @event.OriginalId,
                    Version = @event.Version,
                    Geometry = GeometryTranslator.Translate(@event.Geometry).ToRoadNodeGeometry(),
                    Type = @event.Type,
                    Provenance = new ProvenanceData(new Provenance(
                        LocalDateTimeTranslator.TranslateFromWhen(message.When),
                        Application.RoadRegistry,
                        new Reason(message.Reason),
                        new Operator(message.OrganizationId),
                        Modification.Insert,
                        Organisation.DigitaalVlaanderen))
                }
            );
    }

    // RemoveRoadNode applied RoadNodeWasRemoved to the aggregate and never handed it back to the session, so the
    // snapshot went on saying isRemoved: false while the stream said otherwise. Its road segment counterpart does
    // call session.Store, which is why only road nodes drifted - 206.552 of them on tst. Nothing was wrong with the
    // event it appended, so no assertion on appended events could have caught it.
    [Fact]
    public Task WhenRoadNodeIsRemoved_ThenTheSnapshotSaysSoToo()
    {
        var added = _fixture.Create<RoadNodeAdded>();
        added.OriginalId = 1;

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(added, new RoadNodeRemoved { Id = added.Id });

        return BuildProjection()
            .Scenario()
            .Given(message)
            .ExpectRecords(records =>
            {
                var roadNode = Assert.Single(records.OfType<RoadRegistry.RoadNode.RoadNode>());
                Assert.True(roadNode.IsRemoved, "the road node snapshot should say it is removed");
            });
    }

    // A genuine removal of an outlined road segment: nothing in the stream says it ever stopped being outlined, so
    // the road segment really is gone and the snapshot has to say so.
    [Fact]
    public Task WhenAnOutlinedRoadSegmentIsRemoved_ThenTheSnapshotSaysSoToo()
    {
        var added = _fixture.Create<RoadSegmentAdded>();
        added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Outlined;

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(added, new OutlinedRoadSegmentRemoved { Id = added.Id });

        return BuildProjection()
            .Scenario()
            .Given(message)
            .ExpectRecords(records =>
            {
                var roadSegment = Assert.Single(records.OfType<RoadRegistry.RoadSegment.RoadSegment>());
                Assert.True(roadSegment.IsRemoved, "the road segment snapshot should say it is removed");
            });
    }

    // The other reading of the same event. In the old system a road segment whose geometry stopped being outlined moved
    // from the outlined SqlStreamStore stream to the measured one: the outlined variant was removed while the road
    // segment carried on as measured, under the same id. RemoveOutlinedRoadSegment removed the aggregate either way,
    // which is how a road segment that is alive and measured in production ended up with isRemoved: true.
    //
    // The conversion itself is a RoadSegmentModified carrying the measured draw method, but driving one through this
    // projection needs a real database connection - ModifyRoadSegment recalculates the grade separated junctions over
    // session.Connection, which the in-memory store does not have. The road segment is therefore added as measured
    // here: what is under test is the decision, and the decision reads the same last draw method either way.
    [Fact]
    public Task WhenARoadSegmentThatIsNoLongerOutlinedIsRemoved_ThenTheSnapshotIsLeftAlone()
    {
        var added = _fixture.Create<RoadSegmentAdded>();
        added.GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured;

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(added, new OutlinedRoadSegmentRemoved { Id = added.Id });

        return BuildProjection()
            .Scenario()
            .Given(message)
            .ExpectRecords(records =>
            {
                var roadSegment = Assert.Single(records.OfType<RoadRegistry.RoadSegment.RoadSegment>());
                Assert.False(roadSegment.IsRemoved, "the road segment is measured, so its outlined variant going away removes nothing");
            });
    }

    private (MartenMigrationProjection, InMemoryDocumentStoreSession) BuildProjection()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        return (new MartenMigrationProjection(store), store);
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }
}
