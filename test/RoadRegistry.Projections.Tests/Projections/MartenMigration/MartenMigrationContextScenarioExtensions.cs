namespace RoadRegistry.Projections.Tests.Projections.MartenMigration;

using System.Text;
using BackOffice.Core;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.EntityFrameworkCore;
using RoadNetwork.Events;
using RoadRegistry.MartenMigration.Projections;
using RoadRegistry.Tests.Framework.Projections;
using Xunit.Sdk;

public static class MartenMigrationContextScenarioExtensions
{
    private static MartenMigrationContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<MartenMigrationContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MartenMigrationContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<MartenMigrationContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) Given(
        this (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) scenarioAndStore,
        params object[] messages)
    {
        var (scenario, store) = scenarioAndStore;
        var result = scenario.Given(messages);
        return (result, store);
    }

    public static Task Expect(
        this (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) scenarioAndStore,
        IEnumerable<object> expectedEvents)
    {
        return scenarioAndStore.Expect(expectedEvents.ToArray());
    }

    public static async Task Expect(
        this (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) scenarioAndStore,
        params object[] expectedEvents)
    {
        var (scenario, store) = scenarioAndStore;
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async _ =>
        {
            var comparisonConfig = new ComparisonConfig
            {
                MaxDifferences = 10,
                CustomComparers = new List<BaseTypeComparer>
                {
                    new GeometryMultiPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPointComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPolygonComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryMultiLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer())
                }
            };
            var comparer = new CompareLogic(comparisonConfig);

            var actualRecords = store.AllEvents()
                .Where(x => x is not RoadNetworkChanged)
                .ToArray();
            var result = comparer.Compare(
                expectedEvents,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, expectedEvents));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<MartenMigrationContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = (message as Envelope
                               ?? new Envelope(message, new Dictionary<string, object>
                               {
                                   { Envelope.PositionMetadataKey, position },
                                   { Envelope.StreamIdMetadataKey, RoadNetworkStreamNameProvider.Default.ToString() },
                                   { Envelope.CreatedUtcMetadataKey, Moment.EnvelopeCreatedUtc.ToUniversalTime() },
                                   { Envelope.EventNameMetadataKey, message.GetType().Name },
                               })).ToGenericEnvelope();
                await projector.ProjectAsync(context, envelope);
                position++;
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContextFor(database))
        {
            var result = await specification.Verification(context, CancellationToken.None);

            if (result.Failed)
            {
                throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }
    }

    public static Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<MartenMigrationContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) scenarioAndStore,
        params object[] records)
    {
        var (scenario, store) = scenarioAndStore;
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async _ =>
        {
            var comparisonConfig = new ComparisonConfig { MaxDifferences = 5, IgnoreCollectionOrder = true };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = store.AllEvents()
                .Where(x => x is not RoadNetworkChanged)
                .ToArray();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<MartenMigrationContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = (message as Envelope
                                ?? new Envelope(message, new Dictionary<string, object>
                                {
                                    { Envelope.PositionMetadataKey, position },
                                    { Envelope.StreamIdMetadataKey, RoadNetworkStreamNameProvider.Default.ToString() },
                                    { Envelope.CreatedUtcMetadataKey, Moment.EnvelopeCreatedUtc.ToUniversalTime() },
                                    { Envelope.EventNameMetadataKey, message.GetType().Name },
                                })).ToGenericEnvelope();
                await projector.ProjectAsync(context, envelope);
                position++;
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContextFor(database))
        {
            var result = await specification.Verification(context, CancellationToken.None);

            if (result.Failed)
            {
                throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }
    }

    public static async Task ExpectNone(
        this (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) scenarioAndStore)
    {
        var (scenario, store) = scenarioAndStore;
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async _ =>
        {
            var actualRecords = store.AllEvents()
                .Where(x => x is not RoadNetworkChanged)
                .ToArray();
            return actualRecords.Length == 0
                ? VerificationResult.Pass()
                : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<MartenMigrationContext>(specification.Resolver);
            foreach (var message in specification.Messages)
            {
                var envelope = (message as Envelope
                               ?? new Envelope(message, new Dictionary<string, object>()))
                    .ToGenericEnvelope();
                await projector.ProjectAsync(context, envelope);
            }

            await context.SaveChangesAsync();
        }

        await using (var context = CreateContextFor(database))
        {
            var result = await specification.Verification(context, CancellationToken.None);

            if (result.Failed)
            {
                throw specification.CreateFailedScenarioExceptionFor(result);
            }
        }
    }

    public static (ConnectedProjectionScenario<MartenMigrationContext>, InMemoryDocumentStoreSession) Scenario(this (ConnectedProjection<MartenMigrationContext>, InMemoryDocumentStoreSession) projection)
    {
        return (new ConnectedProjectionScenario<MartenMigrationContext>(Resolve.WhenEqualToHandlerMessageType(projection.Item1.Handlers)), projection.Item2);
    }
}
