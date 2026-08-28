namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentStatusV2;

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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentStatus;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.Framework;
using RoadRegistry.ValueObjects;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// The handler for a status change between two statuses that both leave the segment outside the network. It carries no
// road nodes either way, so the handler scopes on the segment alone and nothing around it is loaded.
[Collection("runsequential")]
public class GivenAnUnconnectedStatusChange : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    // The segment whose status is being changed, and a realized road it shares no road node with.
    private const int ChangedSegmentId = 1;
    private const int NeighbourSegmentId = 2;

    public GivenAnUnconnectedStatusChange(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
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
        // Nothing around the segment is touched: it was outside the network before and stays outside it.
        summary.RoadNodes.Added.Should().BeEmpty();
        summary.RoadNodes.Removed.Should().BeEmpty();
    }

    [Fact]
    public async Task WhenTheRoadSegmentDoesNotExist_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), seedTheRoadSegment: false);

        VerifyThatTicketHasError("NotFound", null);
    }

    [Fact]
    public async Task WhenTheInwinningIsNotCompleted_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet",
            $"Het wegsegment met id {ChangedSegmentId} heeft niet de inwinningsstatus 'compleet'.");
    }

    [Fact]
    public async Task WhenTheRoadSegmentHasTheWrongStatus_ThenTicketError()
    {
        // VAL-4, re-validated here because the request may have gone stale between being accepted and handled. The
        // message names the status the segment should have been in.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true), status: RoadSegmentStatusV2.Gepland);

        VerifyThatTicketHasError("WegsegmentStatuswijzigingStatusNietCorrect",
            $"Het wegsegment met id {ChangedSegmentId} heeft een status die verschilt van 'buiten gebruik'.");
    }

    [Fact]
    public async Task WhenTheCallerMayNotTouchMeasuredRoadSegments_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWith(inwinningCompleted: true),
            drawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten,
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
                RoadSegmentId = ChangedSegmentId,
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
        RoadSegmentStatusV2? status = null,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null,
        bool mayModifyMeasuredRoadSegments = true,
        bool seedTheRoadSegment = true)
    {
        var (nodes, segments) = BuildNetwork(status, drawMethod, seedTheRoadSegment);

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

        var handler = new ChangeRoadSegmentStatusV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            new StatusChangeFakeRoadNetworkRepository(store, nodes, segments),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            extractsDbContext,
            LoggerFactory);

        var sqsRequest = new ChangeRoadSegmentStatusV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = new RoadSegmentId(ChangedSegmentId),
            StatusChange = RoadSegmentStatusChange.OutOfUseToHistorized,
            MayModifyMeasuredRoadSegments = mayModifyMeasuredRoadSegments
        };

        await handler.Handle(new ChangeRoadSegmentStatusV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);
    }

    private (RoadNode[] Nodes, RoadSegment[] Segments) BuildNetwork(
        RoadSegmentStatusV2? status,
        RoadSegmentGeometryDrawMethodV2? drawMethod,
        bool includeTheRoadSegment)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(11, 200, 0, RoadNodeTypeV2.Eindknoop);

        RoadNodeWasAdded[] nodes = [westNode, eastNode];
        RoadSegmentWasAdded[] segments =
        [
            .. includeTheRoadSegment
                ? new[] { BuildSegment(ChangedSegmentId, null, null, BuildGeometry((100, 40), (100, 120)), status ?? RoadSegmentStatusV2.BuitenGebruik, drawMethod) }
                : [],
            BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
        ];

        return (
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
}
