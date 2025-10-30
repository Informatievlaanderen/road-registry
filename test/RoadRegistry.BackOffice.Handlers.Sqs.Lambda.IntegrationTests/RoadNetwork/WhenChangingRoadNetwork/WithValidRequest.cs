namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.WhenChangingRoadNetwork;

using AutoFixture;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using CommandHandling;
using CommandHandling.Actions.ChangeRoadNetwork;
using Core;
using Handlers.RoadNetwork;
using Hosts;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.Geometries;
using Requests.RoadNetwork;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNetwork.Changes;
using RoadSegment.ValueObjects;
using Sqs.RoadNetwork;
using Tests.BackOffice;
using Tests.BackOffice.Scenarios;
using Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using GradeSeparatedJunction = GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadNode.RoadNode;
using RoadSegment = RoadSegment.RoadSegment;

[Collection("DockerFixtureCollection")]
public class WithValidRequest : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    private readonly Mock<ITicketing> _ticketingMock = new();
    private readonly Mock<IExtractRequests> _extractRequestsMock = new();

    public WithValidRequest(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        //TODO-pr seed marten topology projection met data + start lambda function met command + verify ticketing status + verify data in marten

        // Arrange
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", _databaseFixture.ConnectionString)
            ])
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(_ticketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(_extractRequestsMock.Object);

        services
            .AddMartenRoadNetworkRepository()
            .AddSingleton<Core.IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddChangeRoadNetworkCommandHandler()
            .AddScoped<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();

        var sp = services.BuildServiceProvider();

        // force create marten schema
        {
            var store = sp.GetRequiredService<IDocumentStore>();
            await EnsureTopologyProjectionDatabaseTablesExist(store);
            await SeedNetwork(store);
        }

        var sqsRequest = new ChangeRoadNetworkCommandSqsRequest
        {
            Request = new ChangeRoadNetworkCommand(),
            ProvenanceData = new RoadRegistryProvenanceData()
        };

        // Act
        var handler = sp.GetRequiredService<ChangeRoadNetworkCommandSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkCommandSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        // Assert

    }

    private static async Task EnsureTopologyProjectionDatabaseTablesExist(IDocumentStore store)
    {
        //build projection tables
        var projectionDaemon = await store.BuildProjectionDaemonAsync();

        // await projectionDaemon.RebuildProjectionAsync<RoadSegment>(CancellationToken.None);
        // await projectionDaemon.RebuildProjectionAsync<RoadNode>(CancellationToken.None);
        // await projectionDaemon.RebuildProjectionAsync<GradeSeparatedJunction>(CancellationToken.None);
    }

    private static async Task SeedNetwork(IDocumentStore store)
    {
        var seedRoadNetwork = RoadRegistry.RoadNetwork.RoadNetwork.Empty;
        var repository = new RoadNetworkRepository(store);

        var nodeGeometry1 = new Point(200000, 200000);
        var nodeGeometry2 = new Point(200010, 200000);
        var fixture = new RoadNetworkTestData().ObjectProvider;

        var changes = RoadNetworkChanges.Start()
            .Add(new AddRoadNodeChange
            {
                TemporaryId = new RoadNodeId(1),
                OriginalId = null,
                Geometry = nodeGeometry1,
                Type = RoadNodeType.EndNode
            })
            .Add(new AddRoadNodeChange
            {
                TemporaryId = new RoadNodeId(2),
                OriginalId = null,
                Geometry = nodeGeometry2,
                Type = RoadNodeType.EndNode
            })
            .Add(new AddRoadSegmentChange
            {
                TemporaryId = new RoadSegmentId(1),
                OriginalId = null,
                PermanentId = null,
                StartNodeId = new RoadNodeId(1),
                EndNodeId = new RoadNodeId(2),
                Geometry = new LineString([new Coordinate(nodeGeometry1.X, nodeGeometry1.Y), new Coordinate(nodeGeometry2.X, nodeGeometry2.Y)])
                    .ToMultiLineString()
                    .WithMeasureOrdinates(),
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                AccessRestriction = new RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction>()
                    .Add(fixture.Create<RoadSegmentAccessRestriction>()),
                Category = new RoadSegmentDynamicAttributeValues<RoadSegmentCategory>()
                    .Add(fixture.Create<RoadSegmentCategory>()),
                Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphology>()
                    .Add(fixture.Create<RoadSegmentMorphology>()),
                Status = new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>()
                    .Add(fixture.Create<RoadSegmentStatus>()),
                StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                    .Add(fixture.Create<StreetNameLocalId>()),
                MaintenanceAuthorityId = new RoadSegmentDynamicAttributeValues<OrganizationId>()
                    .Add(fixture.Create<OrganizationId>()),
                SurfaceType = new RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType>()
                    .Add(fixture.Create<RoadSegmentSurfaceType>()),
                EuropeanRoadNumbers = [EuropeanRoadNumber.E19],
                NationalRoadNumbers = [NationalRoadNumber.Parse("N123")]
            });

        var result = seedRoadNetwork.Change(changes, new FakeRoadNetworkIdGenerator());
        if (result.Problems.HasError())
        {
            throw new InvalidOperationException($"Seeding road network failed due to problems:{Environment.NewLine}{string.Join(Environment.NewLine, result.Problems.Select(x => x.Describe()))}");
        }

        await repository.Save(seedRoadNetwork, CancellationToken.None);
    }
}
