namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork;

using Actions.ChangeRoadNetwork;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using FluentAssertions;
using Hosts;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.FeatureCompare.V3;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using ScopedRoadNetwork;
using Sqs.RoadNetwork;
using Tests.AggregateTests;
using Tests.BackOffice;
using Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public abstract class RoadNetworkIntegrationTest : IClassFixture<DatabaseFixture>
{
    protected readonly ITestOutputHelper TestOutputHelper;
    protected readonly Mock<ITicketing> TicketingMock = new();
    protected readonly RoadNetworkTestData TestData;

    private readonly DatabaseFixture _databaseFixture;

    protected RoadNetworkIntegrationTest(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        TestOutputHelper = testOutputHelper;
        TestData = new();
    }

    protected async Task Given(IServiceProvider sp, TranslatedChanges changes)
    {
        var fixture = TestData.Fixture;

        var provenanceData = new RoadRegistryProvenanceData();
        var sqsRequest = new ChangeRoadNetworkSqsRequest
        {
            DownloadId = fixture.Create<DownloadId>(),
            TicketId = fixture.Create<TicketId>(),
            Changes = changes.Select(ChangeRoadNetworkItem.Create).ToList(),
            ProvenanceData = provenanceData
        };

        var handler = sp.GetRequiredService<ChangeRoadNetworkSqsLambdaRequestHandler>();
        await handler.Handle(new ChangeRoadNetworkSqsLambdaRequest(string.Empty, sqsRequest), CancellationToken.None);

        PrintTicketErrorsIfAvailable(sqsRequest.TicketId);

        var ticketResult = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Complete) && x.Arguments[0].Equals(sqsRequest.TicketId))
            .Select(x => (TicketResult)x.Arguments[1])
            .SingleOrDefault();
        ticketResult.Should().NotBeNull();
    }

    protected async Task<IServiceProvider> BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", await _databaseFixture.CreateDatabase())
            ])
            .Build();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        services
            .AddSingleton<SqsLambdaHandlerOptions>(new FakeSqsLambdaHandlerOptions())
            .AddSingleton<ICustomRetryPolicy>(new FakeRetryPolicy())
            .AddSingleton(TicketingMock.Object)
            .AddSingleton(Mock.Of<IIdempotentCommandHandler>())
            .AddSingleton(Mock.Of<IRoadRegistryContext>())
            .AddSingleton(Mock.Of<IExtractUploadFailedEmailClient>());

        services
            .AddMartenRoad(options => options.AddRoadNetworkTopologyProjection().AddRoadAggregatesSnapshots())
            .AddSingleton<IRoadNetworkIdGenerator>(new FakeRoadNetworkIdGenerator())
            .AddScoped<ChangeRoadNetworkSqsLambdaRequestHandler>();

        ConfigureServices(services);

        var sp = services.BuildServiceProvider();

        // force create marten schema
        var store = sp.GetRequiredService<IDocumentStore>();
        await EnsureRoadNetworkTopologyProjectionExists(store);

        return sp;
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }

    private static async Task EnsureRoadNetworkTopologyProjectionExists(IDocumentStore store)
    {
        // build projection tables
        await store.BuildProjectionDaemonAsync();
    }

    protected void PrintTicketErrorsIfAvailable(Guid ticketId)
    {
        var ticketError = TicketingMock.Invocations
            .Where(x => x.Method.Name == nameof(ITicketing.Error) && x.Arguments[0].Equals(ticketId))
            .Select(x => (TicketError)x.Arguments[1])
            .SingleOrDefault();
        if (ticketError is not null)
        {
            TestOutputHelper.WriteLine("Ticket errors:");
            foreach (var error in ticketError.Errors!)
            {
                TestOutputHelper.WriteLine($"{error.ErrorCode}: {error.ErrorMessage}");
            }
        }
    }

    protected RoadSegmentGeometry BuildSegmentGeometry(int x1, int y1, int x2, int y2)
    {
        return BuildSegmentGeometry(new Point(x1, y1), new Point(x2, y2));
    }
    protected RoadSegmentGeometry BuildSegmentGeometry(Point start, Point end)
    {
        return new MultiLineString([new LineString([start.Coordinate, end.Coordinate])])
            .WithMeasureOrdinates()
            .ToRoadSegmentGeometry();
    }
}
