namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenRemovingRoadSegmentsV2;

using System.Collections.Generic;
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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.Extracts.Schema;
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
using TicketingService.Abstractions;
using Xunit.Abstractions;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// VAL-9 of GAWR-7731 as the handler has to answer it: a road segment may only be removed once its inwinning is done.
// The aggregate says for itself whether it is a V2 road segment, but not whether a municipality is collecting it
// again - a road segment that was ingewonnen once and is 'locked' now carries its V2 attributes all the same - so the
// handler asks the extracts database, the same as every other editing action does.
[Collection("runsequential")]
public class GivenRoadSegment : BackOfficeLambdaTest
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    public GivenRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenTheInwinningIsNotCompleted_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, ExtractsDbContextWithInwinning(completed: false));

        VerifyThatTicketHasError("WegsegmentInwinningsstatusNietCompleet",
            $"Het wegsegment met id {_testData.Segment1Added.RoadSegmentId} heeft niet de inwinningsstatus 'compleet'.");
    }

    [Fact]
    public async Task WhenTheInwinningIsCompleted_ThenTheRoadSegmentIsRemoved()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store, ExtractsDbContextWithInwinning(completed: true));

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Removed.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenTheV2RoadSegmentIsPartOfNoInwinningAtAll_ThenTheRoadSegmentIsRemoved()
    {
        // A V2 road segment that was never recorded in an inwinning - one of the parts of a split, for instance - has
        // its inwinning done all the same.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var completedResults = CaptureCompletedResults();

        await HandleRequest(store, new FakeExtractsDbContextFactory().CreateDbContext());

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Removed.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenTheRoadSegmentDoesNotExist_ThenTicketErrorSaysSoRatherThanReportingItsInwinning()
    {
        // Nothing is being collected here at all, so an unknown identifier would read as 'nietGestart' and answer a
        // missing road segment with an inwinning problem. Whether it exists is the road network's to say.
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        await HandleRequest(store, new FakeExtractsDbContextFactory().CreateDbContext(), roadSegmentIds: [new RoadSegmentId(999)]);

        VerifyThatTicketHasError("NotFound", "Het wegsegment met id 999 bestaat niet.");
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
        IReadOnlyCollection<RoadSegmentId>? roadSegmentIds = null)
    {
        var handler = new RemoveRoadSegmentsSqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            new FakeRoadNetworkRepository(store, BuildSeedNetwork),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            extractsDbContext,
            LoggerFactory);

        var sqsRequest = new RemoveRoadSegmentsSqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentIds = roadSegmentIds ?? [_testData.Segment1Added.RoadSegmentId]
        };

        await handler.Handle(new RemoveRoadSegmentsSqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);
    }

    private ScopedRoadNetwork BuildSeedNetwork(ScopedRoadNetworkId id)
    {
        // The road nodes carry a type: the test data leaves it off, and a road node without one has not migrated -
        // which the domain refuses before it gets to removing anything.
        return new ScopedRoadNetwork(id,
            [
                RoadNode.Create(_testData.Segment1StartNodeAdded with { Type = RoadNodeTypeV2.Eindknoop }).WithoutChanges(),
                RoadNode.Create(_testData.Segment1EndNodeAdded with { Type = RoadNodeTypeV2.Eindknoop }).WithoutChanges()
            ],
            [RoadSegment.Create(_testData.Segment1Added).WithoutChanges()],
            [],
            []);
    }

    private ExtractsDbContext ExtractsDbContextWithInwinning(bool completed)
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        db.InwinningRoadSegments.Add(new InwinningRoadSegment
        {
            NisCode = "11001",
            RoadSegmentId = _testData.Segment1Added.RoadSegmentId.ToInt32(),
            Completed = completed
        });
        db.SaveChanges();
        return db;
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

    // The scoping is not what these tests are about: the road network the handler gets to see is handed to it.
    private sealed class FakeRoadNetworkRepository : IRoadNetworkRepository
    {
        private readonly RoadNetworkRepository _real;
        private readonly Func<ScopedRoadNetworkId, ScopedRoadNetwork> _loadFactory;

        public FakeRoadNetworkRepository(IDocumentStore store, Func<ScopedRoadNetworkId, ScopedRoadNetwork> loadFactory)
        {
            _real = new RoadNetworkRepository(store);
            _loadFactory = loadFactory;
        }

        public Task<ScopedRoadNetwork> LoadUnderlying(IDocumentSession session, ScopedRoadNetworkId roadNetworkId, Geometry? geometry = null, RoadNetworkIds? ids = null)
            => Load(session, new RoadNetworkIds([], [], [], []), roadNetworkId);

        public Task<ScopedRoadNetwork> LoadWithConnectedSegments(IDocumentSession session, IReadOnlyCollection<RoadSegmentId> roadSegmentIds, ScopedRoadNetworkId roadNetworkId)
            => Load(session, new RoadNetworkIds([], [], [], []), roadNetworkId);

        public Task<ScopedRoadNetwork> Load(IDocumentSession session, RoadNetworkIds ids, ScopedRoadNetworkId roadNetworkId)
            => Task.FromResult(_loadFactory(roadNetworkId));

        public void Save(IDocumentSession session, ScopedRoadNetwork roadNetwork, string commandName)
            => _real.Save(session, roadNetwork, commandName);

        public Task<RoadNetworkExtractDocuments<TRoadNode, TRoadSegment, TGradeSeparatedJunction>> LoadForExtract<TRoadNode, TRoadSegment, TGradeSeparatedJunction>(IDocumentSession session, Geometry geometry)
            where TRoadNode : class
            where TRoadSegment : class
            where TGradeSeparatedJunction : class
            => throw new NotImplementedException();

        public Task Save(ScopedRoadNetwork roadNetwork, string commandName, CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
