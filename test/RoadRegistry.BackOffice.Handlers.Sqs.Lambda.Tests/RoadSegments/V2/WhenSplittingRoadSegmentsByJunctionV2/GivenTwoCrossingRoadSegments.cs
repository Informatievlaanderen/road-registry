namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenSplittingRoadSegmentsByJunctionV2;

using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using FluentAssertions;
using Marten;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentsByJunctionV2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
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
using ProvenanceData = Be.Vlaanderen.Basisregisters.GrAr.Provenance.ProvenanceData;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

[Collection("runsequential")]
public class GivenTwoCrossingRoadSegments : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    public GivenTwoCrossingRoadSegments(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    private RoadSegmentId Segment1Id => _testData.Segment1Added.RoadSegmentId;
    private RoadSegmentId Segment2Id => new(_testData.Segment1Added.RoadSegmentId.ToInt32() + 1);

    [Fact]
    public async Task WhenSplittingByJunction_ThenTicketCompletedWithFourRoadSegments()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store, id => BuildCrossingNetwork(id));

        List<ETagResponse> completedResult = null;
        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
                completedResult = JsonConvert.DeserializeObject<List<ETagResponse>>(result.ResultAsJson!));

        await HandleRequest(CreateSqsRequest(Segment1Id, Segment2Id), store, roadNetworkRepository);

        completedResult.Should().NotBeNull();
        completedResult.Should().HaveCount(4);
        completedResult.Should().OnlyContain(x => x.Location.Contains("/wegsegmenten/") && !string.IsNullOrEmpty(x.ETag));
    }

    [Fact]
    public async Task WhenHandledTwiceWithSameRequest_ThenSecondResponseMatchesFirst()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store, id => BuildCrossingNetwork(id));

        var sqsRequest = CreateSqsRequest(Segment1Id, Segment2Id);

        var completedResults = new List<List<ETagResponse>>();
        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
                completedResults.Add(JsonConvert.DeserializeObject<List<ETagResponse>>(result.ResultAsJson!)!));

        await HandleRequest(sqsRequest, store, roadNetworkRepository);
        await HandleRequest(sqsRequest, store, roadNetworkRepository);

        completedResults.Should().HaveCount(2);
        completedResults[1].Should().BeEquivalentTo(completedResults[0]);
    }

    [Fact]
    public async Task WhenNoJunctionBetweenTheSegments_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store, id => BuildCrossingNetwork(id, withJunction: false));

        await HandleRequest(CreateSqsRequest(Segment1Id, Segment2Id), store, roadNetworkRepository);

        VerifyThatTicketHasError("GeenKruisingTussenWegsegmenten", null);
    }

    [Fact]
    public async Task WhenARoadSegmentDoesNotExist_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store, id => new ScopedRoadNetwork(id));

        await HandleRequest(CreateSqsRequest(Segment1Id, Segment2Id), store, roadNetworkRepository);

        VerifyThatTicketHasError("WegsegmentNietGevondenOfVerwijderd", null);
    }

    [Fact]
    public async Task WhenARoadSegmentHasInvalidStatus_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store, id => BuildCrossingNetwork(id, segment2Status: RoadSegmentStatusV2.Gepland));

        await HandleRequest(CreateSqsRequest(Segment1Id, Segment2Id), store, roadNetworkRepository);

        VerifyThatTicketHasError("WegsegmentKnippenOpKruisingStatusNietCorrect", null);
    }

    private ScopedRoadNetwork BuildCrossingNetwork(ScopedRoadNetworkId id, RoadSegmentStatusV2? segment2Status = null, bool withJunction = true)
    {
        var crossing = new Coordinate(50.0, 50.0);
        var node3Coord = new Coordinate(0.0, 100.0);
        var node4Coord = new Coordinate(100.0, 0.0);

        var node1 = RoadNode.Create(_testData.Segment1StartNodeAdded).WithoutChanges();
        var node2 = RoadNode.Create(_testData.Segment1EndNodeAdded).WithoutChanges();
        var node3 = RoadNode.Create(_testData.Segment2StartNodeAdded with { Geometry = Point(node3Coord).ToRoadNodeGeometry() }).WithoutChanges();
        var node4 = RoadNode.Create(_testData.Segment2EndNodeAdded with { Geometry = Point(node4Coord).ToRoadNodeGeometry() }).WithoutChanges();

        var segment1 = RoadSegment.Create(_testData.Segment1Added).WithoutChanges();
        var segment2 = RoadSegment.Create(_testData.Segment1Added with
        {
            RoadSegmentId = Segment2Id,
            Status = segment2Status ?? _testData.Segment1Added.Status,
            Geometry = LineThrough(node3Coord, crossing, node4Coord).ToRoadSegmentGeometry(),
            StartNodeId = _testData.Segment2StartNodeAdded.RoadNodeId,
            EndNodeId = _testData.Segment2EndNodeAdded.RoadNodeId
        }).WithoutChanges();

        GradeJunction[] gradeJunctions = withJunction
            ?
            [
                GradeJunction.Create(new GradeJunctionWasAdded
                {
                    GradeJunctionId = new GradeJunctionId(1),
                    RoadSegmentId1 = Segment1Id,
                    RoadSegmentId2 = Segment2Id,
                    Geometry = JunctionGeometry.Create(new Point(crossing) { SRID = WellknownSrids.Lambert08 }),
                    Provenance = new ProvenanceData(_testData.Provenance)
                }).WithoutChanges()
            ]
            : [];

        return new ScopedRoadNetwork(id, [node1, node2, node3, node4], [segment1, segment2], [], gradeJunctions);
    }

    private static Point Point(Coordinate coordinate)
    {
        return new Point(coordinate) { SRID = WellknownSrids.Lambert08 };
    }

    private static MultiLineString LineThrough(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(new CoordinateArraySequence(coordinates), GeometryConfiguration.GeometryFactory)])
        {
            SRID = WellknownSrids.Lambert08
        };
    }

    private SplitRoadSegmentsByJunctionV2SqsRequest CreateSqsRequest(RoadSegmentId roadSegmentId1, RoadSegmentId roadSegmentId2)
    {
        return new SplitRoadSegmentsByJunctionV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId1 = roadSegmentId1,
            RoadSegmentId2 = roadSegmentId2
        };
    }

    private async Task HandleRequest(SplitRoadSegmentsByJunctionV2SqsRequest sqsRequest, IDocumentStore store, IRoadNetworkRepository roadNetworkRepository)
    {
        var sqsLambdaRequest = new SplitRoadSegmentsByJunctionV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        var handler = new SplitRoadSegmentsByJunctionV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            roadNetworkRepository,
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            LoggerFactory);

        await handler.Handle(sqsLambdaRequest, CancellationToken.None);
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
        private readonly Func<ScopedRoadNetworkId, ScopedRoadNetwork> _loadFactory;

        public FakeRoadNetworkRepository(IDocumentStore store, Func<ScopedRoadNetworkId, ScopedRoadNetwork> loadFactory)
        {
            _real = new RoadNetworkRepository(store);
            _loadFactory = loadFactory;
        }

        public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
            => Task.FromResult(ids ?? new RoadNetworkIds([], [], [], []));

        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
            => Task.FromResult(new RoadNetworkIds([], roadSegmentIds, [], []));

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
