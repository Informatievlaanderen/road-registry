namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentGeometryDrawMethodV2;

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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometryDrawMethod;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
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
    public async Task WhenTheRoadSegmentInwinningIsNotCompleted_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(OtherDrawMethod()), store, roadNetworkRepository,
            extractsDbContext: ExtractsDbContextWithInwinning(completed: false, _testData.Segment1Added.RoadSegmentId.ToInt32()));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet", null);
    }

    [Fact]
    public async Task WhenTheRoadSegmentIsPartOfNoInwinningAtAll_ThenTicketError()
    {
        // 'nietGestart' is not 'compleet' either: a segment nobody has collected yet may not be edited.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(OtherDrawMethod()), store, roadNetworkRepository,
            extractsDbContext: new FakeExtractsDbContextFactory().CreateDbContext());

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet", null);
    }

    [Fact]
    public async Task WhenTheRoadSegmentDoesNotExist_ThenTicketErrorSaysSoRatherThanReportingItsInwinning()
    {
        // Nothing is being collected here at all, so an unknown identifier would read as 'nietGestart' and answer a
        // missing road segment with an inwinning problem. Whether it exists is the road network's to say.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(OtherDrawMethod(), roadSegmentId: new RoadSegmentId(999)), store, roadNetworkRepository,
            extractsDbContext: new FakeExtractsDbContextFactory().CreateDbContext());

        VerifyThatTicketHasError("NotFound", "Het wegsegment met id 999 bestaat niet.");
    }

    [Fact]
    public async Task WhenChangingGeometryDrawMethod_ThenTicketCompletedWithSummary()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);
        var completedResults = CaptureCompletedResults();

        await HandleRequest(CreateSqsRequest(OtherDrawMethod()), store, roadNetworkRepository);

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Modified.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenChangingGeometryDrawMethod_ThenTheSegmentCarriesTheNewDrawMethod()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        var geometryDrawMethod = OtherDrawMethod();
        await HandleRequest(CreateSqsRequest(geometryDrawMethod), store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        roadSegment!.Attributes!.GeometryDrawMethod.Should().Be(geometryDrawMethod);
    }

    [Fact]
    public async Task WhenTheSameRequestIsHandledTwice_ThenTheSummaryIsReturnedAgain()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);
        var completedResults = CaptureCompletedResults();

        // The second attempt is skipped by the idempotent session, so the summary can only come from the persisted
        // scoped road network - which is exactly what the two-step reload is there for.
        var sqsRequest = CreateSqsRequest(OtherDrawMethod());
        await HandleRequest(sqsRequest, store, roadNetworkRepository);
        await HandleRequest(sqsRequest, store, roadNetworkRepository);

        completedResults.Should().HaveCount(2);
        completedResults[1].Summary.RoadSegments.Modified.Should()
            .BeEquivalentTo(completedResults[0].Summary.RoadSegments.Modified);
    }

    private RoadSegmentGeometryDrawMethodV2 CurrentDrawMethod()
    {
        return RoadSegment.Create(_testData.Segment1Added).Attributes!.GeometryDrawMethod;
    }

    private RoadSegmentGeometryDrawMethodV2 OtherDrawMethod()
    {
        var current = CurrentDrawMethod();
        return RoadSegmentGeometryDrawMethodV2.All.First(x => x != current);
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

    private ChangeRoadSegmentGeometryDrawMethodV2SqsRequest CreateSqsRequest(
        RoadSegmentGeometryDrawMethodV2 geometryDrawMethod,
        RoadSegmentId? roadSegmentId = null)
    {
        return new ChangeRoadSegmentGeometryDrawMethodV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            Groups =
            [
                new ChangeRoadSegmentGeometryDrawMethodV2Group
                {
                    RoadSegmentIds = [roadSegmentId ?? _testData.Segment1Added.RoadSegmentId],
                    GeometryDrawMethod = geometryDrawMethod
                }
            ]
        };
    }

    private async Task HandleRequest(
        ChangeRoadSegmentGeometryDrawMethodV2SqsRequest sqsRequest,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        ExtractsDbContext extractsDbContext = null)
    {
        var handler = new ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            roadNetworkRepository,
            extractsDbContext ?? ExtractsDbContextWithCompletedInwinning(_testData.Segment1Added.RoadSegmentId.ToInt32()),
            LoggerFactory);

        await handler.Handle(new ChangeRoadSegmentGeometryDrawMethodV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);
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

    private static ExtractsDbContext ExtractsDbContextWithCompletedInwinning(params int[] roadSegmentIds)
    {
        return ExtractsDbContextWithInwinning(completed: true, roadSegmentIds);
    }

    private static ExtractsDbContext ExtractsDbContextWithInwinning(bool completed, params int[] roadSegmentIds)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        foreach (var roadSegmentId in roadSegmentIds)
        {
            db.InwinningRoadSegments.Add(new InwinningRoadSegment
            {
                NisCode = "11001",
                RoadSegmentId = roadSegmentId,
                Completed = completed
            });
        }
        db.SaveChanges();
        return db;
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
