namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenSplittingRoadSegmentV2;

using System.Collections.Generic;
using System.Linq;
using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using FluentAssertions;
using Marten;
using Moq;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.SplitRoadSegmentV2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
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
public class GivenRoadSegment : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    public GivenRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenSplittingRealizedRoadSegment_ThenTicketCompletedWithTwoRoadSegments()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store,
            new RoadNetworkIds([new RoadNodeId(1), new RoadNodeId(2)], [new RoadSegmentId(1)], [], []),
            BuildSeedNetwork);

        List<ETagResponse> completedResult = null;
        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
                completedResult = JsonConvert.DeserializeObject<List<ETagResponse>>(result.ResultAsJson!));

        await HandleRequest(CreateSqsRequest(CutPosition(50, 50)), store, roadNetworkRepository);

        completedResult.Should().NotBeNull();
        completedResult.Should().HaveCount(2);
        completedResult.Should().OnlyContain(x => x.Location.Contains("/wegsegmenten/") && !string.IsNullOrEmpty(x.ETag));
    }

    [Fact]
    public async Task WhenRoadSegmentDoesNotExist_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store,
            new RoadNetworkIds([], [], [], []),
            id => new ScopedRoadNetwork(id));

        await HandleRequest(CreateSqsRequest(CutPosition(50, 50)), store, roadNetworkRepository);

        VerifyThatTicketHasError("WegsegmentNietGevondenOfVerwijderd", "Wegsegment 1 bestaat niet of is verwijderd.");
    }

    [Fact]
    public async Task WhenCutPositionTooCloseToRoadNode_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = new FakeRoadNetworkRepository(store,
            new RoadNetworkIds([new RoadNodeId(1), new RoadNodeId(2)], [new RoadSegmentId(1)], [], []),
            BuildSeedNetwork);

        await HandleRequest(CreateSqsRequest(CutPosition(0.5, 0.5)), store, roadNetworkRepository);

        VerifyThatTicketHasError("KnippositieTeDichtBijWegknoop", null);
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

    private static Point CutPosition(double x, double y)
    {
        return new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 };
    }

    private SplitRoadSegmentV2SqsRequest CreateSqsRequest(Point cutPosition)
    {
        return new SplitRoadSegmentV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = new RoadSegmentId(1),
            CutPosition = cutPosition
        };
    }

    private async Task HandleRequest(SplitRoadSegmentV2SqsRequest sqsRequest, IDocumentStore store, IRoadNetworkRepository roadNetworkRepository)
    {
        var sqsLambdaRequest = new SplitRoadSegmentV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        var handler = new SplitRoadSegmentV2SqsLambdaRequestHandler(
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
        private readonly RoadNetworkIds _ids;
        private readonly Func<ScopedRoadNetworkId, ScopedRoadNetwork> _loadFactory;

        public FakeRoadNetworkRepository(IDocumentStore store, RoadNetworkIds ids, Func<ScopedRoadNetworkId, ScopedRoadNetwork> loadFactory)
        {
            _real = new RoadNetworkRepository(store);
            _ids = ids;
            _loadFactory = loadFactory;
        }

        public Task<RoadNetworkIds> GetUnderlyingIdsWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
            => Task.FromResult(_ids);

        public Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
            => Task.FromResult(_loadFactory(roadNetworkId));

        public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
            => _real.Save(session, roadNetwork, commandName);

        public Task<RoadNetworkIds> GetUnderlyingIds(IDocumentSession session, Geometry? geometry = null, RoadNetworkIds? ids = null)
            => throw new NotImplementedException();

        public Task<RoadNetworkIds> GetUnderlyingIdsForExtract(IDocumentSession session, Geometry geometry)
            => throw new NotImplementedException();

        public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
