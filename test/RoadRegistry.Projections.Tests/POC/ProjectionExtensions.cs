namespace RoadRegistry.Projections.IntegrationTests.POC;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Infrastructure.MartenDb.Setup;
using JasperFx.Events;
using KellermanSoftware.CompareNetObjects;
using Marten;
using Tests.Framework.Projections;
using Xunit.Sdk;

public static class ProjectionExtensions
{
    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<IDocumentSession> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<IDocumentSession> scenario,
        IEnumerable<object> records)
    {
        return scenario.Expect(records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<IDocumentSession> scenario,
        params object[] records)
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        var specification = scenario.Verify(async _ =>
        {
            var comparisonConfig = new ComparisonConfig
            {
                MaxDifferences = 10,
                CustomComparers =
                [
                    new GeometryLineStringComparer(RootComparerFactory.GetRootComparer()),
                    new DateTimeComparer(RootComparerFactory.GetRootComparer()),
                    new GeometryPolygonComparer(RootComparerFactory.GetRootComparer())
                ]
            };
            var comparer = new CompareLogic(comparisonConfig);

            var actualRecords = store.AllRecords();
            var result = comparer.Compare(
                records,
                actualRecords
            );

            return result.AreEqual
                ? VerificationResult.Pass()
                : VerificationResult.Fail(result.CreateDifferenceMessage(actualRecords, records));
        });

        var projector = new ConnectedProjector<IDocumentSession>(specification.Resolver);
        var position = 0L;
        foreach (var message in specification.Messages)
        {
            var envelope = BuildEvent(message, position);
            await projector.ProjectAsync(store, envelope);
            position++;
        }

        var result = await specification.Verification(store, CancellationToken.None);

        if (result.Failed)
        {
            throw specification.CreateFailedScenarioExceptionFor(result);
        }
    }

    public static async Task ExpectNone(this ConnectedProjectionScenario<IDocumentSession> scenario)
    {
        var store = new InMemoryDocumentStoreSession(BuildStoreOptions());

        var specification = scenario.Verify(async context =>
        {
            var actualRecords = store.AllRecords();
            return actualRecords.Length == 0
                ? VerificationResult.Pass()
                : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
        });

        var projector = new ConnectedProjector<IDocumentSession>(specification.Resolver);
        foreach (var message in specification.Messages)
        {
            var envelope = BuildEvent(message);
            await projector.ProjectAsync(store, envelope);
        }

        var result = await specification.Verification(store, CancellationToken.None);

        if (result.Failed)
        {
            throw specification.CreateFailedScenarioExceptionFor(result);
        }
    }

    private static IEvent BuildEvent(object message, long version = 0)
    {
        var eventType = typeof(JasperFx.Events.Event<>).MakeGenericType(message.GetType());
        var evt = (JasperFx.Events.IEvent)Activator.CreateInstance(eventType, message)!;
        evt.Version = version;
        return evt;
    }

    public static ConnectedProjectionScenario<IDocumentSession> Scenario(this ConnectedProjection<IDocumentSession> projection)
    {
        return new ConnectedProjectionScenario<IDocumentSession>(Resolve.WhenAssignableToHandlerMessageType(projection.Handlers));
    }

    private static StoreOptions BuildStoreOptions()
    {
        var storeOptions = new StoreOptions();
        storeOptions.ConfigureRoad();
        RoadSegmentProjection.Configure(storeOptions);
        return storeOptions;
    }
}
