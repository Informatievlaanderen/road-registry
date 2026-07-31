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

        ChangeRoadNetworkTicketResult completedResult = null;
        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
                completedResult = JsonConvert.DeserializeObject<ChangeRoadNetworkTicketResult>(result.ResultAsJson!));

        await HandleRequest(CreateSqsRequest(morphology: OtherMorphology()), store, roadNetworkRepository);

        completedResult.Should().NotBeNull();
        completedResult.Summary.RoadSegments.Modified.Should().ContainSingle();
    }

    [Fact]
    public async Task WhenStreetNameIsNotProposedOrCurrent_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        var sqsRequest = CreateSqsRequest(streetNameId: StreetNameId);

        await HandleRequest(sqsRequest, store, roadNetworkRepository,
            streetName: new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = StreetNameStatus.Retired, NisCode = "11001" });

        VerifyThatTicketHasError("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", null);
    }

    [Fact]
    public async Task WhenStreetNameDoesNotExist_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(streetNameId: StreetNameId), store, roadNetworkRepository, streetNameExists: false);

        VerifyThatTicketHasError("StraatnaamNietGekend", null);
    }

    [Fact]
    public async Task WhenMaintenanceAuthorityIsNotKnown_ThenTicketError()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        await HandleRequest(CreateSqsRequest(maintenanceAuthority: MaintenanceAuthorityCode), store, roadNetworkRepository, organizationExists: false);

        VerifyThatTicketHasError("WegbeheerderNietGekend", null);
    }

    [Fact]
    public async Task WhenMaintenanceAuthorityIsGivenAsOvoCode_ThenTheOrganizationCodeIsStored()
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());
        var roadNetworkRepository = BuildRepository(store);

        // The request may carry an OVO code; the organization cache resolves it and the code must end up on the segment.
        var ovoCode = new OrganizationId("OVO002949");

        await HandleRequest(CreateSqsRequest(maintenanceAuthority: ovoCode), store, roadNetworkRepository,
            organization: OrganizationDetail.FromCode(MaintenanceAuthorityCode));

        var roadSegment = await store.LoadAsync(_testData.Segment1Added.RoadSegmentId, CancellationToken.None);
        roadSegment!.Attributes!.MaintenanceAuthorityId.Values
            .Should().OnlyContain(x => x.Value == MaintenanceAuthorityCode);
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

    private RoadSegmentMorphologyV2 OtherMorphology()
    {
        var current = RoadSegment.Create(_testData.Segment1Added).Attributes!.Morphology.Values.First().Value;
        return RoadSegmentMorphologyV2.All.First(x => x != current);
    }

    private ChangeRoadSegmentAttributesV2SqsRequest CreateSqsRequest(
        RoadSegmentMorphologyV2 morphology = null,
        StreetNameLocalId? streetNameId = null,
        OrganizationId? maintenanceAuthority = null)
    {
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
                    Morphology = morphology is not null
                        ? [new AttributeValue<RoadSegmentMorphologyV2>(null, null, morphology)]
                        : null,
                    StreetName = streetNameId is not null
                        ? [new SidedAttributeValue<StreetNameLocalId>(RoadSegmentAttributeSide.Beide, null, null, streetNameId.Value)]
                        : null,
                    MaintenanceAuthority = maintenanceAuthority is not null
                        ? [new SidedAttributeValue<OrganizationId>(RoadSegmentAttributeSide.Beide, null, null, maintenanceAuthority.Value)]
                        : null
                }
            ]
        };
    }

    private async Task HandleRequest(
        ChangeRoadSegmentAttributesV2SqsRequest sqsRequest,
        IDocumentStore store,
        IRoadNetworkRepository roadNetworkRepository,
        StreetNameItem streetName = null,
        OrganizationDetail organization = null,
        bool streetNameExists = true,
        bool organizationExists = true)
    {
        streetName ??= new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = StreetNameStatus.Current, NisCode = "11001" };
        organization ??= OrganizationDetail.FromCode(MaintenanceAuthorityCode);

        var organizationCache = new Mock<IOrganizationCache>();
        organizationCache
            .Setup(x => x.FindByIdOrOvoCodeOrKboNumberAsync(It.IsAny<OrganizationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(organizationExists ? organization : null);

        var streetNameClient = new Mock<IStreetNameClient>();
        streetNameClient
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(streetNameExists ? streetName : null);

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
