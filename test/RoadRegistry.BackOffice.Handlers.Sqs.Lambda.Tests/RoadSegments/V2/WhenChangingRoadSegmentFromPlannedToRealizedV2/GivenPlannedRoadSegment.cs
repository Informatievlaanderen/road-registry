namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentFromPlannedToRealizedV2;

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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentFromPlannedToRealized;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
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
public class GivenPlannedRoadSegment : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    // The planned segment being realized, and an existing realized road it can hook onto at (100, 0).
    private const int PlannedSegmentId = 1;
    private const int RealizedSegmentId = 2;

    public GivenPlannedRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenTheInwinningIsCompleted_ThenTicketIsCompleted()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true));

        var summary = completedResults.Should().ContainSingle().Which.Summary;
        summary.RoadSegments.Modified.Should().ContainSingle();
        // The far end of the planned segment had no road node within reach, so one was added for it.
        summary.RoadNodes.Added.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenTheRoadSegmentDoesNotExist_ThenTicketError()
    {
        // The handler needs the segment's own geometry to work out what to load around it, so it reports a missing
        // segment itself rather than scoping on nothing and letting the domain find out.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), seedTheRoadSegment: false);

        VerifyThatTicketHasError("NotFound", null);
    }

    [Fact]
    public async Task WhenARoadNodeIsOffTheSegmentButWithinReach_ThenItIsStillInScope()
    {
        // The dead end sits 40cm off the planned segment's start point. Scoping on the line alone would not reach it -
        // the two do not touch - so the segment would find no road node and be refused as an island. The buffer is
        // what brings it in.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), plannedStartOffset: 0.4);

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Modified.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenTheRoadNodeInReachHasNotCompletedItsInwinning_ThenTicketError()
    {
        // A V1 node carries no type and still sits on the coordinate it was imported with, which is more precise than
        // the centimetre the register works in. Snapping onto it cannot produce a segment that agrees with it on where
        // it is, so it is refused rather than knotted onto.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), deadEndNodeIsMigrated: false);

        VerifyThatTicketHasError("WegknoopInwinningsstatusNietCompleet",
            "De wegknoop met id 11 heeft niet de inwinningsstatus 'compleet'.");
    }

    [Fact]
    public async Task WhenTheInwinningIsNotCompleted_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet",
            $"Het wegsegment met id {PlannedSegmentId} heeft niet de inwinningsstatus 'compleet'.");
    }

    [Fact]
    public async Task WhenTheRoadSegmentHasNoInwinningAtAll_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(registerInwinning: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet", null);
    }

    [Fact]
    public async Task WhenTheRoadSegmentIsAlreadyRealized_ThenTicketError()
    {
        // VAL-4, re-validated here because the request may have gone stale between being accepted and handled.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), plannedStatus: RoadSegmentStatusV2.Gerealiseerd);

        VerifyThatTicketHasError("WegsegmentRealisatieStatusNietCorrect", null);
    }

    [Fact]
    public async Task WhenTheCallerMayNotTouchMeasuredRoadSegments_ThenTicketError()
    {
        // VAL-9
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true),
            plannedDrawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten,
            mayModifyMeasuredRoadSegments: false);

        VerifyThatTicketHasError("WegsegmentIngemetenNietToegelaten", null);
    }

    private ExtractsDbContext ExtractsDbContextWith(bool inwinningCompleted = true, bool registerInwinning = true)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        if (registerInwinning)
        {
            db.InwinningRoadSegments.Add(new InwinningRoadSegment
            {
                NisCode = "11001",
                RoadSegmentId = PlannedSegmentId,
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

    private async Task HandleRequest(
        IDocumentStore store,
        ExtractsDbContext extractsDbContext,
        RoadSegmentStatusV2? plannedStatus = null,
        RoadSegmentGeometryDrawMethodV2? plannedDrawMethod = null,
        bool mayModifyMeasuredRoadSegments = true,
        bool seedTheRoadSegment = true,
        double plannedStartOffset = 0,
        bool deadEndNodeIsMigrated = true)
    {
        var (nodes, segments) = BuildNetwork(plannedStatus, plannedDrawMethod, seedTheRoadSegment, plannedStartOffset, deadEndNodeIsMigrated);

        // The handler reads the road segment straight from the store to work out what to scope on, so the network has
        // to be there as well as in the repository.
        await using (var seedSession = store.LightweightSession())
        {
            foreach (var segment in segments)
            {
                seedSession.Store(segment);
            }
            foreach (var node in nodes)
            {
                seedSession.Store(node);
            }
            await seedSession.SaveChangesAsync();
        }

        var handler = new ChangeRoadSegmentFromPlannedToRealizedV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            new FakeRoadNetworkRepository(store, nodes, segments),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            extractsDbContext,
            LoggerFactory);

        var sqsRequest = new ChangeRoadSegmentFromPlannedToRealizedV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = new RoadSegmentId(PlannedSegmentId),
            MayModifyMeasuredRoadSegments = mayModifyMeasuredRoadSegments
        };

        await handler.Handle(new ChangeRoadSegmentFromPlannedToRealizedV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);
    }

    private (RoadNode[] Nodes, RoadSegment[] Segments) BuildNetwork(
        RoadSegmentStatusV2? plannedStatus,
        RoadSegmentGeometryDrawMethodV2? plannedDrawMethod,
        bool includeThePlannedRoadSegment,
        double plannedStartOffset,
        bool deadEndNodeIsMigrated)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        // A node that has not completed its inwinning carries no type - that is what HasMigrated() reads.
        var deadEndNode = BuildNode(11, 100, 0, deadEndNodeIsMigrated ? RoadNodeTypeV2.Eindknoop : null);

        RoadNodeWasAdded[] nodes = [westNode, deadEndNode];
        RoadSegmentWasAdded[] segments =
        [
            // Hooks onto the dead end at (100,0) and runs north; the far end has nothing within reach.
            .. includeThePlannedRoadSegment
                ? new[] { BuildSegment(PlannedSegmentId, null, null, BuildGeometry((100 + plannedStartOffset, 0), (100 + plannedStartOffset, 80)), plannedStatus ?? RoadSegmentStatusV2.Gepland, plannedDrawMethod) }
                : [],
            BuildSegment(RealizedSegmentId, westNode, deadEndNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd)
        ];

        return (
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray());
    }

    private RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2? type)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = false,
            Type = type,
            Provenance = new ProvenanceData(_testData.Provenance)
        };
    }

    private RoadSegmentGeometry BuildGeometry(params (double X, double Y)[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates.Select(x => new Coordinate(x.X, x.Y)).ToArray())])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }

    private static RoadSegmentDynamicAttributeValues<T> Spanning<T>(RoadSegmentDynamicAttributeValues<T> template, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>().Add(template.Values.First().Value, geometry);
    }

    private RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded? startNode,
        RoadNodeWasAdded? endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null)
    {
        var template = _testData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode?.RoadNodeId,
            EndNodeId = endNode?.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = status,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = Spanning(template.PedestrianTrafficDirection, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
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

    // Scoping decides which segments the domain gets to see, so the fake honours it the way the real repository does.
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

        public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
        {
            var scoped = _segments
                .Where(x => ids is not null && ids.RoadSegmentIds.Contains(x.RoadSegmentId)
                            || geometry is not null && x.Geometry.Value.Intersects(geometry))
                .ToList();

            return Task.FromResult(ToIds(scoped));
        }

        // A planned road segment carries no road nodes, so there is nothing to reach the surrounding network through:
        // the handler scopes on the geometry instead and never asks for this.
        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
            => throw new NotImplementedException();

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
