namespace RoadRegistry.Projections.IntegrationTests.Infrastructure;

using System.Text;
using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using KellermanSoftware.CompareNetObjects;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RoadNetwork;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using ScopedRoadNetwork;
using ScopedRoadNetwork.Events.V2;
using ScopedRoadNetwork.ValueObjects;
using Tests.Framework.Projections;
using Xunit.Sdk;

public class MartenProjectionIntegrationTestRunner
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ILogger? _logger;
    private readonly List<Action<IServiceCollection>> _servicesConfigurations = [];
    private readonly List<Action<StoreOptions>> _storeConfigurations = [];
    private TimeSpan _projectionWaitTimeout;
    private readonly List<List<(string StreamKey, object Event)>> _givenEvents = [];

    public MartenProjectionIntegrationTestRunner(DatabaseFixture databaseFixture, ILogger? logger = null)
    {
        _databaseFixture = databaseFixture;
        _logger = logger;
        _projectionWaitTimeout = TimeSpan.FromSeconds(10);
    }

    public MartenProjectionIntegrationTestRunner ConfigureServices(Action<IServiceCollection> configure)
    {
        _servicesConfigurations.Add(configure);
        return this;
    }
    public MartenProjectionIntegrationTestRunner ConfigureMarten(Action<StoreOptions> configure)
    {
        _storeConfigurations.Add(configure);
        return this;
    }
    public MartenProjectionIntegrationTestRunner ProjectionWaitTimeout(TimeSpan projectionWaitTimeout)
    {
        _projectionWaitTimeout = projectionWaitTimeout;
        return this;
    }

    public MartenProjectionIntegrationTestRunner Given(string streamKey, params object[] events)
    {
        _givenEvents.Add(events.Select(evt => (streamKey, evt)).ToList());
        return this;
    }
    public MartenProjectionIntegrationTestRunner Given(IEnumerable<(string StreamKey, object Event)> eventsPerStreamKey)
    {
        _givenEvents.Add(eventsPerStreamKey.ToList());
        return this;
    }

    public Task Expect<TProjectionEntity>((string Identifier, TProjectionEntity? Document)[] expectedDocuments)
        where TProjectionEntity : notnull
    {
        return Expect<TProjectionEntity, string>(expectedDocuments);
    }
    public Task Expect<TProjectionEntity>((int Identifier, TProjectionEntity? Document)[] expectedDocuments)
        where TProjectionEntity : notnull
    {
        return Expect<TProjectionEntity, int>(expectedDocuments);
    }
    private Task Expect<TProjectionEntity, TIdentifier>((TIdentifier Identifier, TProjectionEntity? Document)[] expectedDocuments)
        where TProjectionEntity : notnull
        where TIdentifier : notnull
    {
        return Expect(async (_, session) =>
        {
            var actualDocuments = new List<TProjectionEntity?>();
            _logger?.LogInformation("Assert expect");

            foreach (var expectedDocument in expectedDocuments)
            {
                _logger?.LogInformation($"Expected document {expectedDocument.Identifier}: {JsonConvert.SerializeObject(expectedDocument.Document)}");
                var actualDocument = await session.LoadAsync<TProjectionEntity>(expectedDocument.Identifier);
                _logger?.LogInformation($"Actual document {expectedDocument.Identifier}: {JsonConvert.SerializeObject(actualDocument)}");
                actualDocuments.Add(actualDocument);
            }

            CompareResult(expectedDocuments.Select(x => (object?)x.Document).ToArray(), actualDocuments.Cast<object?>().ToArray());
        });
    }

    public void CompareResult(object?[]? expected, object?[]? actual)
    {
        if (expected is null && actual is null)
        {
            return;
        }

        var comparisonConfig = new ComparisonConfig
        {
            MaxDifferences = 10,
            CustomComparers =
            [
                new GeometryMultiPolygonComparer(RootComparerFactory.GetRootComparer()),
                new GeometryPointComparer(RootComparerFactory.GetRootComparer()),
                new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()),
                new GeometryMultiLineStringComparer(RootComparerFactory.GetRootComparer()),
                new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
            ]
        };
        var comparer = new CompareLogic(comparisonConfig);
        var result = comparer.Compare(
            expected,
            actual
        );

        var verificationResult = result.AreEqual
            ? VerificationResult.Pass()
            : VerificationResult.Fail(result.CreateDifferenceMessage(actual, expected));
        if (verificationResult.Failed)
        {
            throw CreateFailedScenarioExceptionFor(verificationResult);
        }
    }

    public async Task Expect(Func<IServiceProvider, IDocumentSession, Task> assert)
    {
        // Arrange
        // Dispose the service provider (and the Marten store it owns) and the projection daemon when done: the daemon
        // runs in the background, and if left running it keeps polling the shared (per test class) database and would
        // consume a following test's events under this test's projections, leaving that test's documents unprojected.
        await using var sp = BuildServiceProvider();

        // Apply the Marten schema via the SQL migrations (production runs AutoCreate.None).
        await sp.RunMartenDatabaseMigrationsAsync();

        var store = sp.GetRequiredService<IDocumentStore>();

        foreach (var events in _givenEvents)
        {
            await using var session = store.LightweightSession();

            var roadNetworkId = new ScopedRoadNetworkId(Guid.NewGuid());
            var roadNetworkStreamKey = StreamKeyFactory.Create(typeof(ScopedRoadNetwork), roadNetworkId);

            session.CausationId = "given";
            session.CorrelationId = roadNetworkStreamKey; // Ensure events are grouped by correlation id

            // Start each stream once; append any further events (including a second "created" event for the same
            // stream, which lets a test replay an "add" for an id that already exists in the same batch).
            var startedStreams = new HashSet<string>();
            foreach (var @event in events)
            {
                if (@event.Event is ICreatedEvent && startedStreams.Add(@event.StreamKey))
                {
                    session.Events.StartStream(@event.StreamKey, @event.Event);
                }
                else
                {
                    session.Events.Append(@event.StreamKey, @event.Event);
                }
            }

            session.Events.Append(roadNetworkStreamKey, new RoadNetworkWasChangedBecauseOfExtract
            {
                RoadNetworkId = roadNetworkId,
                Summary = new RoadNetworkChangedSummary(new RoadNetworkChangesSummary()),
                Provenance = new RoadRegistryProvenanceData()
            });

            await session.SaveChangesAsync();
        }

        // Act
        var projectionDaemon = await store.BuildProjectionDaemonAsync();
        try
        {
            await projectionDaemon.StartAllAsync();
            await projectionDaemon.WaitForNonStaleData(_projectionWaitTimeout);

            // Assert
            await using var session = store.LightweightSession();
            await assert(sp, session);
        }
        finally
        {
            // Stop (awaiting shutdown) before disposing so the background poller cannot outlive this test.
            await projectionDaemon.StopAllAsync();
            projectionDaemon.Dispose();
        }
    }

    public XunitException CreateFailedScenarioExceptionFor(VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", _givenEvents.SelectMany(x => x.Select(y => y.Event)), Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    private ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", _databaseFixture.ConnectionString)
            ])
            .Build();

        var services = new ServiceCollection();
        services
            .AddSingleton<IConfiguration>(configuration)
            .AddLogging();

        foreach (var configure in _servicesConfigurations)
        {
            configure(services);
        }

        services.AddMartenRoad(options =>
        {
            foreach (var configure in _storeConfigurations)
            {
                configure(options);
            }
        }).Services
        .AddMartenDatabaseMigrations();

        return services.BuildServiceProvider();
    }
}
