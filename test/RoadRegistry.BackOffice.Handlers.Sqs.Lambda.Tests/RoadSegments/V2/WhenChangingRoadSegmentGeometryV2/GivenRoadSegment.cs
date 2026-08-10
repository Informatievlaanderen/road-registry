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
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
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

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithoutZone());

        VerifyThatTicketHasError("RoadSegmentOutsideInwinningszone",
            "Het wegsegment valt niet volledig binnen een gemeente die de inwinningsstatus 'compleet' heeft.");
    }

    [Fact]
    public async Task WhenTheNewGeometryCrossesASegmentItShareNoNodeWith_ThenAGradeJunctionIsAdded()
    {
        // The crossed segment is not reachable through the road nodes of the one being changed, so it is only in scope
        // if the new geometry itself is used to scope. Without that it is never loaded and the crossing goes unnoticed.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store,
            extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-1000, 1000), completed: true),
            // The seeded segment runs (0,0) -> (50,50) -> (100,100); this dips it south through the crossing segment
            // while both end vertices stay on their own road node.
            newGeometry: new MultiLineString([new LineString([new Coordinate(0, 0), new Coordinate(60, -50), new Coordinate(100, 100)])])
                .WithSrid(WellknownSrids.Lambert08)
                .ToRoadSegmentGeometry(),
            otherSegments: [CrossingSegment()],
            otherNodes: [CrossingSegmentStartNode(), CrossingSegmentEndNode()]);

        completedResults.Should().ContainSingle()
            .Which.Summary.GradeJunctions.Added.Should().ContainSingle();
    }

    // A segment running north-south well below the seeded one, sharing neither of its road nodes. The old geometry
    // stays clear of it entirely.
    private RoadSegmentWasAdded CrossingSegment()
    {
        return _testData.Segment1Added with
        {
            RoadSegmentId = new RoadSegmentId(99),
            StartNodeId = new RoadNodeId(98),
            EndNodeId = new RoadNodeId(99),
            Geometry = new MultiLineString([new LineString([new Coordinate(60, -100), new Coordinate(60, -10)])])
                .WithSrid(WellknownSrids.Lambert08)
                .ToRoadSegmentGeometry()
        };
    }

    private RoadNodeWasAdded CrossingSegmentStartNode()
    {
        return _testData.Segment1StartNodeAdded with
        {
            RoadNodeId = new RoadNodeId(98),
            Geometry = new Point(new Coordinate(60, -100)).WithSrid(WellknownSrids.Lambert08).ToRoadNodeGeometry()
        };
    }

    private RoadNodeWasAdded CrossingSegmentEndNode()
    {
        return _testData.Segment1EndNodeAdded with
        {
            RoadNodeId = new RoadNodeId(99),
            Geometry = new Point(new Coordinate(60, -10)).WithSrid(WellknownSrids.Lambert08).ToRoadNodeGeometry()
        };
    }

    [Fact]
    public async Task WhenTheInwinningOfTheRoadSegmentItselfIsNotCompleted_ThenTicketError()
    {
        // The zone is completed, but this particular road segment is still being collected: lying in a finished zone
        // is not on its own enough to edit it.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-1000, 1000), completed: true, inwinningCompleted: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet",
            $"Het wegsegment met id {_testData.Segment1Added.RoadSegmentId} heeft niet de inwinningsstatus 'compleet'.");
    }

    [Fact]
    public async Task WhenTheRoadSegmentHasNoInwinningAtAll_ThenTicketError()
    {
        // Never collected at all, which is no more editable than a collection that is still running.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, extractsDbContext: ExtractsDbContextWithZone(ZoneCovering(-1000, 1000), completed: true, registerInwinning: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet", null);
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

    private ExtractsDbContext ExtractsDbContextWithZone(Geometry contour, bool completed, bool registerInwinning = true, bool inwinningCompleted = true)
    {
        var db = ExtractsDbContextWithoutZone(registerInwinning, inwinningCompleted);
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

    // The inwinning of the road segment itself is a rule of its own, next to the zone it lies in, so it is registered
    // as completed by default: a fixture that is about the zone should not trip over it.
    private ExtractsDbContext ExtractsDbContextWithoutZone(bool registerInwinning = true, bool inwinningCompleted = true)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        if (registerInwinning)
        {
            db.InwinningRoadSegments.Add(new InwinningRoadSegment
            {
                NisCode = "11001",
                RoadSegmentId = _testData.Segment1Added.RoadSegmentId.ToInt32(),
                Completed = inwinningCompleted
            });
        }
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

    private ChangeRoadSegmentGeometryV2SqsRequest CreateSqsRequest(RoadSegmentGeometry? newGeometry = null)
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
            Geometry = newGeometry ?? NewGeometry(),
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

    private async Task HandleRequest(
        IDocumentStore store,
        ExtractsDbContext? extractsDbContext = null,
        RoadSegmentGeometry? newGeometry = null,
        RoadSegmentWasAdded[]? otherSegments = null,
        RoadNodeWasAdded[]? otherNodes = null)
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
            BuildRepository(store, otherSegments, otherNodes),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            organizationCache.Object,
            new Mock<IStreetNameClient>().Object,
            extractsDbContext ?? new FakeExtractsDbContextFactory().CreateDbContext(),
            LoggerFactory);

        await handler.Handle(new ChangeRoadSegmentGeometryV2SqsLambdaRequest(Guid.NewGuid().ToString(), CreateSqsRequest(newGeometry)), CancellationToken.None);
    }

    // The whole network the fake repository can hand out; what the handler actually gets is whatever it scopes to.
    private FakeRoadNetworkRepository BuildRepository(IDocumentStore store, RoadSegmentWasAdded[]? otherSegments, RoadNodeWasAdded[]? otherNodes)
    {
        RoadNodeWasAdded[] nodes = [_testData.Segment1StartNodeAdded, _testData.Segment1EndNodeAdded, .. otherNodes ?? []];
        RoadSegmentWasAdded[] segments = [_testData.Segment1Added, .. otherSegments ?? []];

        return new FakeRoadNetworkRepository(store,
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray());
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

    // Scoping is what decides which segments the domain gets to see, so the fake honours it the way the real
    // repository does rather than handing out the whole network regardless of what was asked for.
    private sealed class FakeRoadNetworkRepository : IRoadNetworkRepository
    {
        private readonly RoadNetworkRepository _real;
        private readonly IReadOnlyList<RoadNode> _nodes;
        private readonly IReadOnlyList<RoadSegment> _segments;

        public FakeRoadNetworkRepository(IDocumentStore store, IReadOnlyList<RoadNode> nodes, IReadOnlyList<RoadSegment> segments)
        {
            _real = new RoadNetworkRepository(store);
            _nodes = nodes;
            _segments = segments;
        }

        // Whatever was already in scope, plus everything the geometry runs into.
        public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
        {
            var scoped = _segments
                .Where(x => ids is not null && ids.RoadSegmentIds.Contains(x.RoadSegmentId)
                            || geometry is not null && x.Geometry.Value.Intersects(geometry))
                .ToList();

            return Task.FromResult(ToIds(scoped));
        }

        // The segments reachable through the start and end node of the ones asked for.
        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
        {
            var nodeIds = _segments
                .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
                .SelectMany(NodeIdsOf)
                .ToHashSet();

            var connected = _segments
                .Where(x => NodeIdsOf(x).Any(nodeIds.Contains))
                .ToList();

            return Task.FromResult(ToIds(connected));
        }

        public Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
        {
            return Task.FromResult(new ScopedRoadNetwork(roadNetworkId,
                _nodes.Where(x => ids.RoadNodeIds.Contains(x.RoadNodeId)).ToArray(),
                _segments.Where(x => ids.RoadSegmentIds.Contains(x.RoadSegmentId)).ToArray(),
                [],
                []));
        }

        private static IEnumerable<RoadNodeId> NodeIdsOf(RoadSegment segment)
        {
            return new[] { segment.StartNodeId, segment.EndNodeId }
                .Where(x => x is not null)
                .Select(x => x!.Value);
        }

        private RoadNetworkIds ToIds(IReadOnlyCollection<RoadSegment> segments)
        {
            return new RoadNetworkIds(
                segments.SelectMany(NodeIdsOf).Distinct().ToArray(),
                segments.Select(x => x.RoadSegmentId).Distinct().ToArray(),
                [],
                []);
        }

        public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
            => _real.Save(session, roadNetwork, commandName);

        public Task<RoadNetworkIds> GetUnderlyingIdsForExtract(IDocumentSession session, Geometry geometry)
            => throw new NotImplementedException();

        public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
