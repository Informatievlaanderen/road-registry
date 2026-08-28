namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadSegments.WhenChangingRoadSegmentStatusV2;

using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
using Marten;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Npgsql;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadSegmentStatus;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework;
using TicketingService.Abstractions;
using Xunit.Abstractions;

/// <summary>
/// Runs the status change lambda against whatever database the configuration points at, so a scenario that only shows
/// up on a real network - a segment being connected that has to snap onto road nodes that are already there, add the
/// ones that are not, and turn the crossings it makes into gelijkgrondse kruisingen - can be reproduced without going
/// through the API and the queue. Set the transition below to the one you are reproducing.
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
        var roadSegmentId = new RoadSegmentId(220324);
        // VAL-9: whether the caller holds the 'ingemeten' scope. Set it the way the caller you are reproducing has it.
        var mayModifyMeasuredRoadSegments = true;
        // Which transition to reproduce; see RoadSegmentStatusChange for the full table.
        var statusChange = RoadSegmentStatusChange.PlannedToRealized;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var extractsDbContext = sp.GetRequiredService<ExtractsDbContext>();

        // The request body is empty, so what the action does follows entirely from the segment as it stands and the
        // network around it. Both are printed here, because they are what a failure has to be explained from.
        await PrintRoadSegmentAndItsScope(store, roadSegmentId);

        var sqsRequest = new ChangeRoadSegmentStatusV2SqsRequest
        {
            TicketId = Guid.NewGuid(),
            Metadata = new Dictionary<string, object?>(),
            ProvenanceData = new RoadRegistryProvenanceData(),
            RoadSegmentId = roadSegmentId,
            StatusChange = statusChange,
            MayModifyMeasuredRoadSegments = mayModifyMeasuredRoadSegments
        };

        // A ticket error is the interesting outcome when something is wrong, so it is written out rather than only
        // counted - the Verify below would otherwise report 'called once' and keep the reason to itself.
        CaptureTicketOutcome();

        var handler = new ChangeRoadSegmentStatusV2SqsLambdaRequestHandler(
            new FakeSqsLambdaHandlerOptions(),
            new FakeRetryPolicy(),
            TicketingMock.Object,
            Mock.Of<IIdempotentCommandHandler>(),
            store,
            new RoadNetworkRepository(store),
            sp.GetRequiredService<IRoadNetworkIdGenerator>(),
            extractsDbContext,
            new NullLoggerFactory());

        // Act
        await handler.Handle(new ChangeRoadSegmentStatusV2SqsLambdaRequest("abc", sqsRequest), CancellationToken.None);

        // Assert - the ticket carries what the change amounted to, road nodes and junctions included.
        TicketingMock.Verify(x => x.Error(It.IsAny<Guid>(), It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Never);
        TicketingMock.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // The scope the handler will work with, resolved the same way it resolves it, so a segment that is missing from it
    // is visible here rather than only as a puzzling domain problem later on.
    private async Task PrintRoadSegmentAndItsScope(IDocumentStore store, RoadSegmentId roadSegmentId)
    {
        await using var session = store.LightweightSession();

        var roadSegment = await session.LoadAsync(roadSegmentId, CancellationToken.None);
        if (roadSegment is null)
        {
            _outputHelper.WriteLine($"Road segment {roadSegmentId} was not found.");
            return;
        }

        _outputHelper.WriteLine($"Road segment: {roadSegmentId}");
        _outputHelper.WriteLine($"  Status:     {roadSegment.Status}");
        _outputHelper.WriteLine($"  StartNode:  {roadSegment.StartNodeId?.ToString() ?? "<none>"}");
        _outputHelper.WriteLine($"  EndNode:    {roadSegment.EndNodeId?.ToString() ?? "<none>"}");
        _outputHelper.WriteLine($"  Removed:    {roadSegment.IsRemoved}");
        _outputHelper.WriteLine($"  Geometry:   {roadSegment.Geometry.WKT}");

        var geometry = roadSegment.Geometry.Value.Buffer(Distances.RoadSegmentRealizeMaximumDistanceToRoadNode + 0.5);
        var ids = await new RoadNetworkRepository(store).GetUnderlyingIds(session, geometry);

        _outputHelper.WriteLine("Scope:");
        _outputHelper.WriteLine($"  RoadNodes:               {string.Join(", ", ids.RoadNodeIds)}");
        _outputHelper.WriteLine($"  RoadSegments:            {string.Join(", ", ids.RoadSegmentIds)}");
        _outputHelper.WriteLine($"  GradeSeparatedJunctions: {string.Join(", ", ids.GradeSeparatedJunctionIds)}");
        _outputHelper.WriteLine($"  GradeJunctions:          {string.Join(", ", ids.GradeJunctionIds)}");
    }

    private void CaptureTicketOutcome()
    {
        TicketingMock
            .Setup(x => x.Error(It.IsAny<Guid>(), It.IsAny<TicketError>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketError, CancellationToken>((_, error, _) =>
            {
                _outputHelper.WriteLine($"Ticket error: {error.ErrorCode} - {error.ErrorMessage}");
                foreach (var innerError in error.Errors ?? [])
                {
                    _outputHelper.WriteLine($"  {innerError.ErrorCode} - {innerError.ErrorMessage}");
                }
            });

        TicketingMock
            .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()))
            .Callback<Guid, TicketResult, CancellationToken>((_, result, _) =>
            {
                var ticketResult = JsonConvert.DeserializeObject<ChangeRoadNetworkTicketResult>(result.ResultAsJson!)!;
                _outputHelper.WriteLine($"Ticket completed: {JsonConvert.SerializeObject(ticketResult.Summary, Formatting.Indented)}");
            });
    }

    // The three servers this is about to hit, without their credentials. 'It ran against the wrong database' is the
    // easiest way for a debugging session to mislead you, and the Events one in particular does not follow the machine
    // file unless BuildConfiguration below puts it back.
    private void PrintTargets(IConfiguration configuration)
    {
        _outputHelper.WriteLine("Targets:");
        _outputHelper.WriteLine($"  Marten (aggregates): {DescribePostgres(configuration.GetConnectionString(WellKnownConnectionNames.Marten))}");
        _outputHelper.WriteLine($"  Events (id sequences): {DescribeSqlServer(configuration.GetConnectionString(WellKnownConnectionNames.Events) ?? configuration.GetConnectionString(WellKnownConnectionNames.RoadRegistryEvents))}");
        _outputHelper.WriteLine($"  Extracts (inwinning): {DescribeSqlServer(configuration.GetConnectionString(WellKnownConnectionNames.Extracts) ?? configuration.GetConnectionString(WellKnownConnectionNames.RoadRegistry))}");
        _outputHelper.WriteLine(string.Empty);
    }

    private static string DescribePostgres(string? connectionString)
    {
        if (connectionString is null)
        {
            return "<not set>";
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{builder.Host}:{builder.Port}/{builder.Database}";
    }

    private static string DescribeSqlServer(string? connectionString)
    {
        if (connectionString is null)
        {
            return "<not set>";
        }

        var builder = new SqlConnectionStringBuilder(connectionString);
        return $"{builder.DataSource}/{builder.InitialCatalog}";
    }

    // appsettings.json is copied into this project's output from a referenced host project, and it pins
    // ConnectionStrings:Events at the local docker-compose SQL Server. The id generator asks for 'Events' first and
    // only falls back to 'RoadRegistryEvents', so a machine-specific file that sets only the latter is never
    // consulted: the new road node and junction ids are drawn from the wrong server, or fail to be drawn at all.
    // Putting the machine value back on top makes the whole test follow one environment.
    private static IConfiguration BuildConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

        var roadRegistryEvents = configuration.GetConnectionString(WellKnownConnectionNames.RoadRegistryEvents);
        if (roadRegistryEvents is null)
        {
            return configuration;
        }

        return new ConfigurationBuilder()
            .AddConfiguration(configuration)
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"ConnectionStrings:{WellKnownConnectionNames.Events}"] = roadRegistryEvents
            })
            .Build();
    }

    private IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var configuration = BuildConfiguration();
        PrintTargets(configuration);

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
            // Road node and junction ids have to keep counting where the real network left off, so the sequence in the
            // database is what hands them out - an in-memory generator would collide with what is already there.
            .AddRoadNetworkDbIdGenerator()
            .AddExtractsDbContext(QueryTrackingBehavior.TrackAll)
            ;

        var sp = services.BuildServiceProvider();
        return sp.CreateScope().ServiceProvider;
    }
}
