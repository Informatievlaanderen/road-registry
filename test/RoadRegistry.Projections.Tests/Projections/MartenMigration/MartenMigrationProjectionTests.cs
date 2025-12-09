namespace RoadRegistry.Projections.Tests.Projections.MartenMigration;

using AutoFixture;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using Infrastructure.MartenDb.Setup;
using Marten;
using Microsoft.Extensions.Logging.Abstractions;
using RoadNode;
using RoadRegistry.MartenMigration.Projections;
using RoadRegistry.Tests.BackOffice.Scenarios;
using RoadSegment;
using RoadSegment.ValueObjects;

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
                    Id = @event.Id,
                    TemporaryId = @event.TemporaryId,
                    OriginalId = @event.OriginalId,
                    Version = @event.Version,
                    Geometry = new()
                    {
                        SpatialReferenceSystemIdentifier = @event.Geometry.SpatialReferenceSystemIdentifier,
                        WKT = GeometryTranslator.Translate(@event.Geometry).AsText()
                    },
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

    private (MartenMigrationProjection, InMemoryDocumentStoreSession) BuildProjection()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        return (new MartenMigrationProjection(new MigrationRoadNetworkRepository(store, new NullLoggerFactory())), store);
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        storeOptions.AddMartenDbMigration();
        return storeOptions;
    }
}
