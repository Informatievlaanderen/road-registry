namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentGeometryV2;

using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using Marten;
using Moq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometry;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using Be.Vlaanderen.Basisregisters.GrAr.CrsTransform;
using GeometryExtensions = Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology.GeometryExtensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.Framework;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

[Collection("runsequential")]
public class GivenRoadSegment : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    public GivenRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenTheGeometryLiesWithinACompletedInwinningszone_ThenTicketIsCompleted()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-1000, 1000), completed: true));

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Modified.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenThereIsNoInwinningszoneAtAll_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store);

        VerifyThatTicketHasError("RoadSegmentOutsideInwinningszone",
            "Het wegsegment valt niet volledig binnen een gemeente die de inwinningsstatus 'compleet' heeft.");
    }

    [Fact]
    public async Task WhenTheInwinningszoneCoveringItIsNotCompleted_ThenTicketError()
    {
        // The zone covers the whole geometry, but its inwinning is still running.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-1000, 1000), completed: false));

        VerifyThatTicketHasError("RoadSegmentOutsideInwinningszone", null);
    }

    [Fact]
    public async Task WhenTheGeometryOnlyPartlyLiesWithinACompletedInwinningszone_ThenTicketError()
    {
        // The geometry runs from (0,0) out to (100,100); this zone stops at 60, so the far end sticks out. It has to
        // lie there completely, not merely start there.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-10, 60), completed: true));

        VerifyThatTicketHasError("RoadSegmentOutsideInwinningszone", null);
    }

    // A zone as the inwinning extract stores it: Lambert 72. It is described here in the Lambert 2008 the road
    // segment lives in and then converted, so the area it covers is the area meant.
    private static Geometry ZoneCovering(double min, double max)
    {
        return GeometryExtensions.WithSrid(new Polygon(new LinearRing([
            new Coordinate(min, min),
            new Coordinate(max, min),
            new Coordinate(max, max),
            new Coordinate(min, max),
            new Coordinate(min, min)
        ])), WellknownSrids.Lambert08).TransformFromLambert08To72();
    }

    private static ExtractsDbContext ExtractsDbContextWithZone(Geometry contour, bool completed)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        db.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = "11001",
            Operator = "op",
            DownloadId = Guid.NewGuid(),
            Contour = contour,
            Completed = completed
        });
        db.SaveChanges();
        return db;
    }

    private List<ChangeRoadNetworkTicketResult> CaptureCompletedResults()
    {
        var completedResults = new List<ChangeRoadNetworkTicketResult>();
        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
                completedResults.Add(JsonConvert.DeserializeObject<ChangeRoadNetworkTicketResult>(result.ResultAsJson!)!));
        return completedResults;
    }

    // The seeded segment runs (0,0) -> (50,50) -> (100,100); this only lifts its middle vertex, so both end vertices
    // stay on their road node and nothing is dragged along.
    private RoadSegmentGeometry NewGeometry()
    {
        return new MultiLineString([new LineString([new Coordinate(0, 0), new Coordinate(50, 60), new Coordinate(100, 100)])])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }

    private ChangeRoadSegmentGeometryV2SqsRequest CreateSqsRequest()
    {
        var attributes = _testData.Segment1Added;

        // Every mandatory attribute over the full length: a null from/to position resolves to 0 and to the length of
        // the geometry being submitted.
        return new ChangeRoadSegmentGeometryV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = attributes.RoadSegmentId,
            Geometry = NewGeometry(),
            MayModifyMeasuredRoadSegments = true,
            Morphology = [new AttributeValue<RoadSegmentMorphologyV2>(null, null, attributes.Morphology.Values.First().Value)],
            SurfaceType = [new AttributeValue<RoadSegmentSurfaceTypeV2>(null, null, attributes.SurfaceType.Values.First().Value)],
            AccessRestriction = [new AttributeValue<RoadSegmentAccessRestrictionV2>(null, null, attributes.AccessRestriction.Values.First().Value)],
            Category = [new AttributeValue<RoadSegmentCategoryV2>(null, null, attributes.Category.Values.First().Value)],
            // Not applicable, so the street name registry is never consulted.
            StreetName = [new SidedAttributeValue<StreetNameLocalId>(RoadSegmentAttributeSide.Beide, null, null, StreetNameLocalId.NotApplicable)],
            MaintenanceAuthority = [new SidedAttributeValue<OrganizationId>(RoadSegmentAttributeSide.Beide, null, null, attributes.MaintenanceAuthorityId.Values.First().Value)],
            CarTrafficDirection = [new AttributeValue<RoadSegmentTrafficDirection>(null, null, attributes.CarTrafficDirection.Values.First().Value)],
            BikeTrafficDirection = [new AttributeValue<RoadSegmentTrafficDirection>(null, null, attributes.BikeTrafficDirection.Values.First().Value)],
            PedestrianTrafficDirection = [new AttributeValue<RoadSegmentPedestrianTrafficDirection>(null, null, attributes.PedestrianTrafficDirection.Values.First().Value)]
        };
    }

    private async Task HandleRequest(IDocumentStore store, ExtractsDbContext? extractsDbContext = null)
    {
        var organizationCache = new Mock<IOrganizationCache>();
        organizationCache
            .Setup(x => x.FindByIdOrOvoCodeOrKboNumberAsync(It.IsAny<OrganizationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationId id, CancellationToken _) => OrganizationDetail.FromCode(id));

        var handler = new ChangeRoadSegmentGeometryV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            BuildRepository(store),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            organizationCache.Object,
            new Mock<IStreetNameClient>().Object,
            extractsDbContext ?? new FakeExtractsDbContextFactory().CreateDbContext(),
            LoggerFactory);

        await handler.Handle(new ChangeRoadSegmentGeometryV2SqsLambdaRequest(Guid.NewGuid().ToString(), CreateSqsRequest()), CancellationToken.None);
    }

    private FakeRoadNetworkRepository BuildRepository(IDocumentStore store)
    {
        return new FakeRoadNetworkRepository(store,
            new RoadNetworkIds([new RoadNodeId(1), new RoadNodeId(2)], [_testData.Segment1Added.RoadSegmentId], [], []),
            BuildSeedNetwork);
    }

    private ScopedRoadNetwork BuildSeedNetwork(ScopedRoadNetworkId id)
    {
        return new ScopedRoadNetwork(id,
            [
                RoadNode.Create(_testData.Segment1StartNodeAdded).WithoutChanges(),
                RoadNode.Create(_testData.Segment1EndNodeAdded).WithoutChanges()
            ],
            [RoadSegment.Create(_testData.Segment1Added).WithoutChanges()],
            [],
            []);
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder
            .Register(_ => Dispatch.Using(Resolve.WhenEqualToMessage(
            [
                new RoadNetworkCommandModule(
                    Store,
                    ScopedContainer,
                    new FakeRoadNetworkSnapshotReader(),
                    Clock,
                    new FakeExtractUploadFailedEmailClient(),
                    LoggerFactory
                )
            ]), ApplicationMetadata))
            .SingleInstance();
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        return storeOptions;
    }

    private sealed class FakeRoadNetworkRepository : IRoadNetworkRepository
    {
        private readonly RoadNetworkRepository _real;
        private readonly RoadNetworkIds _ids;
        private readonly Func<ScopedRoadNetworkId, ScopedRoadNetwork> _loadFactory;

        public FakeRoadNetworkRepository(IDocumentStore store, RoadNetworkIds ids, Func<ScopedRoadNetworkId, ScopedRoadNetwork> loadFactory)
        {
            _real = new RoadNetworkRepository(store);
            _ids = ids;
            _loadFactory = loadFactory;
        }

        public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
            => Task.FromResult(_ids);

        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
            => Task.FromResult(_ids);

        public Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
            => Task.FromResult(_loadFactory(roadNetworkId));

        public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
            => _real.Save(session, roadNetwork, commandName);

        public Task<RoadNetworkIds> GetUnderlyingIdsForExtract(IDocumentSession session, Geometry geometry)
            => throw new NotImplementedException();

        public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
