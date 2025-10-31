namespace RoadRegistry.Tests.Framework.Projections;

using System.Text;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
using KellermanSoftware.CompareNetObjects;
using KellermanSoftware.CompareNetObjects.TypeComparers;
using Microsoft.EntityFrameworkCore;
using Producer.Snapshot.ProjectionHost.NationalRoad;
using Xunit.Sdk;

public class MemoryNationalRoadProducerSnapshotContext : NationalRoadProducerSnapshotContext
{
    public MemoryNationalRoadProducerSnapshotContext(DbContextOptions<NationalRoadProducerSnapshotContext> options) : base(options)
    {
    }
}

public static class NationalRoadProducerSnapshotContextScenarioExtensions
{
    //IMPORTANT: Each time you change the db sets on the context, you must adjust this method as well.
    //
    private static async Task<object[]> AllRecords(this NationalRoadProducerSnapshotContext context)
    {
        var records = new List<object>();
        records.AddRange(await context.NationalRoads.ToArrayAsync());

        return records.ToArray();
    }

    private static NationalRoadProducerSnapshotContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<NationalRoadProducerSnapshotContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryNationalRoadProducerSnapshotContext(options);
    }

    private static XunitException CreateFailedScenarioExceptionFor(this ConnectedProjectionTestSpecification<NationalRoadProducerSnapshotContext> specification, VerificationResult result)
    {
        var title = string.Empty;
        var exceptionMessage = new StringBuilder()
            .AppendLine(title)
            .AppendTitleBlock("Given", specification.Messages, Formatters.NamedJsonMessage)
            .Append(result.Message);

        return new XunitException(exceptionMessage.ToString());
    }

    public static Task Expect(
        this ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> scenario,
        DateTime created,
        IEnumerable<object> records)
    {
        return scenario.Expect(created, records.ToArray());
    }

    public static async Task Expect(
        this ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> scenario,
        DateTime created,
        params object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
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
            var actualRecords = await context.AllRecords();
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
            var projector = new ConnectedProjector<NationalRoadProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position }, { "CreatedUtc", created.ToUniversalTime() } }).ToGenericEnvelope();
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
        this ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> scenario,
        IEnumerable<object> records)
    {
        return scenario.ExpectInAnyOrder(records.ToArray());
    }

    public static async Task ExpectInAnyOrder(
        this ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> scenario,
        params object[] records)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var comparisonConfig = new ComparisonConfig { MaxDifferences = 5, IgnoreCollectionOrder = true };
            var comparer = new CompareLogic(comparisonConfig);
            var actualRecords = await context.AllRecords();
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
            var projector = new ConnectedProjector<NationalRoadProducerSnapshotContext>(specification.Resolver);
            var position = 0L;
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { "Position", position } }).ToGenericEnvelope();
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

    public static async Task ExpectNone(this ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> scenario)
    {
        var database = Guid.NewGuid().ToString("N");

        var specification = scenario.Verify(async context =>
        {
            var actualRecords = await context.AllRecords();
            return actualRecords.Length == 0
                ? VerificationResult.Pass()
                : VerificationResult.Fail($"Expected 0 records but found {actualRecords.Length}.");
        });

        await using (var context = CreateContextFor(database))
        {
            var projector = new ConnectedProjector<NationalRoadProducerSnapshotContext>(specification.Resolver);
            foreach (var message in specification.Messages)
            {
                var envelope = new Envelope(message, new Dictionary<string, object> { { "CreatedUtc", DateTime.Now } }).ToGenericEnvelope();
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

    public static ConnectedProjectionScenario<NationalRoadProducerSnapshotContext> Scenario(this ConnectedProjection<NationalRoadProducerSnapshotContext> projection)
    {
        return new ConnectedProjectionScenario<NationalRoadProducerSnapshotContext>(Resolve.WhenEqualToHandlerMessageType(projection.Handlers));
    }
}
