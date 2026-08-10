namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadSegments.WhenChangingRoadSegmentGeometryV2;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentGeometry;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.StreetName;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

/// <summary>
/// Runs the change-geometry lambda against whatever database the configuration points at, so a scenario that only
/// shows up on a real network - a new crossing that has to produce a gelijkgrondse kruising, one that disappears and
/// has to take its junction along - can be reproduced without going through the API and the queue.
///
/// This WRITES: the events it produces land in the configured event store just like a real request would. Point it at
/// a test environment.
/// </summary>
public class ForDebugging
{
    private readonly ITestOutputHelper _outputHelper;
    private Mock<ITicketing> TicketingMock { get; } = new();

    public ForDebugging(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task WithCustomRequestOnActualServer()
    {
        // Arrange
        var roadSegmentId = new RoadSegmentId(481110);
        // The new geometry, in Lambert 2008. Draw it so it runs into the segment you expect a junction with.
        var newGeometryWkt = "MULTILINESTRING ((217368 181577, 217400 181600, 217450 181650))";
        var operatorName = "0425258688";
        var mayModifyMeasuredRoadSegments = true;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();

        var newGeometry = ((MultiLineString)new WKTReader().Read(newGeometryWkt))
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();

        var roadSegment = await LoadRoadSegment(store, roadSegmentId);
        _outputHelper.WriteLine($"Current geometry: {roadSegment.Geometry.WKT}");
        _outputHelper.WriteLine($"New geometry:     {newGeometry.WKT}");

        var sqsRequest = BuildRequest(roadSegment, newGeometry, operatorName, mayModifyMeasuredRoadSegments);

        var handler = new ChangeRoadSegmentGeometryV2SqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            store,
            new RoadNetworkRepository(store),
            sp.GetRequiredService<IRoadNetworkIdGenerator>(),
            OrganizationCacheThatKeepsTheCode(),
            StreetNameClientThatAcceptsEverything(),
            extractsDbContext,
            new NullLoggerFactory());

        // Act
        await handler.Handle(new ChangeRoadSegmentGeometryV2SqsLambdaRequest("abc", sqsRequest), CancellationToken.None);

        // Assert - the ticket carries what the change amounted to, junctions included.
        TicketingMock.Verify(x => x.Error(It.IsAny<Guid>(), It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Never);
        TicketingMock.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static async Task<RoadSegment> LoadRoadSegment(IDocumentStore store, RoadSegmentId roadSegmentId)
    {
        await using var session = store.LightweightSession();
        var repository = new RoadNetworkRepository(store);

        var ids = await repository.GetUnderlyingIds(session, ids: new RoadNetworkIds([], [roadSegmentId], [], []));
        var roadNetwork = await repository.Load(session, ids, new ScopedRoadNetworkId(Guid.NewGuid()));

        if (!roadNetwork.RoadSegments.TryGetValue(roadSegmentId, out var roadSegment) || roadSegment.IsRemoved)
        {
            throw new InvalidOperationException($"Road segment {roadSegmentId} was not found.");
        }

        return roadSegment;
    }

    // The endpoint always sends every attribute along, so they are taken from the segment as it stands. Each one is
    // flattened to a single value over the whole new geometry: enough to make the request valid, but do not use this
    // to check what happens to attribute coverages.
    private static ChangeRoadSegmentGeometryV2SqsRequest BuildRequest(
        RoadSegment roadSegment,
        RoadSegmentGeometry newGeometry,
        string operatorName,
        bool mayModifyMeasuredRoadSegments)
    {
        var attributes = roadSegment.Attributes
                         ?? throw new InvalidOperationException($"Road segment {roadSegment.RoadSegmentId} has not been migrated to V2.");

        return new ChangeRoadSegmentGeometryV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = new RoadRegistryProvenanceData(operatorName: operatorName),
            RoadSegmentId = roadSegment.RoadSegmentId,
            Geometry = newGeometry,
            MayModifyMeasuredRoadSegments = mayModifyMeasuredRoadSegments,
            Morphology = FullLength(attributes.Morphology),
            SurfaceType = FullLength(attributes.SurfaceType),
            AccessRestriction = FullLength(attributes.AccessRestriction),
            Category = FullLength(attributes.Category),
            StreetName = FullLengthPerSide(attributes.StreetNameId),
            MaintenanceAuthority = FullLengthPerSide(attributes.MaintenanceAuthorityId),
            CarTrafficDirection = FullLength(attributes.CarTrafficDirection),
            BikeTrafficDirection = FullLength(attributes.BikeTrafficDirection),
            PedestrianTrafficDirection = FullLength(attributes.PedestrianTrafficDirection)
        };
    }

    // A null from/to position resolves to 0 and to the length of the geometry being submitted.
    private static List<AttributeValue<T>> FullLength<T>(RoadSegmentDynamicAttributeValues<T> attribute)
        where T : notnull
    {
        return [new AttributeValue<T>(null, null, attribute.Values[0].Value)];
    }

    private static List<SidedAttributeValue<T>> FullLengthPerSide<T>(RoadSegmentDynamicAttributeValues<T> attribute)
        where T : notnull
    {
        return [new SidedAttributeValue<T>(RoadSegmentAttributeSide.Beide, null, null, attribute.Values[0].Value)];
    }

    // The organization and street name registries are not what is being debugged here, so both are answered locally:
    // the maintenance authority keeps the code it already had, and every street name passes as 'current'.
    private static IOrganizationCache OrganizationCacheThatKeepsTheCode()
    {
        var organizationCache = new Mock<IOrganizationCache>();
        organizationCache
            .Setup(x => x.FindByIdOrOvoCodeOrKboNumberAsync(It.IsAny<OrganizationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationId id, CancellationToken _) => OrganizationDetail.FromCode(id));
        return organizationCache.Object;
    }

    private static IStreetNameClient StreetNameClientThatAcceptsEverything()
    {
        var streetNameClient = new Mock<IStreetNameClient>();
        streetNameClient
            .Setup(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new StreetNameItem
            {
                Id = id,
                Status = StreetNameStatus.Current
            });
        return streetNameClient.Object;
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging()
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(TicketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            ;

        services
            .AddMartenRoad(options => options
                .AddRoadNetworkTopologyProjection()
                .AddRoadAggregatesSnapshots()
                .ConfigureExtractDocuments()).Services
            // Junction ids have to keep counting where the real network left off, so the sequence in the database is
            // what hands them out - an in-memory generator would collide with what is already there.
            .AddRoadNetworkDbIdGenerator()
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            ;

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
