namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenRealizingRoadSegmentV2;

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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RealizeRoadSegment;
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

        VerifyThatTicketHasError("WegsegmentRealisatieIngemetenNietToegelaten", null);
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
        bool mayModifyMeasuredRoadSegments = true)
    {
        var handler = new RealizeRoadSegmentV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            BuildRepository(store, plannedStatus, plannedDrawMethod),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            extractsDbContext,
            LoggerFactory);

        var sqsRequest = new RealizeRoadSegmentV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = new RoadSegmentId(PlannedSegmentId),
            MayModifyMeasuredRoadSegments = mayModifyMeasuredRoadSegments
        };

        await handler.Handle(new RealizeRoadSegmentV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);
    }

    private FakeRoadNetworkRepository BuildRepository(IDocumentStore store, RoadSegmentStatusV2? plannedStatus, RoadSegmentGeometryDrawMethodV2? plannedDrawMethod)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var deadEndNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Eindknoop);

        RoadNodeWasAdded[] nodes = [westNode, deadEndNode];
        RoadSegmentWasAdded[] segments =
        [
            // Hooks onto the dead end at (100,0) and runs north; the far end has nothing within reach.
            BuildSegment(PlannedSegmentId, null, null, BuildGeometry((100, 0), (100, 80)), plannedStatus ?? RoadSegmentStatusV2.Gepland, plannedDrawMethod),
            BuildSegment(RealizedSegmentId, westNode, deadEndNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd)
        ];

        return new FakeRoadNetworkRepository(store,
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray());
    }

    private RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2 type)
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

        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
        {
            var nodeIds = _segments
                .Where(x => roadSegmentIds.Contains(x.RoadSegmentId))
                .SelectMany(NodeIdsOf)
                .ToHashSet();

            // A planned segment carries no nodes, so nothing is reachable through it - but it is in scope itself.
            var connected = _segments
                .Where(x => roadSegmentIds.Contains(x.RoadSegmentId) || NodeIdsOf(x).Any(nodeIds.Contains))
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
