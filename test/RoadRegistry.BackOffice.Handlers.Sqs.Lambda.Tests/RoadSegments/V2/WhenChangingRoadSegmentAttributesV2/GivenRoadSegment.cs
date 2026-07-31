namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenChangingRoadSegmentAttributesV2;

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
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentAttributes;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
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
    private static readonly OrganizationId MaintenanceAuthorityCode = new("AWV114");
    private static readonly OrganizationId MaintenanceAuthorityOvoCode = new("OVO002949");
    private static readonly StreetNameLocalId StreetNameId = new(123);

    private readonly RoadNetworkTestDataV2 _testData = new();

    public GivenRoadSegment(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenChangingMorphology_ThenTicketCompletedWithSummary()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);
        var completedResults = CaptureCompletedResults();

        await HandleRequest(CreateSqsRequest(morphology: OtherMorphology()), store, roadNetworkRepository);

        completedResults.Should().ContainSingle()
            .Which.Summary.RoadSegments.Modified.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenTheSameRequestIsHandledTwice_ThenTheSummaryIsReturnedAgain()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);
        var completedResults = CaptureCompletedResults();

        // The second attempt is skipped by the idempotent session, so the summary can only come from the persisted
        // scoped road network - which is exactly what the two-step reload is there for.
        var sqsRequest = CreateSqsRequest(morphology: OtherMorphology());
        await HandleRequest(sqsRequest, store, roadNetworkRepository);
        await HandleRequest(sqsRequest, store, roadNetworkRepository);

        completedResults.Should().HaveCount(2);
        completedResults[1].Summary.RoadSegments.Modified.Should()
            .BeEquivalentTo(completedResults[0].Summary.RoadSegments.Modified);
    }

    [Fact]
    public async Task WhenStreetNameIsNotProposedOrCurrent_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(streetNameId: StreetNameId), store, roadNetworkRepository,
            streetNames: _ => KnownStreetName(StreetNameStatus.Retired));

        VerifyThatTicketHasError("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", null);
    }

    [Fact]
    public async Task WhenStreetNameDoesNotExist_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(streetNameId: StreetNameId), store, roadNetworkRepository,
            streetNames: _ => null);

        VerifyThatTicketHasError("StraatnaamNietGekend", null);
    }

    [Fact]
    public async Task WhenStreetNameIsNotApplicable_ThenTheStreetNameRegistryIsNotCalled()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        // Only actual street name identifiers are validated; the -8/-9 placeholders are not known to the registry.
        var dependencies = await HandleRequest(CreateSqsRequest(streetNameId: StreetNameLocalId.NotApplicable), store, roadNetworkRepository);

        dependencies.StreetNameClient.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        roadSegment!.Attributes!.StreetNameId.Values.Should().OnlyContain(x => x.Value == StreetNameLocalId.NotApplicable);
    }

    [Fact]
    public async Task WhenMaintenanceAuthorityIsNotKnown_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(maintenanceAuthority: MaintenanceAuthorityCode), store, roadNetworkRepository,
            organizations: _ => null);

        VerifyThatTicketHasError("WegbeheerderNietGekend", null);
    }

    [Fact]
    public async Task WhenMaintenanceAuthorityIsGivenAsOvoCode_ThenTheOrganizationCodeIsStored()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        // The request may carry an OVO code; the organization cache resolves it and the code must end up on the segment.
        await HandleRequest(CreateSqsRequest(maintenanceAuthority: MaintenanceAuthorityOvoCode), store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        roadSegment!.Attributes!.MaintenanceAuthorityId.Values
            .Should().OnlyContain(x => x.Value == MaintenanceAuthorityCode);
    }

    [Fact]
    public async Task WhenMaintenanceAuthorityDiffersPerSide_ThenEachValueIsResolvedOnItsOwn()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        var otherCode = new OrganizationId("AWV999");

        await HandleRequest(
            CreateSqsRequest(
                maintenanceAuthority: MaintenanceAuthorityOvoCode,
                maintenanceAuthoritySide: RoadSegmentAttributeSide.Links,
                secondMaintenanceAuthority: otherCode,
                secondMaintenanceAuthoritySide: RoadSegmentAttributeSide.Rechts),
            store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        var values = roadSegment!.Attributes!.MaintenanceAuthorityId.Values;
        values.Should().Contain(x => x.Side == RoadSegmentAttributeSide.Links && x.Value == MaintenanceAuthorityCode);
        values.Should().Contain(x => x.Side == RoadSegmentAttributeSide.Rechts && x.Value == otherCode);
    }

    [Fact]
    public async Task WhenStreetNameIsGivenForOneSide_ThenOnlyThatSideIsChanged()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(streetNameId: StreetNameId, streetNameSide: RoadSegmentAttributeSide.Links),
            store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        var values = roadSegment!.Attributes!.StreetNameId.Values;
        values.Should().Contain(x => x.Side == RoadSegmentAttributeSide.Links && x.Value == StreetNameId);
        values.Should().NotContain(x => x.Side == RoadSegmentAttributeSide.Rechts && x.Value == StreetNameId);
    }

    [Fact]
    public async Task WhenNoToPositionIsGiven_ThenTheAttributeCoversTheWholeSegment()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        // A null totPositie means "the end of that particular segment", resolved against the segment's own length.
        await HandleRequest(CreateSqsRequest(morphology: OtherMorphology()), store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        var expectedLength = roadSegment!.Geometry.Value.Length;
        roadSegment.Attributes!.Morphology.Values.Should().ContainSingle()
            // positions are rounded to cm
            .Which.Coverage.To.ToDouble().Should().BeApproximately(expectedLength, 0.01);
    }

    [Fact]
    public async Task WhenNoFromPositionIsGiven_ThenTheAttributeStartsAtZero()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        // A null vanPositie means the start of the segment; the tail keeps the original morphology up to the end.
        var morphology = OtherMorphology();
        await HandleRequest(
            CreateSqsRequest(morphologyValues:
            [
                new AttributeValue<RoadSegmentMorphologyV2>(null, new RoadSegmentPositionV2(50), morphology),
                new AttributeValue<RoadSegmentMorphologyV2>(new RoadSegmentPositionV2(50), null, CurrentMorphology())
            ]),
            store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        var changed = roadSegment!.Attributes!.Morphology.Values.Should()
            .ContainSingle(x => x.Value == morphology).Subject;
        changed.Coverage.From.ToDouble().Should().Be(0);
        changed.Coverage.To.ToDouble().Should().Be(50);
    }

    [Fact]
    public async Task WhenPositionsAreGiven_ThenTheyAreUsedAsIs()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        var morphology = OtherMorphology();
        await HandleRequest(
            CreateSqsRequest(morphologyValues:
            [
                new AttributeValue<RoadSegmentMorphologyV2>(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(10), CurrentMorphology()),
                new AttributeValue<RoadSegmentMorphologyV2>(new RoadSegmentPositionV2(10), new RoadSegmentPositionV2(50), morphology),
                new AttributeValue<RoadSegmentMorphologyV2>(new RoadSegmentPositionV2(50), null, CurrentMorphology())
            ]),
            store, roadNetworkRepository);

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        var values = roadSegment!.Attributes!.Morphology.Values;

        var changed = values.Should().ContainSingle(x => x.Value == morphology).Subject;
        changed.Coverage.From.ToDouble().Should().Be(10);
        changed.Coverage.To.ToDouble().Should().Be(50);

        // the trailing null totPositie still resolves to the end of the segment
        values.Max(x => x.Coverage.To.ToDouble()).Should()
            .BeApproximately(roadSegment.Geometry.Value.Length, 0.01);
    }

    private RoadSegmentMorphologyV2 CurrentMorphology()
    {
        return RoadSegment.Create(_testData.Segment1Added).Attributes!.Morphology.Values.First().Value;
    }

    private RoadSegmentMorphologyV2 OtherMorphology()
    {
        var current = CurrentMorphology();
        return RoadSegmentMorphologyV2.All.First(x => x != current);
    }

    private static StreetNameItem KnownStreetName(string status)
    {
        return new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = status, NisCode = "11001" };
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

    private ChangeRoadSegmentAttributesV2SqsRequest CreateSqsRequest(
        RoadSegmentMorphologyV2 morphology = null,
        AttributeValue<RoadSegmentMorphologyV2>[] morphologyValues = null,
        StreetNameLocalId? streetNameId = null,
        RoadSegmentAttributeSide streetNameSide = null,
        OrganizationId? maintenanceAuthority = null,
        RoadSegmentAttributeSide maintenanceAuthoritySide = null,
        OrganizationId? secondMaintenanceAuthority = null,
        RoadSegmentAttributeSide secondMaintenanceAuthoritySide = null,
        RoadSegmentPositionV2? fromPosition = null,
        RoadSegmentPositionV2? toPosition = null)
    {
        SidedAttributeValue<OrganizationId>[] maintenanceAuthorities = maintenanceAuthority is not null
            ?
            [
                new SidedAttributeValue<OrganizationId>(maintenanceAuthoritySide ?? RoadSegmentAttributeSide.Beide, fromPosition, toPosition, maintenanceAuthority.Value),
                ..secondMaintenanceAuthority is not null
                    ? new[] { new SidedAttributeValue<OrganizationId>(secondMaintenanceAuthoritySide ?? RoadSegmentAttributeSide.Beide, fromPosition, toPosition, secondMaintenanceAuthority.Value) }
                    : []
            ]
            : null;

        return new ChangeRoadSegmentAttributesV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            Groups =
            [
                new ChangeRoadSegmentAttributesV2Group
                {
                    RoadSegmentIds = [_testData.Segment1Added.RoadSegmentId],
                    Morphology = morphologyValues ?? (morphology is not null
                        ? [new AttributeValue<RoadSegmentMorphologyV2>(fromPosition, toPosition, morphology)]
                        : null),
                    StreetName = streetNameId is not null
                        ? [new SidedAttributeValue<StreetNameLocalId>(streetNameSide ?? RoadSegmentAttributeSide.Beide, fromPosition, toPosition, streetNameId.Value)]
                        : null,
                    MaintenanceAuthority = maintenanceAuthorities
                }
            ]
        };
    }

    private async Task<Dependencies> HandleRequest(
        ChangeRoadSegmentAttributesV2SqsRequest sqsRequest,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        Func<int, StreetNameItem> streetNames = null,
        Func<OrganizationId, OrganizationDetail> organizations = null)
    {
        streetNames ??= _ => KnownStreetName(StreetNameStatus.Current);
        // Any code resolves to the known organization code, which is what makes an OVO code end up as its code.
        organizations ??= id => OrganizationDetail.FromCode(id == MaintenanceAuthorityOvoCode ? MaintenanceAuthorityCode : id);

        var organizationCache = new Mock<IOrganizationCache>();
        organizationCache
            .Setup(x => x.FindByIdOrOvoCodeOrKboNumberAsync(It.IsAny<OrganizationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationId id, CancellationToken _) => organizations(id));

        var streetNameClient = new Mock<IStreetNameClient>();
        streetNameClient
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => streetNames(id));

        var handler = new ChangeRoadSegmentAttributesV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            store,
            roadNetworkRepository,
            organizationCache.Object,
            streetNameClient.Object,
            LoggerFactory);

        await handler.Handle(new ChangeRoadSegmentAttributesV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest), CancellationToken.None);

        return new Dependencies(organizationCache, streetNameClient);
    }

    private sealed record Dependencies(Mock<IOrganizationCache> OrganizationCache, Mock<IStreetNameClient> StreetNameClient);

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
