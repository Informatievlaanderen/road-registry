namespace RoadRegistry.Projections.Tests.Projections.MartenMigration;

using AutoFixture;
using BackOffice;
using BackOffice.Core;
using BackOffice.Messages;
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
            .Expect(new RoadRegistry.RoadNode.Events.RoadNodeAdded
                {
                    RoadNodeId = new RoadNodeId(@event.Id),
                    OriginalId = new RoadNodeId(@event.OriginalId!.Value),
                    Geometry = GeometryTranslator.Translate(@event.Geometry).ToGeometryObject(),
                    Type = RoadNodeType.Parse(@event.Type),
                }
            );
    }

    [Fact]
    public Task WhenRoadSegmentAdded_ThenSucceeded()
    {
        var @event = _fixture.Create<RoadSegmentAdded>();
        @event.LeftSide.StreetNameId = 1;
        @event.RightSide.StreetNameId = 1;
        @event.Surfaces = @event.Surfaces.Take(1).ToArray();

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(@event);

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(new RoadRegistry.RoadSegment.Events.RoadSegmentAdded
            {
                RoadSegmentId = new RoadSegmentId(@event.Id),
                OriginalId = new RoadSegmentId(@event.TemporaryId),
                Geometry = GeometryTranslator.Translate(@event.Geometry).ToGeometryObject(),
                StartNodeId = new RoadNodeId(@event.StartNodeId),
                EndNodeId = new RoadNodeId(@event.EndNodeId),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(@event.GeometryDrawMethod),
                AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>(RoadSegmentAccessRestriction.Parse(@event.AccessRestriction)),
                Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>(RoadSegmentCategory.Parse(@event.Category)),
                Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>(RoadSegmentMorphology.Parse(@event.Morphology)),
                Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.Parse(@event.Status)),
                StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                    .Add(new StreetNameLocalId(1)),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>(new OrganizationId(@event.MaintenanceAuthority.Code)),
                SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>()
                    .Add(new RoadSegmentPosition(@event.Surfaces[0].FromPosition),
                        new RoadSegmentPosition(@event.Surfaces[0].ToPosition),
                        RoadSegmentSurfaceType.Parse(@event.Surfaces[0].Type)),
                EuropeanRoadNumbers = [],
                NationalRoadNumbers = []
            });
    }

    // [Fact]
    // public async Task WhenRoadSegmentRemoved_ThenNone()
    // {
    //     var fixture = new RoadNetworkTestData().ObjectProvider;
    //     fixture.Freeze<RoadSegmentId>();
    //
    //     var roadSegment1Added = fixture.Create<RoadSegmentAdded>();
    //     var roadSegment1Removed = fixture.Create<RoadSegmentRemoved>();
    //
    //     await BuildProjection()
    //         .Scenario()
    //         .Given(roadSegment1Added, roadSegment1Removed)
    //         .ExpectNone();
    // }

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
