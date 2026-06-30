namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.RoadSegments.V2.WhenCreatingOutlineV2;

using Autofac;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Marten;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.CreateRoadSegmentOutlineV2;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.StreetName;
using RoadRegistry.Tests.Framework;
using Xunit.Abstractions;

[Collection("runsequential")]
public class GivenOrganizationExists : BackOfficeLambdaTest
{
    private const int StreetNameId = 71671;
    private const double GeometryLength = 10;

    public GivenOrganizationExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenValidRequest_ThenTicketIsCompleted()
    {
        var streetNameClientMock = new Mock<IStreetNameClient>();
        streetNameClientMock
            .Setup(x => x.GetAsync(StreetNameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = StreetNameStatus.Current, NisCode = "11001" });

        ScopedRoadNetwork? savedNetwork = null;
        var repositoryMock = new Mock<IRoadNetworkRepository>();
        repositoryMock
            .Setup(x => x.Save(It.IsAny<ScopedRoadNetwork>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<ScopedRoadNetwork, string, CancellationToken>((network, _, _) => savedNetwork = network)
            .Returns(Task.CompletedTask);

        var extractsDbContext = CreateExtractsDbContextWithCompletedZone();
        var sqsRequest = CreateSqsRequest();
        var sqsLambdaRequest = new CreateRoadSegmentOutlineV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        await HandleRequest(sqsLambdaRequest, repositoryMock.Object, streetNameClientMock.Object, extractsDbContext);

        var savedSegment = savedNetwork!.RoadSegments.Values.Single();
        VerifyThatTicketHasCompleted(TicketingMock, "/v3/wegsegmenten/1", savedSegment.LastEventHash);
    }

    [Fact]
    public async Task WhenStreetNameNotFound_ThenTicketError()
    {
        var streetNameClientMock = new Mock<IStreetNameClient>();
        streetNameClientMock
            .Setup(x => x.GetAsync(StreetNameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StreetNameItem?)null);

        var extractsDbContext = CreateExtractsDbContextWithCompletedZone();
        var sqsRequest = CreateSqsRequest();
        var sqsLambdaRequest = new CreateRoadSegmentOutlineV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        await HandleRequest(sqsLambdaRequest, new Mock<IRoadNetworkRepository>().Object, streetNameClientMock.Object, extractsDbContext);

        VerifyThatTicketHasError("StraatnaamNietGekend", "De straatnaam is niet gekend in het Straatnamenregister.");
    }

    [Fact]
    public async Task WhenStreetNameNotProposedOrCurrent_ThenTicketError()
    {
        var streetNameClientMock = new Mock<IStreetNameClient>();
        streetNameClientMock
            .Setup(x => x.GetAsync(StreetNameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = StreetNameStatus.Retired, NisCode = "11001" });

        var extractsDbContext = CreateExtractsDbContextWithCompletedZone();
        var sqsRequest = CreateSqsRequest();
        var sqsLambdaRequest = new CreateRoadSegmentOutlineV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        await HandleRequest(sqsLambdaRequest, new Mock<IRoadNetworkRepository>().Object, streetNameClientMock.Object, extractsDbContext);

        VerifyThatTicketHasError("WegsegmentStraatnaamNietVoorgesteldOfInGebruik", "Deze actie is enkel toegelaten voor straatnamen met status 'voorgesteld' of 'in gebruik'.");
    }

    [Fact]
    public async Task WhenGeometryNotWithinCompletedInwinningszone_ThenTicketError()
    {
        var streetNameClientMock = new Mock<IStreetNameClient>();
        streetNameClientMock
            .Setup(x => x.GetAsync(StreetNameId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StreetNameItem { Id = StreetNameId, Name = "Teststraat", Status = StreetNameStatus.Current, NisCode = "11001" });

        var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext(); // no completed zones

        var sqsRequest = CreateSqsRequest();
        var sqsLambdaRequest = new CreateRoadSegmentOutlineV2SqsLambdaRequest(Guid.NewGuid().ToString(), sqsRequest);

        await HandleRequest(sqsLambdaRequest, new Mock<IRoadNetworkRepository>().Object, streetNameClientMock.Object, extractsDbContext);

        VerifyThatTicketHasError("RoadSegmentOutsideInwinningszone", "Het wegsegment valt niet volledig binnen een gemeente die de inwinningsstatus 'compleet' heeft.");
    }

    private static ExtractsDbContext CreateExtractsDbContextWithCompletedZone()
    {
        var db = new FakeExtractsDbContextFactory().CreateDbContext();
        db.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = "11001",
            Operator = "op",
            DownloadId = Guid.NewGuid(),
            Contour = new Polygon(new LinearRing([
                new Coordinate(-1000, -1000),
                new Coordinate(1000, -1000),
                new Coordinate(1000, 1000),
                new Coordinate(-1000, 1000),
                new Coordinate(-1000, -1000)
            ])),
            Completed = true
        });
        db.SaveChanges();
        return db;
    }

    private CreateRoadSegmentOutlineV2SqsRequest CreateSqsRequest()
    {
        var line = new LineString(
            new CoordinateArraySequence([new CoordinateM(0, 0, 0), new CoordinateM(GeometryLength, 0, GeometryLength)]),
            GeometryConfiguration.GeometryFactory);
        var geometry = line.ToMultiLineString().ToRoadSegmentGeometry();

        return new CreateRoadSegmentOutlineV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = ObjectProvider.Create<ProvenanceData>(),
            RoadSegmentId = new (1),
            Geometry = geometry,
            Status = RoadSegmentStatusV2.Gepland,
            Morphology =
            [
                new ChangeRoadSegmentMorphologyAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    Morphology = RoadSegmentMorphologyV2.Parallelweg
                }
            ],
            SurfaceType =
            [
                new ChangeRoadSegmentSurfaceTypeAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    SurfaceType = RoadSegmentSurfaceTypeV2.Verhard
                }
            ],
            AccessRestriction =
            [
                new ChangeRoadSegmentAccessRestrictionAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    AccessRestriction = RoadSegmentAccessRestrictionV2.OpenbareWeg
                }
            ],
            StreetNameId =
            [
                new ChangeRoadSegmentStreetNameIdAttributeValue
                {
                    Side = RoadSegmentAttributeSide.Both,
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    StreetNameId = new StreetNameLocalId(StreetNameId)
                }
            ],
            MaintenanceAuthorityId =
            [
                new ChangeRoadSegmentMaintenanceAuthorityIdAttributeValue
                {
                    Side = RoadSegmentAttributeSide.Both,
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    MaintenanceAuthorityId = new OrganizationId("AWV")
                }
            ],
            Category =
            [
                new ChangeRoadSegmentCategoryAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    Category = RoadSegmentCategoryV2.RegionaleWeg
                }
            ],
            CarTrafficDirection =
            [
                new ChangeRoadSegmentCarTrafficDirectionAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    TrafficDirection = RoadSegmentTrafficDirection.Forward
                }
            ],
            BikeTrafficDirection =
            [
                new ChangeRoadSegmentBikeTrafficDirectionAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    TrafficDirection = RoadSegmentTrafficDirection.Both
                }
            ],
            PedestrianTrafficDirection =
            [
                new ChangeRoadSegmentPedestrianTrafficDirectionAttributeValue
                {
                    FromPosition = RoadSegmentPositionV2.Zero,
                    ToPosition = new RoadSegmentPositionV2(GeometryLength),
                    TrafficDirection = RoadSegmentPedestrianTrafficDirection.Both
                }
            ]
        };
    }

    private async Task HandleRequest(
        CreateRoadSegmentOutlineV2SqsLambdaRequest sqsLambdaRequest,
        IRoadNetworkRepository roadNetworkRepository,
        IStreetNameClient streetNameClient,
        ExtractsDbContext extractsDbContext)
    {
        var handler = new CreateRoadSegmentOutlineV2SqsLambdaRequestHandler(
            SqsLambdaHandlerOptions,
            new FakeRetryPolicy(),
            TicketingMock.Object,
            ScopedContainer.Resolve<IIdempotentCommandHandler>(),
            new Mock<IDocumentStore>().Object,
            roadNetworkRepository,
            OrganizationCache,
            streetNameClient,
            extractsDbContext,
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
}
