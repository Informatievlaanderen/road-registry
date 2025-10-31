﻿namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using KellermanSoftware.CompareNetObjects;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using Tests.Framework.Projections;
using Xunit.Sdk;

public class MartenProjectionTestRunner
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly List<Action<IServiceCollection>> _servicesConfigurations = [];
    private readonly List<Action<StoreOptions>> _storeConfigurations = [];
    private TimeSpan _projectionWaitTimeout;
    private readonly List<(string StreamKey, object[] Events)> _givenEvents = [];

    public MartenProjectionTestRunner(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
        _projectionWaitTimeout = TimeSpan.FromSeconds(5);
    }

    public MartenProjectionTestRunner ConfigureServices(Action<IServiceCollection> configure)
    {
        _servicesConfigurations.Add(configure);
        return this;
    }
    public MartenProjectionTestRunner ConfigureMarten(Action<StoreOptions> configure)
    {
        _storeConfigurations.Add(configure);
        return this;
    }
    public MartenProjectionTestRunner ProjectionWaitTimeout(TimeSpan projectionWaitTimeout)
    {
        _projectionWaitTimeout = projectionWaitTimeout;
        return this;
    }

    public MartenProjectionTestRunner Given(string streamKey, params object[] events)
    {
        _givenEvents.Add((streamKey, events));
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
        return Assert(async session =>
        {
            foreach (var expectedDocument in expectedDocuments)
            {
                var actualDocument = await session.LoadAsync<TProjectionEntity>(expectedDocument.Identifier);

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
                    expectedDocument.Document,
                    actualDocument
                );

                var verificationResult = result.AreEqual
                    ? VerificationResult.Pass()
                    : VerificationResult.Fail(result.CreateDifferenceMessage([actualDocument], [expectedDocument.Document]));
                if (verificationResult.Failed)
                {
                    var title = string.Empty;
                    var exceptionMessage = new StringBuilder()
                        .AppendLine(title)
                        .AppendTitleBlock("Given", _givenEvents.SelectMany(x => x.Events), Formatters.NamedJsonMessage)
                        .Append(verificationResult.Message);

                    throw new XunitException(exceptionMessage.ToString());
                }
            }
        });
    }

    public async Task Assert(Func<IDocumentSession, Task> assert)
    {
        // Arrange
        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();
        await using var session = store.LightweightSession();
        session.CausationId = Guid.NewGuid().ToString();

        foreach (var (streamKey, events) in _givenEvents)
        {
            foreach (var @event in events)
            {
                session.Events.AppendOrStartStream(streamKey, @event);
            }
        }
        await session.SaveChangesAsync();

        // Act
        var projectionDaemon = await store.BuildProjectionDaemonAsync();
        await projectionDaemon.StartAllAsync();
        await projectionDaemon.WaitForNonStaleData(_projectionWaitTimeout);

        // Assert
        await assert(session);
    }

    private IServiceProvider BuildServiceProvider()
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
        });

        return services.BuildServiceProvider();
    }
}

public static class MartenProjectionTestRunnerExtensions
{
    public static MartenProjectionTestRunner ConfigureRoadNetworkChangesProjection<TProjection>(
        this MartenProjectionTestRunner runner,
        Action<StoreOptions> configureProjection)
        where TProjection : IRoadNetworkChangesProjection, new()
    {
        return runner.ConfigureMarten(options =>
        {
            configureProjection(options);

            options.AddRoadNetworkChangesProjection(
                "projection_roadnetworkchanges",
                [new TProjection()]);
        });
    }

    public static MartenProjectionTestRunner Given<TEntity, TIdentifier>(this MartenProjectionTestRunner runner, TIdentifier identifier, params object[] events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        return runner.Given(StreamKeyFactory.Create(typeof(TEntity), identifier), events);
    }

    public static MartenProjectionTestRunner Given<TEntity, TIdentifier>(this MartenProjectionTestRunner runner, ICollection<(TIdentifier Identifier, object[] Events)> events)
        where TEntity : MartenAggregateRootEntity<TIdentifier>
    {
        foreach (var evt in events)
        {
            runner.Given<TEntity, TIdentifier>(evt.Identifier, evt.Events);
        }
        return runner;
    }
}
